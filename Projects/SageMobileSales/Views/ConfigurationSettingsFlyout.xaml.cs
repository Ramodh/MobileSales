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
using Microsoft.Practices.Prism.StoreApps;
using Microsoft.Practices.Prism.PubSubEvents;
// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace SageMobileSales.Views
{
    public sealed partial class ConfigurationSettingsFlyout : SettingsFlyout
    {
        private readonly IEventAggregator _eventAggregator;
        public ConfigurationSettingsFlyout(IEventAggregator eventAggregator)
        {
            this.InitializeComponent();
           
            _eventAggregator = eventAggregator;
            //this.PasswordBox.KeyDown += PasswordBox_KeyDown;
            //this.Unloaded += SignInFlyout_Unloaded;
          //  _eventAggregator.GetEvent<FocusOnKeyboardInputChangedEvent>().Publish(false);
            var viewModel = this.DataContext as IFlyoutViewModel;
            viewModel.CloseFlyout = () => this.Hide();
        }
     

    }
}
