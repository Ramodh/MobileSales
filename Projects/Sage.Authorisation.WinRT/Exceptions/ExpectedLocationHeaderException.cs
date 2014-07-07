using System;

namespace Sage.Authorisation.WinRT.Exceptions
{
    /// <summary>
    ///     Thrown when the authorisation server does not return a valid location with a
    ///     302 HTTP status code.
    /// </summary>
    /// <remarks>
    ///     The server response did not contain the expected redirect location. This may be due to a temporary server problem,
    ///     invalid or expired credential,
    ///     or configuration error. You should check the parameters used to instantiate the
    ///     <see cref="Sage.Authorisation.WinRT.OAuthClient">OAuthClient</see> class
    ///     as well as the parameters used in the call to
    ///     <see cref="Sage.Authorisation.WinRT.OAuthClient.AuthoriseAsync">Authorise</see>.
    ///     <para>
    ///         Web proxy servers may also interfere with HTTP redirects and cause these exceptions, expecially if proxy
    ///         server filters HTTP response headers.
    ///     </para>
    /// </remarks>
    internal class ExpectedLocationHeaderException : AuthorisationException
    {
        internal ExpectedLocationHeaderException(string state)
            : base(loader.GetString("ExceptionInvalidResponseExpectedLocationHeader"), state)
        {
            HResult = ExceptionHResults.ExpectedLocationHeader;
        }

        internal ExpectedLocationHeaderException(string message, string state)
            : base(message, state)
        {
            HResult = ExceptionHResults.ExpectedLocationHeader;
        }

        internal ExpectedLocationHeaderException(string message, string state, Exception inner)
            : base(message, state, inner)
        {
            HResult = ExceptionHResults.ExpectedLocationHeader;
        }
    }
}