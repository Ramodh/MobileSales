using System;
using System.Threading.Tasks;
using Windows.Storage;
using Sage.Authorisation.WinRT;
using SageMobileSales.DataAccess;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.ServiceAgents.Common;

namespace SageMobileSales.ServiceAgents.Services
{
    public class OAuthService : IOAuthService
    {
        # region Local Variables

        private OAuthClient _client;
        private string _clientId = String.Empty;
        private string _deviceName = String.Empty;
        private string _log = String.Empty;
        private string _scope = String.Empty;
        private string _state = String.Empty;
        private string _token = String.Empty;

        # endregion

        private readonly IDatabase _database;

        // Constructor
        public OAuthService(IDatabase database)
        {
            SuppressInteractive = false;
            ResetDuration = false;
            _database = database;
            // Authorisation Info 

            _state = "Examplestate";
            _deviceName = "mobilesalesiosclient";
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

        public bool ResetDuration { get; set; }

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

        public bool SuppressInteractive { get; set; }

        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        #endregion

        /// <summary>
        ///     Makes call to OAuthClient(Authorisation Library) where it will validate the AuthorisationInfo which we are passing
        ///     and inturn returns AccessToken.
        /// </summary>
        /// <returns></returns>
        public async Task<String> Authorize()
        {
            try
            {
                _clientId = Constants.ClientId;

                _scope = Constants.Scope;
                ApplicationDataContainer settingsLocal = ApplicationData.Current.LocalSettings;
                var authinfo = new AuthorisationInfo();
                authinfo.Scope = Scope;
                authinfo.State = State;
                authinfo.DeviceName = DeviceName;

                _client = new OAuthClient(ClientId);
                _client.LogEvent += onLogEvent;
                AuthorisationResult result = await _client.AuthoriseAsync(authinfo);
                Token = result.AccessToken;
                // Adding AccessToken to Application Data.
                // So that we can use this in the enitre application when ever we need.
                settingsLocal.Containers["SageSalesContainer"].Values["AccessToken"] = Token;
                Constants.AccessToken = Token;
                //Token = Constants.AccessToken;
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
            //finally
            //{
            //    _client.LogEvent -= onLogEvent;
            //}
            return Token;
        }

        /// <summary>
        ///     Makes call to OAuthClient to remove the AccessToken which is internally saved by Authorisation library.
        /// </summary>
        /// <returns></returns>
        public async Task Cleanup()
        {
            try
            {
                Constants.IsDbDeleted = true;
                ApplicationDataContainer settingsLocal = ApplicationData.Current.LocalSettings;
                settingsLocal.DeleteContainer("SageSalesContainer");
                bool isServerChanged = false;
                if (_client == null)
                {
                    _client = new OAuthClient(ClientId);
                }
                if (settingsLocal.Containers.ContainsKey("ConfigurationSettingsContainer"))
                {
                    if (settingsLocal.Containers["ConfigurationSettingsContainer"].Values["IsServerChanged"] != null)
                    {
                        isServerChanged = Convert.ToBoolean(settingsLocal.Containers["ConfigurationSettingsContainer"].Values["IsServerChanged"]);
                    }
                }
                await _client.CleanupAsync(isServerChanged);
                _client.LogEvent += onLogEvent;
                Token = String.Empty;
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