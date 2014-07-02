using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sage.Authorisation.WinRT.Exceptions
{
    /// <summary>
    /// Thrown when the Sage.Authorisation library experiences any problems communicating with the authorisation server.
    /// </summary>
    /// <remarks>
    /// If this error occurs, there was HTTP(S) or socket connection errors. A firewall or proxy server may be interferring with communication. A check should
    /// be performed to test whether the machine has access to the internet and the Sage ID servers.
    /// <para>
    /// This error is non-recoverable and suggests network or configuration issues.
    /// </para>
    /// </remarks>
    internal class CommunicationException : AuthorisationException
    {
        internal CommunicationException(string message)
            : base(message)
        {
            HResult = ExceptionHResults.CommunicationException;
        }

        internal CommunicationException(string message, string state)
            : base(String.Format(loader.GetString("ExceptionCommunicationException"), message), state)
        {
            HResult = ExceptionHResults.CommunicationException;
        }

        internal CommunicationException(string message, string state, Exception inner)
            : base(message, state, inner)
        {
            HResult = ExceptionHResults.CommunicationException;
        }
    }
}
