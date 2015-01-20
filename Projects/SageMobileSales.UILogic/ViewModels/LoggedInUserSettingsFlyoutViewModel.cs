using System;
using Windows.ApplicationModel;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using SageMobileSales.DataAccess.Repositories;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SageMobileSales.UILogic.Common;

namespace SageMobileSales.UILogic.ViewModels
{
    public class LoggedInUserSettingsFlyoutViewModel : BindableBase, IFlyoutViewModel
    {
        private Action _closeFlyout;
        private bool _isOpened;
        private string _repName;
        private INavigationService _navigationService;
        private readonly ISalesRepRepository _salesRepRepository;

        public LoggedInUserSettingsFlyoutViewModel(INavigationService navigationService, IEventAggregator eventAggregator, ISalesRepRepository salesRepRepository)
        {
            _navigationService = navigationService;
            _salesRepRepository = salesRepRepository;
            getLoggedInUserName();
        }

        private async void getLoggedInUserName()
        {
            try
            {
                var frame = Window.Current.Content as Frame;
                if (frame.SourcePageType.Name == PageUtils.SignInPage)
                {
                    RepName = "Please Sign In with valid credentials";
                }
                else
                {
                    RepName = "Signed in user as "+ await _salesRepRepository.GetSalesRepName();
                }
            }
            catch (Exception ex)
            { }
        }

        [RestorableState]
        public bool IsOpened
        {
            get { return _isOpened; }
            private set { SetProperty(ref _isOpened, value); }
        }

        public string RepName
        {
            get;
            set;
        }

        private void Close()
        {
            IsOpened = false;
        }

        public Action CloseFlyout
        {
            get { return _closeFlyout; }
            set { SetProperty(ref _closeFlyout, value); }
        }
    }
}
