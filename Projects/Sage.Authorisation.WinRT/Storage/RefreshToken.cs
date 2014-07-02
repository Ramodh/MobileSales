using System.Runtime.Serialization;

namespace Sage.Authorisation.WinRT.Storage
{
    /// <summary>
    /// Refresh token structure. DataContract attributes allow it to be stored in local file storage serialized by data contract serializer.
    /// XmlSerializer would be quicker but it doesn't work with none public types.
    /// </summary>
    [DataContract]
    internal class RefreshToken : Token
    {
        #region Fields

        protected string _accessToken;

        #endregion

        #region Public Properties

        [DataMember]
        public string Token
        {
            get { return _accessToken; }
            set { _accessToken = value; }
        }

        #endregion
    }
}
