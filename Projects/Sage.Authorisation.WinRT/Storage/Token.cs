using System;
using System.Runtime.Serialization;

namespace Sage.Authorisation.WinRT.Storage
{
    /// <summary>
    ///     Base class for tokens
    /// </summary>
    [KnownType(typeof (RefreshToken))]
    [DataContract]
    internal class Token
    {
        protected DateTime _retrievedUtc = DateTime.UtcNow;
        protected string _scope;

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