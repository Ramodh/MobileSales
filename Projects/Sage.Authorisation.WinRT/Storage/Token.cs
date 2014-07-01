using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Sage.Authorisation.WinRT.Storage
{
    /// <summary>
    /// Base class for tokens
    /// </summary>
    [KnownType(typeof(RefreshToken))]
    [DataContract]
    internal class Token
    {
        protected string _scope;
        protected DateTime _retrievedUtc = DateTime.UtcNow;

        [DataMember]
        public DateTime RetrievedUtc
        {
            get { return _retrievedUtc; }
            set { _retrievedUtc = value; }
        }
        
        [DataMember]
        public string Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }
    }
}
