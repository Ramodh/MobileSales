using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sage.Authorisation.WinRT;
using SageMobileSales.DataAccess;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.ServiceAgents.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;



namespace SageMobileSales.ServiceAgents.Services
{
    public class OAuthService : IOAuthService
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

        private readonly IDatabase _database;

        // Constructor
        public OAuthService(IDatabase database)
        {
            _database = database;
            // Authorisation Info 
          
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

        /// <summary>
        /// Makes call to OAuthClient(Authorisation Library) where it will validate the AuthorisationInfo which we are passing and inturn returns AccessToken.
        /// </summary>
        /// <returns></returns>
        public async Task<String> Authorize()
        {

            try
            {
                _clientId = Constants.ClientId;

                _scope = Constants.Scope;
                var settingsLocal = ApplicationData.Current.LocalSettings;
                AuthorisationInfo authinfo = new AuthorisationInfo();
                authinfo.Scope = Scope;
                authinfo.State = State;
                authinfo.DeviceName = DeviceName;

                _client = new OAuthClient(ClientId);
                _client.LogEvent += onLogEvent;
                var result = await _client.AuthoriseAsync(authinfo);
                Token = result.AccessToken;
                // Adding AccessToken to Application Data.
                // So that we can use this in the enitre application when ever we need.
                settingsLocal.Containers["SageSalesContainer"].Values["AccessToken"] = Token;
               Constants.AccessToken = Token;
                //Token = Constants.AccessToken;
            }
            catch(NullReferenceException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
                Log = ex.Message + Environment.NewLine + Log;

            }
            //finally
            //{
            //    _client.LogEvent -= onLogEvent;
            //}
            return Token;
        }

        /// <summary>
        /// Makes call to OAuthClient to remove the AccessToken which is internally saved by Authorisation library.
        /// </summary>
        /// <returns></returns>
        public async Task Cleanup()
        {

            try
            {
                Constants.IsDbDeleted = true;
                if (_client == null)
                {
                    _client = new OAuthClient(ClientId);
                }
                    await _client.CleanupAsync();                   
                    _client.LogEvent += onLogEvent;                
                Token = String.Empty;
                var settingsLocal = ApplicationData.Current.LocalSettings;
                settingsLocal.DeleteContainer("SageSalesContainer");
             //  await _database.Delete();
              
            }
            catch (NullReferenceException ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
                Log = ex.Message + Environment.NewLine + Log;

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
