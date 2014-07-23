using Sage.Authorisation.WinRT;
using SageMobileSales.ServiceAgents.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.ServiceAgents
{
    public class OAuthServiceAgent : IAuthInterface
    {
        private string _clientId = String.Empty;
        private string _deviceName = String.Empty;
        private string _log = String.Empty;

        private string _scope = String.Empty;
        private string _state = String.Empty;

        private string _token = String.Empty;
        private List<SiteConfiguration> _sites = new List<SiteConfiguration>();

        public OAuthServiceAgent()
        {
            _clientId = "TO3afnij1xMZrsH8akholwxvcJFlFc1N";// "eCnWXp7LaNNQwfWPMRbtRbe8Vvq6K00q";
            _scope = "gvb7lu14();";//"w8gygxtw();";
            _state = "Example state";
            _deviceName = "Example";
            _token = String.Empty;

            # region Sites
            //_sites.Add(new SiteConfiguration()
            //{
            //    SiteName = "SageMobileMobile",
            //    Protocol = "https",
            //    AuthInfo = new AuthorisationInfo
            //    {
            //        Scope = "gvb7lu14();",//"wpohijms();",
            //        State = "SCA+Time+Login",
            //        DeviceName = "Win8Device"
            //    },
            //    SSOClientId = "TO3afnij1xMZrsH8akholwxvcJFlFc1N",//"ji7EFlYjeN4rfIrb27a1jTI4NZIFiLgb",
            //    SSORedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native",//"https://holly.sageconstructionanywhere.com/oauth/native",
            //    CloudHostUrl = "https://services.sso.staging.services.sage.com"// "hollyapi.sageconstructionanywhere.com/"
            //});

            //_sites.Add(new SiteConfiguration()
            //{
            //    SiteName = "devpreview",
            //    Protocol = "https",
            //    AuthInfo = new AuthorisationInfo
            //    {
            //        Scope = "kbyemk8m();",
            //        State = "SCA+Time+Login",
            //        DeviceName = "Win8Device"
            //    },
            //    SSOClientId = "j2sak5nIN2YZ6szKaqx0hv66tLW0Mku0",
            //    SSORedirectUrl = "https://signon.sso.services.sage.com/oauth/native",
            //    CloudHostUrl = "devpreviewapi.sageconstructionanywhere.com/"
            //});
            # endregion
        }

        public string Token
        {
            get { return _token; }
            set
            {
                _token = value;
            }
        }

        private void onLogEvent(object sender, LogEvent e)
        {
            Log = e.Message + Environment.NewLine + Log;
        }

        public string ClientId
        {
            get { return _clientId; }
            set { _clientId = value; }
        }

        public string DeviceName
        {
            get { return _deviceName; }
            set { _deviceName = value; }
        }

        public string Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }

        public string State
        {
            get { return _state; }
            set { _state = value; }
        }

        public string Log
        {
            get { return _log; }
            set
            {
                _log = value;
            }
        }

        
        private Configuration GetConfigurationForSite(string siteName)
        {
            var siteConfig = _sites.FirstOrDefault(x => x.SiteName.ToLowerInvariant() == siteName.ToLowerInvariant());

            var config = new Configuration();

            if (siteConfig != null)
            {
                config.RedirectUri = siteConfig.SSORedirectUrl;
            }

            return config;
        }

        public List<SiteConfiguration> GetAvailableSites()
        {
            return _sites;
        }

        public async Task<String> Authorize(string siteName, bool forceInteractiveLogon = true)
        {

            OAuthClient client = null;

            try
            {
                var site = _sites.SingleOrDefault(x => x.SiteName.Equals(siteName, StringComparison.CurrentCultureIgnoreCase));

                if (site != null)
                {
                    client = new OAuthClient(site.SSOClientId);
                    client.LogEvent += onLogEvent;
                    if (forceInteractiveLogon)
                    {
                        site.AuthInfo.ResetDuration = true;
                    }
                    var result = await client.AuthoriseAsync(site.AuthInfo);
                    Token = result.AccessToken;
                }
                else
                {
                    // todo - handle this case
                }
                //AuthorisationInfo info = new AuthorisationInfo();
                //info.Scope = Scope;
                //info.State = State;
                //info.DeviceName = DeviceName;

                //client = new OAuthClient(ClientId);
                //client.LogEvent += onLogEvent;

                //var result = await client.AuthoriseAsync(info);
                //Token = result.AccessToken;



            }
            catch (Exception e)
            {
                Log = e.Message + Environment.NewLine + Log;
            }
            finally
            {
                client.LogEvent -= onLogEvent;
            }

            return Token;
        }
    }
}
