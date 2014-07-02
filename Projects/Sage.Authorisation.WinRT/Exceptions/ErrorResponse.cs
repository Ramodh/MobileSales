using Sage.Authorisation.WinRT.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Sage.Authorisation.WinRT.Exceptions
{
    internal class ErrorResponse
    {
        /// <summary>
        /// Error code returned from server
        /// </summary>       
        public string Error
        {
            get;set;
        }

        /// <summary>
        /// Error description returned from server
        /// </summary>
        public string Error_Description
        {
            get;set;
        }
        
        /// <summary>
        /// Raises an AuthorisationErrorResponseException with the specified state.
        /// </summary>
        /// <param name="state">State for exception</param>
        public void Throw(string state)
        {
            throw new AuthorisationErrorResponseException(state, Error, Error_Description);
        }

        /// <summary>
        /// Shows the value of the object properties
        /// </summary>
        public override string ToString()
        {
            return String.Format("ErrorResponse: Error={0}, Error_Description={1}.", Error, Error_Description);
        }
    }
}
