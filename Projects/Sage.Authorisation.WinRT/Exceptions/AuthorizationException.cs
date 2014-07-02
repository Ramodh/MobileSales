using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace Sage.Authorisation.WinRT.Exceptions
{
    internal class AuthorisationException : Exception
    {
        protected static ResourceLoader loader = new ResourceLoader("Sage.Authorisation.WinRT/Resources");

        /// <summary>
        /// Constructor used by OAuthClientThin.  Thin clients do not handle most
        /// of the authorisation process so won't have a state parameter that needs
        /// to be included in the exception.
        /// </summary>
        internal AuthorisationException()
            : base(loader.GetString("ExceptionSageAuthorisationException"))
        {
        }

        internal AuthorisationException(string state)
            : base(loader.GetString("ExceptionSageAuthorisationException"))
        {
        }

        internal AuthorisationException(string message, string state)
            : base(message)
        {
        }

        internal AuthorisationException(string message, string state, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Gets any state information that was supplied as part of the authorisation request. Please note, State may not always be available depending on the nature of the exception.
        /// </summary>
        public string State { get; internal set; }
    }
}
