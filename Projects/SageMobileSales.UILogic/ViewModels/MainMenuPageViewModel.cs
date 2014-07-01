using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using Sage.Authorisation.WinRT;
using SageMobileSales.DataAccess;
using SageMobileSales.ServiceAgents;
using SageMobileSales.ServiceAgents.Services;
using SageMobileSales.UILogic.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace SageMobileSales.UILogic.ViewModels
{
   public class MainMenuPageViewModel:ViewModel
    {
       public DelegateCommand CleanUpCommand { get; private set; }

       private INavigationService _navigationService;
       private readonly IOAuthService _oAuthService;
       private readonly IDataContext _dataContext;

       public MainMenuPageViewModel(IOAuthService oAuthService,IDataContext dataContext,INavigationService navigationService)
        {
            _oAuthService = oAuthService;
            _dataContext = dataContext;
            _navigationService = navigationService;
            CleanUpCommand = DelegateCommand.FromAsyncHandler(Cleanup);  
            
        }
       # region Command Implementations

       /// <summary>
       /// Makes call to OAuthService to clear stored AccessToken from Authorisation library
       /// Removes  AccessToken from ApplicationData which we stored internally.
       /// Makes call to datacontext to delete LocalDB
       /// </summary>
       /// <returns></returns>
       private async Task Cleanup()
       {

           try
           {
               await _oAuthService.Cleanup();
               var settingsLocal = ApplicationData.Current.LocalSettings;
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
    }
}
