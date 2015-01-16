using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Windows.ApplicationModel.Resources;
using Windows.Security.Authentication.Web;
using Windows.UI.Popups;
using Newtonsoft.Json;
using Sage.Authorisation.WinRT.Exceptions;
using Sage.Authorisation.WinRT.Storage;

namespace Sage.Authorisation.WinRT
{
    internal class HttpHelper
    {
        private readonly Configuration _configuration;
        private readonly ResourceLoader _loader;
        private readonly Logger _log;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HttpHelper" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="configuration">The configuration.</param>
        internal HttpHelper(Logger logger, Configuration configuration)
        {
            _log = logger;
            _configuration = configuration;
            _loader = new ResourceLoader("Sage.Authorisation.WinRT/Resources");
        }

        /// <summary>
        ///     Authenticates the using WebAutheticationBroker.
        /// </summary>
        /// <param name="startUri">The start URI.</param>
        /// <param name="endUri">The end URI.</param>
        /// <returns>
        ///     The final URI from the authentication broker with the query string
        /// </returns>
        /// <exception cref="CommunicationException">
        ///     Throws a CommunicationException if SageId returns a HTTP code other than 200
        ///     during the authorisation process
        /// </exception>
        /// <exception cref="AuthorisationException">
        ///     Throws an AuthorisationException if the user closes the web authentication
        ///     broker
        /// </exception>
        internal async Task<Uri> AuthenticateUsingBrokerAsync(Uri startUri, Uri endUri)
        {
            _log.Info(LogEventType.AuthenticateUsingBroker, _loader.GetString("LogAuthenticateUsingBroker"));

            var auth = await WebAuthenticationBroker.AuthenticateAsync(
                WebAuthenticationOptions.None
                , startUri
                , endUri);

            _log.Info(LogEventType.AuthenticateUsingBroker, _loader.GetString("LogAuthenticateUsingBrokerCompleted"));

            if (auth.ResponseStatus == WebAuthenticationStatus.Success)
            {
                return new Uri(auth.ResponseData);
            }
            if (auth.ResponseStatus == WebAuthenticationStatus.ErrorHttp)
            {
                // throw authentication failed exception
                throw new CommunicationException(_loader.GetString("ExceptionHttpErrorInAuthenticationBroker"));
            }
            // throw user cancelled exception
            throw new AuthorisationException(_loader.GetString("ExceptionUserClosedAuthenticationBroker"));
        }

        /// <summary>
        ///     Gets the client credential from Sage ID.
        /// </summary>
        /// <param name="accessCode">The access code.</param>
        /// <param name="password">The password.</param>
        /// <param name="deviceName">Name of the device.</param>
        /// <param name="state">The state.</param>
        /// <returns>CertificateMetadata containing the expiry and freidnly name of the certificate</returns>
        /// <exception cref="CommunicationException">
        ///     Throws a CommunicationException if SageID returns a HTTP BadRequest status
        ///     with error details
        /// </exception>
        /// <exception cref="ExpectedOKException">
        ///     Throws ExpectedOKException if SageID doesn't return any HTTP status other thab
        ///     BadRequest or Success when retreiving the certificate
        /// </exception>
        internal async Task<CertificateMetadata> GetClientCredentialAsync(string accessCode, string password,
            string deviceName, string state)
        {
            _log.Info(LogEventType.HttpGetClientCredential, _loader.GetString("LogHttpGetClientCredential"));

            HttpClient client = null;
            try
            {
                client = new HttpClient();

                var xmlDateTimeUtc = XmlConvert.ToString(DateTime.UtcNow, "yyyy-MM-ddTHH:mm:ss.fffffffK");

                var requestUri = new Uri(_configuration.GetClientCredentialUri, UriKind.Absolute);

                var postData = String.Format(_configuration.GetClientCredentialPostDataFormatter
                    , Uri.EscapeDataString(accessCode)
                    , Uri.EscapeDataString(Configuration.ClientCredentialFormat)
                    , Uri.EscapeDataString(password) // password 
                    , Uri.EscapeDataString(deviceName) // device name
                    , Uri.EscapeDataString(xmlDateTimeUtc)); // current client UTC time

                var result = await client.PostAsync(requestUri, new StringContent(postData));

                _log.Info(LogEventType.HttpGetClientCredential, _loader.GetString("LogHttpGetClientCredentialComplete"));

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await result.Content.ReadAsStringAsync();

                    var certData = ParseJsonString<CertificateMetadata>(responseString);

                    return certData;
                }

                if (result.StatusCode == HttpStatusCode.BadRequest)
                {
                    // TODO: throw better exception
                    // TODO: parse error data from JSON
                    throw new Exception();
                }

                //TODO: Throw better exception
                throw new Exception();
            }
            catch (WebException wex)
            {
                throw new CommunicationException(
                    String.Format(_loader.GetString("ExceptionShieldedWebException"), "GetClientCredential"), state, wex);
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                }
            }
        }

        /// <summary>
        ///     Exchanges an access code for refresh and access tokens
        /// </summary>
        /// <param name="accessCode">The access code.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        /// <exception cref="ExpectedOKException"></exception>
        /// <exception cref="CommunicationException"></exception>
        internal async Task<TokenResponse> ExchangeAccessCodeAsync(string accessCode, string state)
        {
            _log.Info(LogEventType.HttpGetTokens, _loader.GetString("LogHttpGetTokens"));

            HttpClientHandler handler = null;
            HttpClient client = null;
            try
            {
                handler = new HttpClientHandler();

                handler.ClientCertificateOptions = ClientCertificateOption.Automatic;

                client = new HttpClient(handler);

                var request = new HttpRequestMessage(HttpMethod.Post, _configuration.GetAccessTokenUri);

                var postData = String.Format(_configuration.GetAccessTokenPostDataFormatter
                    , Uri.EscapeDataString(Configuration.GetTokensGrantType)
                    , Uri.EscapeDataString(accessCode)
                    , Uri.EscapeDataString(_configuration.RedirectUri));

                request.Content = new StringContent(postData);

                var result = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                _log.Info(LogEventType.HttpGetTokensComplete, _loader.GetString("LogHttpGetTokensComplete"));

                var responseString = await result.Content.ReadAsStringAsync();

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var tokenResponse = TokenResponse.Parse(responseString);
                    return tokenResponse;
                }
                else
                {
                    await ShowMessageDialogAsync(result);
                }
                //if (result.StatusCode == HttpStatusCode.BadRequest)
                //{
                //    // throw exception containing details from response
                //    var error = ParseJsonString<ErrorResponse>(responseString);
                //    error.Throw(state);
                //}

                throw new ExpectedOKException(result.StatusCode, state);
            }
            catch (WebException wex)
            {
                throw new CommunicationException(
                    String.Format(_loader.GetString("ExceptionShieldedWebException"), "GetAccessToken"), state, wex);
            }
            finally
            {
                if (handler != null)
                {
                    handler.Dispose();
                }
                if (client != null)
                {
                    client.Dispose();
                }
            }
        }

        /// <summary>
        ///     Refreshes the access token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="state">The state.</param>
        /// <returns>
        ///     A TokenResponse containing an access token and refresh token based on the scope contained in the provided
        ///     RefreshToken
        /// </returns>
        internal async Task<TokenResponse> RefreshAccessTokenAsync(RefreshToken refreshToken, string state)
        {
            _log.Info(LogEventType.HttpGetTokens, _loader.GetString("LogHttpGetTokens"));

            HttpClientHandler handler = null;
            HttpClient client = null;
            try
            {
                handler = new HttpClientHandler();

                handler.ClientCertificateOptions = ClientCertificateOption.Automatic;

                client = new HttpClient(handler);

                var request = new HttpRequestMessage(HttpMethod.Post, _configuration.GetAccessTokenUri);

                var postData = String.Format(_configuration.RefreshAccessTokenPostDataFormatter
                    , Uri.EscapeDataString(Configuration.RefreshAccessTokenPostGrantType)
                    , Uri.EscapeDataString(refreshToken.Token)
                    , Uri.EscapeDataString(refreshToken.Scope));

                request.Content = new StringContent(postData);

                var result = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                _log.Info(LogEventType.HttpGetTokensComplete, _loader.GetString("LogHttpGetTokensComplete"));

                var responseString = await result.Content.ReadAsStringAsync();

                if (result.StatusCode == HttpStatusCode.BadRequest)
                {
                    // throw exception containing details from response
                    var error = ParseJsonString<ErrorResponse>(responseString);
                    error.Throw(state);
                }

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    var tokenResponse = TokenResponse.Parse(responseString);
                    return tokenResponse;
                }

                throw new ExpectedOKException(result.StatusCode, state);
            }
            catch (WebException wex)
            {
                throw new CommunicationException(
                    String.Format(_loader.GetString("ExceptionShieldedWebException"), "RefreshAccessToken"), state, wex);
            }
            finally
            {
                if (handler != null)
                {
                    handler.Dispose();
                }
                if (client != null)
                {
                    client.Dispose();
                }
            }
        }

        /// <summary>
        ///     Checks to see if we already have a valid cert and if we do, starts an authorisation.
        ///     If we don't have a cert this method simpyly returns the start URI to be used directly by the
        ///     WebAuthenticationBroker
        /// </summary>
        /// <param name="startInfo">The start info.</param>
        /// <param name="hasCert">Use <c>true</c> if there is a valid client credential.</param>
        /// <returns>A uri to be passed to the WebAuthenticationBroker</returns>
        /// <exception cref="System.ArgumentNullException">startInfo</exception>
        internal async Task<Uri> StartAuthorisationAttemptAsync(AuthorisationInfo startInfo, bool hasCert)
        {
            #region Validation

            if (startInfo == null)
            {
                throw new ArgumentNullException("startInfo");
            }

            #endregion

            _log.Info(LogEventType.HttpStartAuthorisation, _loader.GetString("LogHttpStartAuthorisation"));

            var uri = startInfo.GetTemplatedUri(hasCert, _configuration);

            if (hasCert)
            {
                uri = await StartAuthenticatedAttemptAsync(uri, startInfo);
            }

            return uri;
        }

        /// <summary>
        ///     Parses a json string and return an instance of T.
        /// </summary>
        /// <typeparam name="T">The expected type described by the JSON string</typeparam>
        /// <param name="jsonString">The json String.</param>
        /// <returns></returns>
        private static T ParseJsonString<T>(string jsonString)
        {
            var settings = new JsonSerializerSettings();

            settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            settings.DateParseHandling = DateParseHandling.DateTime;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            var result = JsonConvert.DeserializeObject<T>(jsonString, settings);
            return result;
        }

        /// <summary>
        ///     Makes the actually HTTP request to SageID to start the authenticated attempt (when a credential is present)
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <param name="startInfo">The start info.</param>
        /// <returns></returns>
        /// <exception cref="ExpectedFoundException"></exception>
        /// <exception cref="ExpectedLocationHeaderException"></exception>
        /// <exception cref="CommunicationException"></exception>
        private async Task<Uri> StartAuthenticatedAttemptAsync(Uri uri, AuthorisationInfo startInfo)
        {
            HttpClientHandler handler = null;
            HttpClient client = null;
            HttpRequestMessage request = null;
            try
            {
                handler = new HttpClientHandler();

                handler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                handler.AllowAutoRedirect = false;

                client = new HttpClient(handler);
                request = new HttpRequestMessage(HttpMethod.Get, uri);

                var result = await client.SendAsync(request);

                if (result.StatusCode != HttpStatusCode.Found)
                {
                    throw new ExpectedFoundException(result.StatusCode, startInfo.State);
                }

                if (null == result.Headers.Location)
                {
                    throw new ExpectedLocationHeaderException(startInfo.State);
                }

                return result.Headers.Location;
            }
            catch (WebException wex)
            {
                throw new CommunicationException(
                    String.Format(_loader.GetString("ExceptionShieldedWebException"), "StartAuthorisationAttempt"),
                    startInfo.State, wex);
            }
            finally
            {
                if (handler != null)
                {
                    handler.Dispose();
                }
                if (client != null)
                {
                    client.Dispose();
                }
                if (request != null)
                {
                    request.Dispose();
                }
            }
        }

        private async Task ShowMessageDialogAsync(HttpResponseMessage result)
        {
            string errorText;
            string errorTitle;

            switch (result.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    errorText = "There was a problem contacting the sign in server.";
                    errorTitle = "Unable to sign in";
                    break;

                case HttpStatusCode.Unauthorized:
                case HttpStatusCode.Forbidden:
                    errorText = "The user has not been granted sufficient permission to proceed";
                    errorTitle = "Unable to sign in";
                    break;

                case HttpStatusCode.NotFound:
                    errorText = "Verify the correct server is selected in app settings";
                    errorTitle = "Unable to connect to the site";
                    break;

                case HttpStatusCode.InternalServerError:
                    errorText = "Please try again in a few minutes";
                    errorTitle = "Cannot contact server";
                    break;

                default:
                    errorText = "There was a problem contacting the sign in server.";
                    errorTitle = "Unable to sign in";
                    break;
            }

            var msgDialog = new MessageDialog(errorText, errorTitle);
            await msgDialog.ShowAsync();
        }
    }
}