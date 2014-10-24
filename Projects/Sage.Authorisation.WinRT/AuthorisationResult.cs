using System;

namespace Sage.Authorisation.WinRT
{
    /// <summary>
    ///     This class encapsulates the result of the authorisation attempt.
    ///     <para>
    ///         It contains the encrypted AccessToken, its expiry, and also the user state which was specified when the
    ///         Authorise method was called with
    ///         an instance of <see cref="AuthorisationInfo">AuthorisationInfo</see>.
    ///     </para>
    /// </summary>
    /// <example>
    ///     The following code snippet shows how to make a call, via REST, using the encrypted access token in an
    ///     <see cref="AuthorisationResult">AuthorisationResult.</see>
    ///     <code>
    ///   <summary>
    ///             Make a call to a resource application using REST and authorisation using an http header
    ///         </summary>
    ///   public string UseAccessTokenRestHeader(AuthorisationResult result)
    ///   {
    ///       HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://resourceapplicationurl");
    ///       request.Method = "GET";
    ///       request.Headers.Add(HttpRequestHeader.Authorisation, "Bearer " + result.AccessToken);
    /// 
    ///       HttpWebResponse response;
    ///       try
    ///       {
    ///           response = (HttpWebResponse)request.GetResponse();
    ///       }
    ///       catch (WebException we)
    ///       {
    ///           response = (HttpWebResponse)we.Response;
    ///       }
    /// 
    ///       string resultMessage = (new StreamReader(response.GetResponseStream()).ReadToEnd());
    /// 
    ///       return resultMessage;
    ///   }
    /// 
    /// </code>
    /// </example>
    /// <remarks>
    ///     The <see cref="Sage.Authorisation.WinRT.AuthorisationResult.Expiry">Expiry</see> property gives the expiration of
    ///     the access token in UTC.
    ///     <para>
    ///         It is not possible for the client application to decrypt the AccessToken. The encryption key is known only to
    ///         the resource web application.
    ///     </para>
    /// </remarks>
    public sealed class AuthorisationResult
    {
        /// <summary>
        ///     Gets whether or not the authorisation attempt succeeded.
        /// </summary>
        public bool Success { get; internal set; }

        /// <summary>
        ///     User state which may have been provided when the authorisation attempt was started.
        /// </summary>
        public string State { get; internal set; }

        /// <summary>
        ///     Encrypted access token returned from Sage ID servers. This can not be decrypted by the client application.
        /// </summary>
        public string AccessToken { get; internal set; }

        /// <summary>
        ///     The UTC expiry of the access token token
        /// </summary>
        public DateTimeOffset Expiry { get; internal set; }

        /// <summary>
        ///     The exception that occurred if Success is false.
        /// </summary>
        public string Error { get; internal set; }
    }
}