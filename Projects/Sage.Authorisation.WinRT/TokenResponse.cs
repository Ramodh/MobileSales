using Newtonsoft.Json;

namespace Sage.Authorisation.WinRT
{
    internal class TokenResponse
    {
        public string Access_Token { get; set; }

        public string Refresh_Token { get; set; }

        public string Token_Type { get; set; }

        public int Expires_In { get; set; }

        public string Scope { get; set; }

        internal static TokenResponse Parse(string responseString)
        {
            var result = JsonConvert.DeserializeObject<TokenResponse>(responseString);
            return result;
        }

        internal AuthorisationResult AsAuthorisationResult()
        {
            var result = new AuthorisationResult();
            return result;
        }
    }
}