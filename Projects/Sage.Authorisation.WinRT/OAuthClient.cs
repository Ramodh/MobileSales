using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Sage.Authorisation.WinRT.Exceptions;
using Sage.Authorisation.WinRT.Storage;

namespace Sage.Authorisation.WinRT
{
    /// <summary>
    ///     Provides functionality to communicate with the Sage ID OAuth authorisation server
    ///     to obtain a set of credentials or tokens that will allow a client application to interact with a resource
    ///     application on behalf of a user.
    ///     <para>
    ///         The <see cref="OAuthClient">OAuthClient</see> class handles all the complications of OAuth 2.0, including
    ///         requesting and managing client credentials, refresh tokens,
    ///         access tokens and their expiry. It also presents the interactive user of the system with a secure dialog with
    ///         which to authenticate
    ///         with Sage ID if required. To help mitigate against spyware and malware, the authentication dialog is started in
    ///         its own Windows Desktop that is
    ///         independant of the desktop in which the current user is running their applications.
    ///     </para>
    /// </summary>
    /// <example>
    ///     <code>
    ///     string clientId = "yourclientidentifier";
    /// 
    ///     AuthorisationInfo startInfo = new AuthorisationInfo();
    ///     startInfo.Scope = "scopestring";
    ///     startInfo.State = "aaabbbccc";
    /// 
    ///     AuthorisationResult result = null;
    /// 
    ///     using(OAuthClient oauthClient = new OAuthClient(clientId))
    ///     {
    ///     result = oauthClient.Authorise(startInfo);
    ///     }
    ///   </code>
    /// </example>
    public sealed class OAuthClient
    {
        #region Fields

        private readonly CredentialStore _certificateStore;
        private readonly string _clientId;
        private readonly ResourceLoader _loader = new ResourceLoader("Sage.Authorisation.WinRT/Resources");
        private readonly Logger _log;
        private readonly TokenStore _tokenStore;
        private volatile bool _busy;
        private Configuration _configuration = new Configuration();
        private HttpHelper _httpHelper;
        private bool _suppressInteractive;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="OAuthClient" /> class.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        public OAuthClient(string clientId)
        {
            _clientId = clientId;

            // duplicating this initialisation to keep fields readonly
            _log = new Logger();
            _log.LogMessage += SurfaceLogEvent;

            _httpHelper = new HttpHelper(_log, _configuration);
            _tokenStore = new TokenStore(_clientId, _log, _httpHelper);
            _certificateStore = new CredentialStore(clientId, _httpHelper);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OAuthClient" /> class.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="supressInteractive">
        ///     if set to <c>true</c> then the client will throw an exception if the authorisation
        ///     needs user intervention (i.e. access token and refresh token are expired).
        /// </param>
        public OAuthClient(string clientId, bool supressInteractive)
        {
            _clientId = clientId;
            _suppressInteractive = supressInteractive;

            // duplicating this initialisation to keep fields readonly
            _log = new Logger();
            _log.LogMessage += SurfaceLogEvent;
            _tokenStore = new TokenStore(_clientId, _log, _httpHelper);
            _certificateStore = new CredentialStore(clientId, _httpHelper);
        }

        #endregion

        /// <summary>
        ///     <para>
        ///         The client ID you were provided with on registration. Please contact SSDPDeveloperSupport@sage.com to obtain a
        ///         valid client identifier.
        ///     </para>
        ///     <para>
        ///         This property can only be set before an initial call is made to either
        ///         <see cref="Sage.Authorisation.WinRT.OAuthClient.AuthoriseAsync">Authorise</see> or
        ///         <see cref="Sage.Authorisation.WinRT.OAuthClient.CleanupAsync">Cleanup</see>.
        ///     </para>
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        ///     Thrown if this property is being set after a call to
        ///     <see cref="Sage.Authorisation.WinRT.OAuthClient.AuthoriseAsync">Authorise</see> or
        ///     <see cref="Sage.Authorisation.WinRT.OAuthClient.CleanupAsync">Cleanup</see>.
        /// </exception>
        public string ClientID
        {
            get { return _clientId; }
        }

        /// <summary>
        ///     [OPTIONAL] Causes interactive authorisation with the logged on user to be suppressed.
        ///     <para>
        ///         Calls to <see cref="Sage.Authorisation.WinRT.OAuthClient.AuthoriseAsync">Authorise</see> will
        ///         return cached access tokens, or attempt to retrieve new access tokens using stored refefresh tokens, if
        ///         available. In the situation where no tokens can be retrieved
        ///         without prompting the interactive user for credentials or confirmation, and the SuppressInteractive property is
        ///         set to true,
        ///         <see cref="Sage.Authorisation.WinRT.Exceptions.InteractiveAuthorisationRequiredException">InteractiveAuthorisationRequiredException</see>
        ///         will be thrown. This may be useful behavior for a Windows Service, which has no GUI.
        ///     </para>
        ///     <para>
        ///         The default for this property is false.
        ///     </para>
        /// </summary>
        public bool SuppressInteractive
        {
            get { return _suppressInteractive; }
            set
            {
                if (_busy)
                {
                    throw new AuthorisationBusyException();
                }
                _suppressInteractive = value;
            }
        }

        /// <summary>
        ///     Event to notify clients whenever the Sage.Authorisation library wants to
        ///     log a message. Clients can subscribe to this event to include Sage.Authorisation
        ///     logging messages in their own logs.
        /// </summary>
        public event EventHandler<LogEvent> LogEvent;

        /// <summary>
        ///     Starts a new authorisation attempt with the Sage ID authorisation server.
        ///     <para>
        ///         The following operations are performed automatically for you, if required, when a call is made to Authorise:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>Calls Sage ID to retrieve a client credential (if not installed)</description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Stores a client credential (X.509 certificate and private key) in the windows certificate
        ///                     store
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>Calls returns a cached access token, if available, for the specified Scope and Client ID.</description>
        ///             </item>
        ///             <item>
        ///                 <description>Uses a refresh token to obtain a new access token if the cached access token has expired.</description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Presents a secure desktop to the interactive user to authenticate and authorise
        ///                     operations.
        ///                 </description>
        ///             </item>
        ///             <item>
        ///                 <description>Managed an encrypted store of refresh tokens.</description>
        ///             </item>
        ///             <item>
        ///                 <description>Refreshes the client credential.</description>
        ///             </item>
        ///         </list>
        ///         See the remarks section for more information.
        ///     </para>
        /// </summary>
        /// <param name="startInfo">A structure containing information that will be used to authorise the client.</param>
        /// <returns>
        ///     An AuthorisationResult object that contains information about whether or not the
        ///     authorisation attempt succeeded.
        /// </returns>
        /// <example>
        ///     <code>
        /// string clientId = "yourclientidentifier";
        /// AuthorisationInfo startInfo = new AuthorisationInfo();
        /// startInfo.ResponseType = "code";
        /// startInfo.Scope = "scopestring";
        /// startInfo.State = "aaabbbccc";
        /// AuthorisationResult result = null;
        /// using(OAuthClient oauthClient = new OAuthClient(clientId))
        /// {
        ///   result = oauthClient.Authorise(startInfo);
        ///   ILogEvent[] logEvents = oauthClient.GetLogEvents();
        /// }
        ///   </code>
        /// </example>
        /// <exception cref="Sage.Authorisation.WinRT.Exceptions.AuthorisationBusyException">
        ///     An authorisation attempt is already in
        ///     progress.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="startInfo" /> is null.</exception>
        /// <exception cref="Sage.Authorisation.WinRT.Exceptions.CommunicationException">
        ///     Sage.Authorisation library could not
        ///     communicate with the authorisation server.
        /// </exception>
        /// <exception cref="Sage.Authorisation.WinRT.Exceptions.ExpectedFoundException">
        ///     The authorisation server did not respond
        ///     with an expected HTTP 302 response.
        /// </exception>
        /// <exception cref="Sage.Authorisation.WinRT.Exceptions.ExpectedLocationHeaderException">
        ///     The authorisation server did not
        ///     include a location with an HTTP 302 response.
        /// </exception>
        /// <exception cref="Sage.Authorisation.WinRT.Exceptions.AuthorisationErrorResponseException">
        ///     The authorisation server
        ///     returned an error code during the authorisation process.
        /// </exception>
        /// <exception cref="Sage.Authorisation.WinRT.Exceptions.InteractiveAuthorisationRequiredException">
        ///     No cached access token
        ///     was available and no valid refresh token was stored for the current client ID and scope which would allow the
        ///     silent retrieval of a new access token.
        /// </exception>
        /// <remarks>
        ///     <para>
        ///         The Authorise method is used to attempt to obtain authorisation from a user via the
        ///         authorisation service.
        ///     </para>
        ///     <para>
        ///         An initial request is made to the authorisation service using the parameters that have
        ///         been supplied in the <paramref name="startInfo" /> parameter.  The supplied parameters will
        ///         be checked to ensure they are correct and valid.  If the parameters are correct then
        ///         a window will be opened on a secure desktop which will enable a user to log into Sage ID
        ///         and either grant or deny the permission sets that have been requested.
        ///     </para>
        ///     <para>Calling this method blocks the current thread until the authorisation process is complete.</para>
        ///     <para>
        ///         Information about the progress of an Authorise call is visible by subscribing to the
        ///         <see cref="LogEvent">LogEvent</see> event.
        ///     </para>
        /// </remarks>
        public IAsyncOperation<AuthorisationResult> AuthoriseAsync(AuthorisationInfo startInfo)
        {
            // Methods in the public interface can't return Task or Task<T> so they can't use async/await.
            // We can move all the work to a private method which can use await and wrap the result for winRT by calling .AsAsyncOperation()
            return DoAuthorisationAsync(startInfo).AsAsyncOperation();
        }

        /// <summary>
        ///     Performs a clean up of data stored by the OAuth Client by clearing cached access tokens,
        ///     refresh tokens, client credentials and other encrypted files from the secure storage area.
        /// </summary>
        /// <returns></returns>
        /// <example>
        ///     <code>
        /// string clientId = "yourclientidentifier";
        /// using(OAuthClient oauthClient = new OAuthClient(clientId))
        /// {
        ///   oauthClient.CleanUp();
        /// }
        ///   </code>
        /// </example>
        /// <exception cref="Sage.Authorisation.WinRT.Exceptions.AuthorisationBusyException">
        ///     An authorisation attempt is already in
        ///     progress.
        /// </exception>
        /// <remarks>
        ///     It is required that this method is called when your application is being uninstalled.
        /// </remarks>
        public IAsyncAction CleanupAsync()
        {
            if (_busy)
            {
                throw new AuthorisationBusyException();
            }

            //CredentialStore credentialStore = new CredentialStore(_clientId, new HttpHelper(_log, _configuration));

            //_log.Info(LogEventType.ClearingClientCredential, _loader.GetString("LogClearingClientCredential"));

            //credentialStore.ClearCredentialMetadata();

            return _tokenStore.CleanupAsync().AsAsyncAction();
        }

        /// <summary>
        ///     Override the default configuration with the values in the provided configuration object.
        /// </summary>
        /// <remarks>
        ///     The runtime component comes baked with the default settings for the environment it targets but it may be
        ///     required to override these settings, since Windows Store applications don't provide an app.config file we override
        ///     the defaults by providing a configuration object here.
        /// </remarks>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="Sage.Authorisation.WinRT.Exceptions.AuthorisationBusyException">
        ///     You can't override the configuration
        ///     while an authorisation is in progress.
        /// </exception>
        public void SetConfiguration(Configuration configuration)
        {
            if (_busy)
            {
                throw new AuthorisationBusyException(
                    _loader.GetString("ExceptionAuthorisationBusyException_SetConfiguration"));
            }
            _configuration = configuration;
            _httpHelper = new HttpHelper(_log, _configuration);
        }

        /// <summary>
        ///     Performs an interactive authorisation where the user logs in.
        /// </summary>
        /// <param name="currentAuthorisation">The current authorisation.</param>
        /// <returns></returns>
        /// <exception cref="AuthorisationException"></exception>
        /// <exception cref="AuthorisationErrorResponseException"></exception>
        /// <exception cref="System.Exception"></exception>
        private async Task<AuthorisationResult> AuthoriseUserAsync(AuthorisationInfo currentAuthorisation)
        {
            bool hasCert = _certificateStore.HasValidCredential();

            // gets uri using mutually authenticated initial call if necessary
            Uri brokerUri = await _httpHelper.StartAuthorisationAttemptAsync(currentAuthorisation, hasCert);

            Uri resultUri =
                await _httpHelper.AuthenticateUsingBrokerAsync(brokerUri, new Uri(_configuration.RedirectUri));

            var formDecoder = new WwwFormUrlDecoder(resultUri.Query);

            // match expecetd uris
            if (
                StringComparer.OrdinalIgnoreCase.Equals(
                    resultUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.SafeUnescaped)
                    , _configuration.RedirectUri))
            {
                _log.Diagnostic(LogEventType.RedirectUriFound,
                    String.Format(_loader.GetString("LogRedirectUriFound"), resultUri));

                // no parameters on returned uri
                if (resultUri.Query.Length == 0)
                {
                    throw new AuthorisationException(currentAuthorisation.State);
                }

                // returned uri contains error details
                if (formDecoder.Any(x => x.Name == Configuration.RedirectUriError))
                {
                    // if these other parameters are not present in the query this will throw an index out of bounds exception (or something like that)
                    // but they should always be here.
                    throw new AuthorisationErrorResponseException(currentAuthorisation.State
                        , formDecoder.GetFirstValueByName(Configuration.RedirectUriError)
                        , formDecoder.GetFirstValueByName(Configuration.RedirectUriErrorDescription));
                }

                IWwwFormUrlDecoderEntry accessCodeParameter =
                    formDecoder.FirstOrDefault(x => x.Name == _configuration.StartAuthorisationResponseType);
                if (accessCodeParameter == null || String.IsNullOrEmpty(accessCodeParameter.Value))
                {
                    // no authorisation result returned
                    throw new AuthorisationException(currentAuthorisation.State);
                }

                bool requestCredential = false;
                IWwwFormUrlDecoderEntry requestCredentialParameter =
                    formDecoder.FirstOrDefault(x => x.Name == Configuration.RedirectUriCred);
                if (requestCredentialParameter != null &&
                    bool.TryParse(requestCredentialParameter.Value, out requestCredential))
                {
                    await
                        _certificateStore.RetrieveAndImportCredentialAsync(accessCodeParameter.Value,
                            currentAuthorisation.DeviceName, currentAuthorisation.State);
                }

                AccessToken accessToken =
                    await
                        _tokenStore.GetAndStoreTokensAsync(accessCodeParameter.Value, currentAuthorisation.Scope,
                            _configuration.RedirectUri, currentAuthorisation.State);

                var result = new AuthorisationResult
                {
                    AccessToken = accessToken.Token,
                    Expiry = accessToken.ExpiryUtc,
                    State = currentAuthorisation.State,
                    Success = true,
                };

                return result;
            }

            // we should never reach this line as the authentication broker should always finish on a known uri.
            throw new Exception();
        }

        /// <summary>
        ///     Methods in the public interface can't return Task or Task&lt;T&gt; so they can't use async/await.
        ///     We can move all the work to a private method which can use await and wrap the result for winRT by calling
        ///     .AsAsyncOperation()
        /// </summary>
        /// <param name="authorisationInfo">The authorisation info.</param>
        /// <returns></returns>
        /// <exception cref="AuthorisationBusyException"></exception>
        private async Task<AuthorisationResult> DoAuthorisationAsync(AuthorisationInfo authorisationInfo)
        {
            if (_busy)
            {
                throw new AuthorisationBusyException();
            }

            try
            {
                if (_clientId == null)
                {
                    throw new InvalidOperationException("ClientId has not been set");
                }

                if (authorisationInfo == null)
                {
                    throw new ArgumentNullException("authorisationInfo");
                }

                _busy = true;

                authorisationInfo.ClientId = ClientID;

                // Initialise the return value for the Authorise call
                var currentResult = new AuthorisationResult
                {
                    State = authorisationInfo.State
                };

                AuthorisationResult result = null;
                if (authorisationInfo.ResetDuration)
                {
                    _log.Info(LogEventType.ResetDuration, _loader.GetString("LogResetTokenDuration"));

                    _tokenStore.ClearAccessToken(authorisationInfo.Scope);
                    _tokenStore.ClearRefreshToken(authorisationInfo.Scope);

                    // Launch new authorisation
                    result = await PerformInteractiveAuthorisationAsync(authorisationInfo);
                    return result;
                }

                // this uses the try-get pattern because it doesn't need to be async, it's just checking an in memory dictionary
                if (TryAuthoriseFromExistingAccessToken(authorisationInfo, out result))
                {
                    return result;
                }

                // this can't use the try-get pattern because async methods can't use out parameters.
                result = await TryAuthoriseFromRefreshTokenAsync(authorisationInfo);

                if (result == null)
                {
                    result = await PerformInteractiveAuthorisationAsync(authorisationInfo);
                }

                return result;
            }
            finally
            {
                _busy = false;
            }
        }

        /// <summary>
        ///     Performs an interactive authorisation unless the client has requested no interactive authorisations
        /// </summary>
        /// <exception cref="InteractiveAuthorisationRequiredException">
        ///     Client requested to SuppressInteractive authorisations but
        ///     an interactive authorisation is required.
        /// </exception>
        /// <param name="currentAuthorisationInfo"></param>
        /// <returns></returns>
        private async Task<AuthorisationResult> PerformInteractiveAuthorisationAsync(
            AuthorisationInfo currentAuthorisationInfo)
        {
            if (_suppressInteractive)
            {
                throw new InteractiveAuthorisationRequiredException(currentAuthorisationInfo.State);
            }

            _log.Info(LogEventType.InteractiveAuthentication, _loader.GetString("LogInteractiveAuthentication"));

            AuthorisationResult result = await AuthoriseUserAsync(currentAuthorisationInfo);

            return result;
        }

        /// <summary>
        ///     Surfaces the log event fired by the logger.
        /// </summary>
        /// <param name="e">The e.</param>
        private void SurfaceLogEvent(LogEvent e)
        {
            if (LogEvent != null)
            {
                LogEvent(this, e);
            }
        }

        /// <summary>
        ///     Tries to get an authorisation result from existing access token.
        /// </summary>
        /// <param name="currentAuthorisation">The current authorisation.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private bool TryAuthoriseFromExistingAccessToken(AuthorisationInfo currentAuthorisation,
            out AuthorisationResult result)
        {
            result = null;

            AccessToken token = _tokenStore.GetValidAccessToken(currentAuthorisation.Scope);

            if (token != null)
            {
                result = new AuthorisationResult();

                result.AccessToken = token.Token;
                result.Expiry = token.ExpiryUtc;
                result.Success = true;
            }

            return (token != null);
        }

        /// <summary>
        ///     Tries to get an authorisation result from existing refresh token.
        /// </summary>
        /// <param name="currentAuthorisationInfo">The current authorisation info.</param>
        /// <returns></returns>
        private async Task<AuthorisationResult> TryAuthoriseFromRefreshTokenAsync(
            AuthorisationInfo currentAuthorisationInfo)
        {
            try
            {
                AccessToken accessToken = await _tokenStore.RefreshAccessTokenAsync(currentAuthorisationInfo);

                if (accessToken != null)
                {
                    var result = new AuthorisationResult
                    {
                        AccessToken = accessToken.Token,
                        Expiry = accessToken.ExpiryUtc,
                        State = currentAuthorisationInfo.State,
                        Success = true,
                    };

                    return result;
                }
                return null;
            }
            catch (AuthorisationErrorResponseException ae)
            {
                //
                // Check if there is a problem with the refresh token
                //
                if (ae.Error == AuthorisationErrorResponseException.INVALID_GRANT)
                {
                    // deliberately not awaited
                    _tokenStore.ClearRefreshToken(currentAuthorisationInfo.Scope);
                    return null;
                }

                throw;
            }
        }
    }
}