using System;
using System.Text;

namespace Sage.Authorisation.WinRT
{
    /// <summary>
    ///     The parameters required to start a authorisation attempt with
    ///     an authorisation server.
    /// </summary>
    /// <example>
    ///     <code>
    ///     AuthorisationInfo startInfo = new AuthorisationInfo()
    ///     {
    ///     Scope = "yourscopegoeshere",
    ///     State = "optional state string"
    ///     };
    ///   </code>
    /// </example>
    public sealed class AuthorisationInfo
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AuthorisationInfo" /> class.
        /// </summary>
        public AuthorisationInfo()
        {
            ResponseType = Configuration.StartAuthorisationResponseTypeDefault;
            State = String.Empty;
            DeviceName = String.Empty;
        }

        /// <summary>
        ///     [OPTIONAL] The ResetDuration flag, if set to true, forces an interactive authentication and allows
        ///     the <see cref="Sage.Authorisation.WinRT.OAuthClient">AuthoriseAsync</see> call to return tokens for the longest
        ///     possible time
        ///     permitted by the scope.
        /// </summary>
        /// <remarks>This parameter is optional and will default to false.</remarks>
        public bool ResetDuration { get; set; }

        /// <summary>
        ///     [OPTIONAL] Must be set to "code"
        /// </summary>
        public string ResponseType { get; set; }

        /// <summary>
        ///     The permission sets being requested expressed as a String.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        ///     [OPTIONAL] Client supplied state that will be returned back to the client
        ///     after an authorisation attempt.
        /// </summary>
        /// <remarks>This parameter is optional</remarks>
        public string State { get; set; }

        /// <summary>
        ///     [OPTIONAL] The name of the device that will be recorded for this client.
        /// </summary>
        /// <remarks>This parameter is optional</remarks>
        public string DeviceName { get; set; }

        /// <summary>
        ///     The client ID provided on registration
        /// </summary>
        internal string ClientId { get; set; }

        /// <summary>
        ///     Gets the templated URI.
        /// </summary>
        /// <param name="hasClientCredential">if set to <c>true</c> [has client credential].</param>
        /// <param name="config">The config.</param>
        /// <returns></returns>
        internal Uri GetTemplatedUri(bool hasClientCredential, Configuration config)
        {
            var template =
                new StringBuilder(hasClientCredential
                    ? config.StartAuthorisationAttemptWithCredentialUri
                    : config.StartAuthorisationAttemptUri);

            template.Replace("{RESPONSETYPE}", Uri.EscapeDataString(ResponseType));
            template.Replace("{CLIENTID}", Uri.EscapeDataString(ClientId));
            template.Replace("{REDIRECTURI}", Uri.EscapeDataString(config.RedirectUri));
            template.Replace("{SCOPE}", Uri.EscapeDataString(Scope));
            template.Replace("{STATE}", Uri.EscapeDataString(State));

            return new Uri(template.ToString());
        }
    }
}