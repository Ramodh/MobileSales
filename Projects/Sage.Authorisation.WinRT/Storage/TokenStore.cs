using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;

namespace Sage.Authorisation.WinRT.Storage
{
    /// <summary>
    ///     TokenStore handles retrieval and secure storage of RefreshTokens and AccessTokens
    /// </summary>
    internal class TokenStore
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TokenStore" /> class.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="httpHelper">The HTTP helper.</param>
        internal TokenStore(string clientId, Logger logger, HttpHelper httpHelper)
        {
            _clientId = clientId;
            _logger = logger;
            _httpHelper = httpHelper;
        }

        /// <summary>
        ///     Clear in memory cache of access tokens and clear secure storage
        /// </summary>
        internal async Task CleanupAsync()
        {
            _accessTokens.Clear();

            var tokenFolder = await GetRefreshTokenFolderAsync();

            await tokenFolder.DeleteAsync();

            _logger.Info(LogEventType.ClearedSecureStorage,
                String.Format(_loader.GetString("LogClearStorage"), tokenFolder.Path));
        }

        /// <summary>
        ///     Clear an access token from the cache.
        /// </summary>
        /// <param name="scope">The scope with which to search for a refresh token</param>
        internal void ClearAccessToken(string scope)
        {
            var hashedScope = HashScope(scope);

            _accessTokens.Remove(hashedScope);
        }

        /// <summary>
        ///     Clear a refresh token for the specified scope and current client ID
        /// </summary>
        /// <param name="scope">The scope with which to search for a refresh token</param>
        internal void ClearRefreshToken(string scope)
        {
            var hashedScope = HashScope(scope);

            ClearValueAsync(String.Format(RefreshTokenFileNameFormat, hashedScope));
        }

        /// <summary>
        ///     Retrieves the access tokens from SageID and stores them securely.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="scope"></param>
        /// <param name="redirectUri"></param>
        /// <param name="state"></param>
        /// <returns>The access token for the requested scope</returns>
        internal async Task<AccessToken> GetAndStoreTokensAsync(string code, string scope, string redirectUri,
            string state)
        {
            #region Validation

            if (String.IsNullOrEmpty(code))
            {
                throw new ArgumentNullException("code");
            }

            if (_httpHelper == null)
            {
                throw new InvalidOperationException(_loader.GetString("ExceptionTokenStoreHttpHelperNull"));
            }

            #endregion

            var response = await _httpHelper.ExchangeAccessCodeAsync(code, redirectUri);

            // Store refresh token in encrypted store.
            var refreshToken = new RefreshToken
            {
                Token = response.Refresh_Token,
                RetrievedUtc = DateTime.UtcNow,
                Scope = scope
            };

            // Store the access token in memory
            var accessToken = new AccessToken
            {
                Token = response.Access_Token,
                ExpiryUtc = DateTime.UtcNow.AddSeconds(response.Expires_In),
                RetrievedUtc = DateTime.UtcNow,
                Scope = scope
            };

            // these are not awaited as we don't need to wait for them to complete before we return the token
            // we may want to change this later.
            StoreRefreshTokenAsync(refreshToken);
            StoreAccessToken(accessToken);

            return accessToken;
        }

        /// <summary>
        ///     Read a refresh token for the specified scope and current client ID
        /// </summary>
        /// <param name="scope">The scope with which to search for a refresh token</param>
        /// <returns>True if refresh token was found</returns>
        internal async Task<RefreshToken> GetRefreshTokenAsync(string scope)
        {
            var hashedScope = HashScope(scope);

            var token =
                await GetValueAsync<RefreshToken>(String.Format(RefreshTokenFileNameFormat, hashedScope));

            return token;
        }

        /// <summary>
        ///     Gets a valid access token if one exists for the given scope
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns>The access token or null</returns>
        internal AccessToken GetValidAccessToken(string scope)
        {
            var token = GetAccessToken(scope);

            if (token != null)
            {
                if (!token.Expired)
                {
                    // token is present and in-date, return it and don't continue
                    return token;
                }

                // clear token and return null if token is expired
                ClearAccessToken(scope);
            }

            return null;
        }

        /// <summary>
        ///     Refreshes an access token, stores the returned tokens and returns the new access token
        /// </summary>
        /// <param name="currentAuthorisationInfo"></param>
        /// <returns></returns>
        internal async Task<AccessToken> RefreshAccessTokenAsync(AuthorisationInfo currentAuthorisationInfo)
        {
            var refreshToken = await GetRefreshTokenAsync(currentAuthorisationInfo.Scope);

            if (refreshToken == null)
            {
                return null;
            }

            if (_httpHelper == null)
            {
                throw new InvalidOperationException(_loader.GetString("ExceptionTokenStoreHttpHelperNull"));
            }

            var response =
                await _httpHelper.RefreshAccessTokenAsync(refreshToken, currentAuthorisationInfo.State);

            // Store refresh token in encrypted store.
            var newRefreshToken = new RefreshToken
            {
                Token = response.Refresh_Token,
                RetrievedUtc = DateTime.UtcNow,
                Scope = currentAuthorisationInfo.Scope
            };

            // Store the access token in memory
            var accessToken = new AccessToken
            {
                Token = response.Access_Token,
                ExpiryUtc = DateTime.UtcNow.AddSeconds(response.Expires_In),
                RetrievedUtc = DateTime.UtcNow,
                Scope = currentAuthorisationInfo.Scope
            };

            // these are not awaited as we don't need to wait for them to complete before we return the token
            // we may want to change this later.
            StoreRefreshTokenAsync(newRefreshToken);
            StoreAccessToken(accessToken);

            return accessToken;
        }

        /// <summary>
        ///     Gets the refresh token folder from application local storage.
        /// </summary>
        /// <returns></returns>
        private static async Task<StorageFolder> GetRefreshTokenFolderAsync()
        {
            var tokenFolder =
                await
                    ApplicationData.Current.LocalFolder.CreateFolderAsync(RefreshTokenFolderName,
                        CreationCollisionOption.OpenIfExists);
            return tokenFolder;
        }

        /// <summary>
        ///     Clears a value from the application data file storage.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private async void ClearValueAsync(string key)
        {
            var tokenFolder = await GetRefreshTokenFolderAsync();
            var file = await tokenFolder.GetFileAsync(key);
            await file.DeleteAsync();
        }

        /// <summary>
        ///     Read a refresh token for the specified scope and current client ID
        /// </summary>
        /// <param name="scope">The scope with which to search for a refresh token</param>
        /// <returns>
        ///     True if valid AccessToken found
        /// </returns>
        private AccessToken GetAccessToken(string scope)
        {
            var hashedScope = HashScope(scope);
            AccessToken token = null;

            if (_accessTokens.ContainsKey(hashedScope))
            {
                token = _accessTokens[hashedScope];
            }

            return token;
        }

        /// <summary>
        ///     Gets the file identified by the key from local storage and deserializes it.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>The instance of T which was stored in the local storage</returns>
        private async Task<T> GetValueAsync<T>(string key) where T : Token
        {
            _logger.Diagnostic(LogEventType.ReadValue, String.Format(_loader.GetString("LogReadValue"), key));

            try
            {
                var tokenFolder = await GetRefreshTokenFolderAsync();
                var sessionFile = await tokenFolder.GetFileAsync(key);

                var serializer = new DataContractSerializer(typeof (T));

                var refreshTokenFileBuffer = await FileIO.ReadBufferAsync(sessionFile);
                var refreshTokenDecrypted = await _encryptor.DecryptAsync(refreshTokenFileBuffer);
                var binaryToken = CryptographicBuffer.ConvertStringToBinary(refreshTokenDecrypted,
                    BinaryStringEncoding.Utf8);
                var objectStream = binaryToken.AsStream();
                var token1 = (T) serializer.ReadObject(objectStream);
                return token1;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Stores the token in local file storage using the key as the file name
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="_data">The _data.</param>
        /// <returns>Task for async awaitableness</returns>
        private async Task StoreValueAsync(string key, Token _data)
        {
            _logger.Diagnostic(LogEventType.PersistValue, String.Format(_loader.GetString("LogPersistValue"), key));

            var tokenFolder = await GetRefreshTokenFolderAsync();
            var sessionFile = await tokenFolder.CreateFileAsync(key, CreationCollisionOption.ReplaceExisting);

            var sessionSerializer = new DataContractSerializer(typeof (Token));
            var stream = new MemoryStream();
            sessionSerializer.WriteObject(stream, _data);

            // reset the stream back to the start so we can read it
            stream.Position = 0;
            var sr = new StreamReader(stream);
            var encrypted = await _encryptor.EncryptAsync(sr.ReadToEnd());

            await FileIO.WriteTextAsync(sessionFile, encrypted);
        }

        /// <summary>
        ///     Hashes the scope and encodes it in a safe encoding for file names.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        private string HashScope(string scope)
        {
            var p = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var hashBuffer = p.HashData(CryptographicBuffer.ConvertStringToBinary(scope, BinaryStringEncoding.Utf8));

            // Base32 encode this, base64 might have invalid characters
            var encoded = Base32Encoder.Base32Encode(hashBuffer);
            return encoded;
        }

        /// <summary>
        ///     Store a refresh token against a scope for the current client ID
        /// </summary>
        /// <param name="token">the refresh token instance</param>
        private void StoreAccessToken(AccessToken token)
        {
            var hashedScope = HashScope(token.Scope);

            _accessTokens[hashedScope] = token;
        }

        /// <summary>
        ///     Store a refresh token against a scope for the current client ID
        /// </summary>
        /// <param name="refreshToken">the refresh token instance</param>
        private async void StoreRefreshTokenAsync(RefreshToken refreshToken)
        {
            var hashedScope = HashScope(refreshToken.Scope);

            await StoreValueAsync(String.Format(RefreshTokenFileNameFormat, hashedScope), refreshToken);
        }

        #region Fields

        private const string RefreshTokenFileNameFormat = "{0}.rt";
        private const string RefreshTokenFolderName = "SIDRTS";

        /// <summary>
        ///     Static dictionary of access tokens
        /// </summary>
        private static readonly Dictionary<string, AccessToken> _accessTokens = new Dictionary<string, AccessToken>();

        private readonly string _clientId;

        private readonly SymmetricEncryptor _encryptor = new SymmetricEncryptor();
        private readonly HttpHelper _httpHelper;
        private readonly ResourceLoader _loader = new ResourceLoader("Sage.Authorisation.WinRT/Resources");
        private readonly Logger _logger;

        #endregion
    }
}