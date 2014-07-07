using System;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;

namespace SageMobileSales.UILogic.ViewModels
{
    public class MainMenuPageViewModel : ViewModel
    {
        private readonly IDataContext _dataContext;
        private readonly INavigationService _navigationService;
        private readonly IOAuthService _oAuthService;

        public MainMenuPageViewModel(IOAuthService oAuthService, IDataContext dataContext,
            INavigationService navigationService)
        {
            _oAuthService = oAuthService;
            _dataContext = dataContext;
            _navigationService = navigationService;
            CleanUpCommand = DelegateCommand.FromAsyncHandler(Cleanup);
        }

        # region Command Implementations

        /// <summary>
        ///     Makes call to OAuthService to clear stored AccessToken from Authorisation library
        ///     Removes  AccessToken from ApplicationData which we stored internally.
        ///     Makes call to datacontext to delete LocalDB
        /// </summary>
        /// <returns></returns>
        private async Task Cleanup()
        {
            try
            {
                await _oAuthService.Cleanup();
                ApplicationDataContainer settingsLocal = ApplicationData.Current.LocalSettings;
                settingsLocal.Values.Remove("AccessToken");
                settingsLocal.Values.Remove(PageUtils.IsAuthorised);
                await _dataContext.DeleteDatabase();

                _navigationService.Navigate("Signin", null);
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        # endregion

        public DelegateCommand CleanUpCommand { get; private set; }
    }
}