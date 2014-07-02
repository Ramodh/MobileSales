using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sage.Authorisation.WinRT
{
    internal class TokenResponse
    {
        public string Access_Token
        {
            get;
            set;
        }

        public string Refresh_Token
        {
            get;
            set;
        }

        public string Token_Type
        {
            get;
            set;
        }

        public int Expires_In
        {
            get;
            set;
        }

        public string Scope
        {
            get;
            set;
        }

        internal static TokenResponse Parse(string responseString)
        {
            TokenResponse result = JsonConvert.DeserializeObject<TokenResponse>(responseString);
            return result;
        }

        internal AuthorisationResult AsAuthorisationResult()
        {
            AuthorisationResult result = new AuthorisationResult();
            return result;
        }
    }
}
