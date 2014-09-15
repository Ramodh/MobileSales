using Microsoft.Practices.Prism.StoreApps;
using SageMobileSales.DataAccess.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace SageMobileSales.Views
{
    public sealed partial class QuoteLineItemUserControl : UserControl
    {

        private string _log = string.Empty;

        public DelegateCommand RecentOrdersCommand { get; private set; }

        public QuoteLineItemUserControl()
        {
            this.InitializeComponent();
            RecentOrdersCommand = DelegateCommand.FromAsyncHandler(NavigateToRecentOrders);
        }



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
