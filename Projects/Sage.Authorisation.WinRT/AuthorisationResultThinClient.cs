namespace Sage.Authorisation.WinRT
{
    /// <summary>
    ///     This class encapsulates the result of a partial authorisation attempt.
    ///     <para>
    ///         It contains the access code and a boolean to specify if a new client credential is required.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     This partial result should be used when SageID OAuth is being used in a "local" client-server scenario.
    /// </remarks>
    public sealed class AuthorisationResultThinClient
    {
        /// <summary>
        ///     Encrypted access token returned from Sage ID servers. This can not be decrypted by the client application.
        /// </summary>
        public string AuthorisationCode { get; private set; }

        /// <summary>
        ///     The exception that occurred if Success is false.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether the server needs to refresh it's client credential.
        /// </summary>
        /// <value>
        ///     <c>true</c> if the server needs to refresh it's client credential; otherwise, <c>false</c>.
        /// </value>
        public bool RefreshCredential { get; private set; }

        /// <summary>
        ///     Gets whether or not the authorisation attempt succeeded.
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        ///     Creates an AuthorisationClientResultThin instance
        /// </summary>
        /// <param name="success">if set to <c>true</c> authorisation was a success.</param>
        /// <param name="authorisationCode">The authorisation code.</param>
        /// <param name="refreshCredential">if set to <c>true</c> credential should be refreshed.</param>
        /// <returns>
        ///     An initialised AuthorisationClientResultThin
        /// </returns>
        internal static AuthorisationResultThinClient CreateResult(bool success, string authorisationCode,
            bool refreshCredential)
        {
            var result = new AuthorisationResultThinClient
            {
                Success = success,
                AuthorisationCode = authorisationCode,
                RefreshCredential = refreshCredential
            };

            return result;
        }
    }
}