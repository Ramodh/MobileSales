﻿using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps.Interfaces;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace SageMobileSales.Views
{
    public sealed partial class ConfigurationSettingsFlyout : SettingsFlyout
    {
        private readonly IEventAggregator _eventAggregator;

        public ConfigurationSettingsFlyout(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            _eventAggregator = eventAggregator;
            //this.PasswordBox.KeyDown += PasswordBox_KeyDown;
            //this.Unloaded += SignInFlyout_Unloaded;
            //  _eventAggregator.GetEvent<FocusOnKeyboardInputChangedEvent>().Publish(false);
            var viewModel = DataContext as IFlyoutViewModel;
            viewModel.CloseFlyout = () => Hide();
        }
    }
}