using System;
using System.Net;

namespace Sage.Authorisation.WinRT.Exceptions
{
    /// <summary>
    ///     Thrown when the authorisation server does not respond with an expected
    ///     302 HTTP status code.
    /// </summary>
    /// <remarks>
    ///     An unexpected response code was returned from the server. This may be due to a temporary server problem, invalid or
    ///     expired credential,
    ///     or configuration error. You should check the parameters used to instantiate the
    ///     <see cref="Sage.Authorisation.WinRT.OAuthClient">OAuthClient</see> class
    ///     as well as the parameters used in the call to
    ///     <see cref="Sage.Authorisation.WinRT.OAuthClient.AuthoriseAsync">Authorise</see>.
    ///     <para>
    ///         Web proxy servers may also interfere with HTTP redirects and cause these exceptions, expecially if proxy
    ///         server credentials are required.
    ///     </para>
    /// </remarks>
    internal class ExpectedFoundException : AuthorisationException
    {
        internal ExpectedFoundException(HttpStatusCode responseCode, string state)
            : base(String.Format(loader.GetString("ExceptionInvalidResponseExpectedFound"), responseCode), state)
        {
            HResult = ExceptionHResults.ExpectedFoundException;
        }

        internal ExpectedFoundException(string message, string state)
            : base(message, state)
        {
            HResult = ExceptionHResults.ExpectedFoundException;
        }

        internal ExpectedFoundException(string message, string state, Exception inner)
            : base(message, state, inner)
        {
            HResult = ExceptionHResults.ExpectedFoundException;
        }
    }
}