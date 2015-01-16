using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using SageMobileSales.DataAccess.Model;

namespace SageMobileSales.UILogic.Helpers
{
    internal class ProductCollection : ObservableCollection<ProductDetails>, ISupportIncrementalLoading
    {
        private int previousItemIndex;
        public List<ProductDetails> ProductList { get; set; }

        public bool HasMoreItems
        {
            get { return (Count < ProductList.Count); }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            var nextItemIndex = 0;
            // Simulate a delay
            var delay = Task.Delay(50);

            var load = delay.ContinueWith(
                t =>
                {
                    var startSize = Count;
                    count = (uint) Math.Min(count, (ProductList.Count - startSize));

                    while (nextItemIndex < count)
                    {
                        Add(ProductList.ElementAt(previousItemIndex));
                        nextItemIndex++;
                        previousItemIndex++;
                    }

                    return (new LoadMoreItemsResult {Count = count});
                },
                TaskScheduler.FromCurrentSynchronizationContext()
                );
            return (load.AsAsyncOperation());
        }
    }
}