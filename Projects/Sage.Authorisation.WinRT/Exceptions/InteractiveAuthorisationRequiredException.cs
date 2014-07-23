using System;

namespace Sage.Authorisation.WinRT.Exceptions
{
    internal class InteractiveAuthorisationRequiredException : AuthorisationException
    {
        internal InteractiveAuthorisationRequiredException(string state) : base(state)
        {
            HResult = ExceptionHResults.InteractiveAuthorisationRequired;
        }

        internal InteractiveAuthorisationRequiredException(string state, Exception inner)
            : base(state)
        {
            HResult = ExceptionHResults.InteractiveAuthorisationRequired;
        }
    }
}