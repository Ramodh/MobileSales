using Microsoft.Practices.Prism.StoreApps;
using SageMobileSales.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SageMobileSales.UILogic.ViewModels
{
   internal class RecentOrdersPageViewModel: ViewModel
    {
              
       private QuoteLineItemViewModel _lineItemDetail;
       private List<RecentOrders> _productRecentOrders;
       private string _customerName;

       public QuoteLineItemViewModel LineItemDetail
       {
           get { return _lineItemDetail; }
           private set { SetProperty(ref _lineItemDetail, value); }
       }

       public List<RecentOrders> ProductRecentOrders
       {
           get { return _productRecentOrders; }
           private set { SetProperty(ref _productRecentOrders, value); }
       }

       public string CustomerName
       {
           get { return _customerName; }
           private set { SetProperty(ref _customerName, value); }
       }

       public override void OnNavigatedTo(object navigationParameter, Windows.UI.Xaml.Navigation.NavigationMode navigationMode, Dictionary<string, object> viewModelState)
       {
           if (navigationParameter != null)
           {
               LineItemDetail = navigationParameter as QuoteLineItemViewModel;
           }

           //TODO
           // Need to be replaced with real data once Recent Orders service calls are done
           ProductRecentOrders = new List<RecentOrders>();
           for (int i = 0; i < 4; i++)
           {
               RecentOrders recentOrder = new RecentOrders();
               recentOrder.Date = Convert.ToDateTime("05/09/2014");
               recentOrder.Invoice = "#1234567";
               recentOrder.Quantity = 9;
               recentOrder.UnitPrice = Convert.ToDecimal("369.89");
               recentOrder.Total = Convert.ToDecimal("3329.01");
               ProductRecentOrders.Add(recentOrder);
           }
           CustomerName = "Actwin Co. Ltd";

           base.OnNavigatedTo(navigationParameter, navigationMode, viewModelState);
       }
    }
}
