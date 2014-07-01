using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace SageMobileSales.UILogic.Helpers
{
    class ProductCollection : ObservableCollection<ProductDetails>, ISupportIncrementalLoading
    {
        private int previousItemIndex = 0;
        private List<ProductDetails> _productList;
        public List<ProductDetails> ProductList
        {
            get { return _productList; }
            set { _productList = value; }
        }

        public bool HasMoreItems
        {
            get
            {
                return (this.Count < ProductList.Count);
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
                  count = (uint)Math.Min(count, (this.ProductList.Count - startSize));

                  while (nextItemIndex < count)
                  {
                      this.Add(ProductList.ElementAt(previousItemIndex));
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
