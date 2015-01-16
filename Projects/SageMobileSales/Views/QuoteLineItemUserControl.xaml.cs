using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SageMobileSales.DataAccess.Common;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace SageMobileSales.Views
{
    public sealed partial class QuoteLineItemUserControl : UserControl
    {
        private string _log = string.Empty;

        public QuoteLineItemUserControl()
        {
            InitializeComponent();
            RecentOrdersCommand = DelegateCommand.FromAsyncHandler(NavigateToRecentOrders);
        }

        public DelegateCommand RecentOrdersCommand { get; private set; }

        private async Task NavigateToRecentOrders()
        {
            try
            {
            }
            catch (Exception ex)
            {
                _log = AppEventSource.Log.WriteLine(ex);
                AppEventSource.Log.Error(_log);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        /// <summary>
        ///     Grid View Item Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="parameter"></param>
        public async void GridViewRecentOrderItemClick(object sender, object parameter)
        {
            //var arg = (parameter as ItemClickEventArgs).ClickedItem as OrderDetails;

            //if (arg != null)
            //    _navigationService.Navigate("OrderDetails", arg);
        }
    }
}