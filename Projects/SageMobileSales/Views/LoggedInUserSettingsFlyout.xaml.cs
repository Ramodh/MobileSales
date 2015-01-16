using Windows.UI.Xaml.Controls;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.StoreApps.Interfaces;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace SageMobileSales.Views
{
    public sealed partial class LoggedInUserSettingsFlyout : SettingsFlyout
    {
        private readonly IEventAggregator _eventAggregator;

        public LoggedInUserSettingsFlyout(IEventAggregator eventAggregator)
        {
            this.InitializeComponent();
            _eventAggregator = eventAggregator;
            var viewModel = DataContext as IFlyoutViewModel;
            viewModel.CloseFlyout = () => Hide();
        }
    }
}
