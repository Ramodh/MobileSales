using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sage.Authorisation.WinRT;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.ServiceAgents.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;


namespace SageMobileSales.ServiceAgents
{
    public class OAuthService
    {
        # region Local Variables

        private string _clientId = String.Empty;
        private string _deviceName = String.Empty;
        private string _log = String.Empty;
        private bool _resetDuration = false;
        private string _scope = String.Empty;
        private string _state = String.Empty;
        private bool _suppressInteractive = false;
        private string _token = String.Empty;
        private OAuthClient _client = null;
        # endregion

        // Constructor
        public OAuthService()
        {
            _clientId = "TO3afnij1xMZrsH8akholwxvcJFlFc1N";// "eCnWXp7LaNNQwfWPMRbtRbe8Vvq6K00q";
            _scope = "gvb7lu14();";//"w8gygxtw();";
            _state = "Example state";
            _deviceName = "Example";
            _token = String.Empty;
        }

        #region Properties

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

        public string Log
        {
            get { return _log; }
            set { _log = value; }
        }

        public bool ResetDuration
        {
            get { return _resetDuration; }
            set { _resetDuration = value; }
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

        public bool SuppressInteractive
        {
            get { return _suppressInteractive; }
            set { _suppressInteractive = value; }
        }

        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        #endregion

        public async Task<String> Authorize()
        {

            try
            {
                AuthorisationInfo authinfo = new AuthorisationInfo();
                authinfo.Scope = Scope;
                authinfo.State = State;
                authinfo.DeviceName = DeviceName;

                _client = new OAuthClient(ClientId);
                _client.LogEvent += onLogEvent;
                var result = await _client.AuthoriseAsync(authinfo);
                Token = result.AccessToken;
            }
            catch (Exception e)
            {
                Log = e.Message + Environment.NewLine + Log;
            }
            finally
            {
                _client.LogEvent -= onLogEvent;
            }
            return Token;
        }

        public async Task Cleanup()
        {

            try
            {
                _client = new OAuthClient(ClientId);
                _client.LogEvent += onLogEvent;
                await _client.CleanupAsync();
                Token = String.Empty;
            }
            finally
            {
                _client.LogEvent -= onLogEvent;
            }
        }

        private void onLogEvent(object sender, LogEvent e)
        {
            Log = e.Message + Environment.NewLine + Log;
        }
    }
}
