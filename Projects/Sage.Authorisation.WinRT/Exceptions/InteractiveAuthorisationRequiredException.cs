using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Authorisation.WinRT.Exceptions
{
    internal class InteractiveAuthorisationRequiredException : AuthorisationException
    {
        internal InteractiveAuthorisationRequiredException(string state) :base (state)
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
