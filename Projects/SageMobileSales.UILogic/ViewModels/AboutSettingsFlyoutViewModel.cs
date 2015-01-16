using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using System;
using Windows.ApplicationModel;

namespace SageMobileSales.UILogic.ViewModels
{
    internal class AboutSettingsFlyoutViewModel : BindableBase, IFlyoutViewModel
    {
        private Action _closeFlyout;
        private bool _isOpened;
        private INavigationService _navigationService;

        public AboutSettingsFlyoutViewModel(INavigationService navigationService, IEventAggregator eventAggregator)
        {
            _navigationService = navigationService;
        }

        [RestorableState]
        public bool IsOpened
        {
            get { return _isOpened; }
            private set { SetProperty(ref _isOpened, value); }
        }

        public string AppVersion
        {
            get
            {
                var version = Package.Current.Id.Version;
                return String.Format("Version {0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build,
                    version.Revision);
            }
        }

        public Action CloseFlyout
        {
            get { return _closeFlyout; }
            set { SetProperty(ref _closeFlyout, value); }
        }

        private void Close()
        {
            IsOpened = false;
        }
    }
}