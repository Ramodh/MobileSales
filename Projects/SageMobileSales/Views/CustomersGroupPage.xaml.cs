using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using SageMobileSales.Common;
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
using System.ComponentModel;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace SageMobileSales.Views
{
    /// <summary>
    /// A page that displays a grouped collection of Customers.
    /// </summary>
    public sealed partial class CustomersGroupPage : VisualStateAwarePage
    {
        public CustomersGroupPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var viewModel = this.DataContext as INotifyPropertyChanged;
            if (viewModel != null)
            {
                viewModel.PropertyChanged += viewModel_PropertyChanged;
            }
            base.OnNavigatedTo(e);
        }

        void viewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GroupedCustomerList")
            {
                (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = groupedItemsViewSource.View.CollectionGroups;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var sageMobileSales = App.Current as App;
            var viewModel = this.DataContext as INotifyPropertyChanged;
            if (sageMobileSales != null && !sageMobileSales.IsSuspending && viewModel != null)
            {
                viewModel.PropertyChanged -= viewModel_PropertyChanged;
            }
            base.OnNavigatedFrom(e);
        }
    }
}
