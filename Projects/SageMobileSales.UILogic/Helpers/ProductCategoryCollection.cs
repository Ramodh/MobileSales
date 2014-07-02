using SageMobileSales.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace SageMobileSales.UILogic.Helpers
{
    /// <summary>
    /// Binds to gridview to enable data visualization
    /// </summary>
    class ProductCategoryCollection : ObservableCollection<ProductCategory>, ISupportIncrementalLoading
    {
        private int previousItemIndex = 0;
        private List<ProductCategory> _productCategoryList;
        public List<ProductCategory> ProductCategoryList
        {
            get { return _productCategoryList; }
            set { _productCategoryList = value; }
        }

        public bool HasMoreItems
        {
            get
            {
                return (this.Count < ProductCategoryList.Count);
            }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            int nextItemIndex = 0;
            // Simulate a delay
            var delay = Task.Delay(50);

            var load = delay.ContinueWith(
              t =>
              {
                  var startSize = this.Count;
                  count = (uint)Math.Min(count, (this.ProductCategoryList.Count - startSize));

                  while (nextItemIndex < count)
                  {
                      this.Add(ProductCategoryList.ElementAt(previousItemIndex));                      
                      nextItemIndex++;
                      previousItemIndex++;
                  }

                  return (new LoadMoreItemsResult() { Count = count });
              },
              TaskScheduler.FromCurrentSynchronizationContext()
            );
            return (load.AsAsyncOperation<LoadMoreItemsResult>());
        }
    }
}
