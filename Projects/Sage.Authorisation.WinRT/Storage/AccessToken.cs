using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Sage.Authorisation.WinRT.Storage
{
    /// <summary>
    /// Access token structure to store in memory for quick authorisations when token is in date.
    /// </summary>
    internal class AccessToken : Token
    {
        #region Fields

        protected string _accessToken;

        protected DateTime _expiryUtc;

        #endregion

        #region Public Properties

        public DateTime ExpiryUtc
        {
            get { return _expiryUtc; }
            set { _expiryUtc = value; }
        }

        public string Token
        {
            get { return _accessToken; }
            set { _accessToken = value; }
        }

        public bool Expired
        {
            get { return _expiryUtc < DateTime.UtcNow; }
        }

        #endregion
    }
}
