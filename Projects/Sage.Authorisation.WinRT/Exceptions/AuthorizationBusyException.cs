using System;

namespace Sage.Authorisation.WinRT.Exceptions
{
    internal class AuthorisationBusyException : AuthorisationException
    {
        internal AuthorisationBusyException()
            :base(loader.GetString("ExceptionAuthorisationBusyException"))
        {
            HResult = ExceptionHResults.AuthorisationInProgress;
        }

        internal AuthorisationBusyException(string message)
            : base(message)
        {
            HResult = ExceptionHResults.AuthorisationInProgress;
        }

        internal AuthorisationBusyException(string message, string state)
            : base(message, state)
        {
            HResult = ExceptionHResults.AuthorisationInProgress;
        }

        internal AuthorisationBusyException(string message, string state, Exception inner)
            : base(message, state, inner)
        {
            HResult = ExceptionHResults.AuthorisationInProgress;
        }
    }
}
