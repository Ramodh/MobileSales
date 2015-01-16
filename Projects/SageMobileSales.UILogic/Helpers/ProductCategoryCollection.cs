using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using SageMobileSales.DataAccess.Entities;

namespace SageMobileSales.UILogic.Helpers
{
    /// <summary>
    ///     Binds to gridview to enable data visualization
    /// </summary>
    internal class ProductCategoryCollection : ObservableCollection<ProductCategory>, ISupportIncrementalLoading
    {
        private int previousItemIndex;
        public List<ProductCategory> ProductCategoryList { get; set; }

        public bool HasMoreItems
        {
            get { return (Count < ProductCategoryList.Count); }
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
                    count = (uint) Math.Min(count, (ProductCategoryList.Count - startSize));

                    while (nextItemIndex < count)
                    {
                        Add(ProductCategoryList.ElementAt(previousItemIndex));
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