using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Sage.Authorisation.WinRT.Exceptions;

namespace Sage.Authorisation.WinRT
{
    /// <summary>
    ///     Provides functionality to communicate with the Sage ID OAuth authorisation server
    ///     to obtain an authorisation code.
    ///     <para>
    ///         The <see cref="OAuthClientThin">OAuthClientThin</see> class is a cut down version of the OAuthClient. It
    ///         presents the interactive
    ///         user of the system with a secure dialog with which to authenticate. To help mitigate against spyware and
    ///         malware, the authentication dialog
    ///         is started in its own Windows Desktop that is independant of the desktop in which the current user is running
    ///         their applications.
    ///     </para>
    /// </summary>
    /// <example>
    ///     <code>
    /// string uri = "URI RETURNED FROM BEGIN AUTHORISATION ATTEMPT";
    /// string clientId = "yourclientidentifier";
    /// IAuthorisationResultThin result = null;
    /// using(IOauthThin oauthClientThin = new OAuthClientThin(clientId))
    /// {
    ///     result = oauthClientThin.Authorise(uri);
    /// }
    ///   </code>
    /// </example>
    public sealed class OAuthClientThin
    {
        private readonly ResourceLoader _loader = new ResourceLoader("Sage.Authorisation.WinRT/Resources");
        private readonly Logger _log;
        private bool _busy;
        private Configuration _configuration = new Configuration();
        private HttpHelper _httpHelper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="OAuthClientThin" /> class.
        /// </summary>
        public OAuthClientThin()
        {
            _log = new Logger();
            _log.LogMessage += SurfaceLogEvent;
            _httpHelper = new HttpHelper(_log, _configuration);
        }

        /// <summary>
        ///     Performs the interactive part of an authorisation.
        /// </summary>
        /// <param name="startUri">The URI which was returned by a call to BeginAuthorisationAttempt.</param>
        /// <returns>
        ///     An IAuthorisationResultThin object that contains information about the authorisation attempt.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         In the thin client flow the BeginAuthorisationAttempt step isn't performed by the client, some other component
        ///         performs this step and passes the URI to this method.
        ///     </para>
        ///     <para>
        ///         The result of this authorise call (the access code) should be passed back to the other component to obtain it's
        ///         tokens and credentials.
        ///     </para>
        ///     <para>Calling this method blocks the current thread until the authorisation process is complete.</para>
        /// </remarks>
        public IAsyncOperation<AuthorisationResultThinClient> AuthoriseAsync(string startUri)
        {
            return DoAuthoriseAsync(startUri).AsAsyncOperation();
        }

        /// <summary>
        ///     Override the default configuration with the values in the provided configuration object.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <exception cref="AuthorisationBusyException"></exception>
        /// <exception cref="Sage.Authorisation.WinRT.Exceptions.AuthorisationBusyException">
        ///     You can't override the configuration
        ///     while an authorisation is in progress.
        /// </exception>
        /// <remarks>
        ///     The runtime component comes baked with the default settings for the environment it targets but it may be required
        ///     to override these settings, since Windows Store applications don't provide an app.config file we override the
        ///     defaults by providing a configuration object here.
        /// </remarks>
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
        ///     Event to notify clients whenever the Sage.Authorisation library wants to
        ///     log a message. Clients can subscribe to this event to include Sage.Authorisation
        ///     logging messages in their own logs.
        /// </summary>
        public event EventHandler<LogEvent> LogEvent;

        /// <summary>
        ///     Performs the interactive part of an authorisation.
        /// </summary>
        /// <param name="startUriString">The start URI retrieved from the server component.</param>
        /// <returns>
        ///     An IAuthorisationResultThin object that contains information about the authorisation attempt.
        /// </returns>
        /// <exception cref="AuthorisationBusyException"></exception>
        /// <exception cref="AuthorisationException"></exception>
        /// <exception cref="AuthorisationErrorResponseException">STATE</exception>
        /// <exception cref="System.Exception"></exception>
        /// <remarks>
        ///     <para>
        ///         In the thin client flow the BeginAuthorisationAttempt step isn't performed by the client, some other component
        ///         performs this step and passes the URI to this method.
        ///     </para>
        ///     <para>
        ///         The result of this authorise call (the access code) should be passed back to the other component to obtain it's
        ///         tokens and credentials.
        ///     </para>
        ///     <para>Calling this method blocks the current thread until the authorisation process is complete.</para>
        /// </remarks>
        private async Task<AuthorisationResultThinClient> DoAuthoriseAsync(string startUriString)
        {
            if (_busy)
            {
                throw new AuthorisationBusyException();
            }
            _busy = true;

            var startUri = new Uri(startUriString);

            var resultUri =
                await _httpHelper.AuthenticateUsingBrokerAsync(startUri, new Uri(_configuration.RedirectUri));

            var formDecoder = new WwwFormUrlDecoder(resultUri.Query);

            // match expecetd uri
            if (StringComparer.OrdinalIgnoreCase.Equals(
                resultUri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.SafeUnescaped)
                , _configuration.RedirectUri))
            {
                _log.Diagnostic(LogEventType.RedirectUriFound,
                    String.Format(_loader.GetString("LogRedirectUriFound"), resultUri));

                // no parameters on returned uri
                if (resultUri.Query.Length == 0)
                {
                    throw new AuthorisationException();
                }

                // returned uri contains error details
                if (formDecoder.Any(x => x.Name == Configuration.RedirectUriError))
                {
                    // if these other parameters are not present in the query this will throw an index out of bounds exception (or something like that)
                    // but they should always be there.
                    throw new AuthorisationErrorResponseException("STATE"
                        , formDecoder.GetFirstValueByName(Configuration.RedirectUriError)
                        , formDecoder.GetFirstValueByName(Configuration.RedirectUriErrorDescription));
                }

                var accessCodeParameter =
                    formDecoder.FirstOrDefault(x => x.Name == _configuration.StartAuthorisationResponseType);
                if (accessCodeParameter == null || String.IsNullOrEmpty(accessCodeParameter.Value))
                {
                    // no authorisation result returned
                    throw new AuthorisationException();
                }

                var requestCredential = false;
                var requestCredentialParameter =
                    formDecoder.FirstOrDefault(x => x.Name == Configuration.RedirectUriCred);
                bool.TryParse(requestCredentialParameter.Value, out requestCredential);

                var result = AuthorisationResultThinClient.CreateResult(true,
                    accessCodeParameter.Value, requestCredential);

                _busy = false;
                return result;
            }
            _busy = false;
            throw new Exception();
        }

        /// <summary>
        ///     Surfaces the log event from the logger.
        /// </summary>
        /// <param name="e">The event.</param>
        private void SurfaceLogEvent(LogEvent e)
        {
            if (LogEvent != null)
            {
                LogEvent(this, e);
            }
        }
    }
}