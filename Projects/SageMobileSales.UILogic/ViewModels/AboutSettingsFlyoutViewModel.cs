using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.StoreApps.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Practices.Prism.PubSubEvents;
using Windows.ApplicationModel;

namespace SageMobileSales.UILogic.ViewModels
{
    class AboutSettingsFlyoutViewModel:Microsoft.Practices.Prism.StoreApps.BindableBase, IFlyoutViewModel
    {
        private bool _isOpened;
        private Action _closeFlyout;
        private INavigationService _navigationService;
   
        [RestorableState]
        public bool IsOpened
        {
            get { return _isOpened; }
            private set { SetProperty(ref _isOpened, value); }
        }

        public AboutSettingsFlyoutViewModel(INavigationService navigationService, IEventAggregator eventAggregator)
        {
          
            _navigationService = navigationService;
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

    public string AppVersion
    {
        get
        {
            var version = Package.Current.Id.Version;
            return String.Format("Version {0}.{1}.{2}.{3}", version.Major, version.Minor,version.Build,version.Revision);
        }
    }
    }
}
