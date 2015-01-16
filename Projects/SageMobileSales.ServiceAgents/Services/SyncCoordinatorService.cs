using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using SageMobileSales.DataAccess.Common;
using SageMobileSales.DataAccess.Events;
using SageMobileSales.ServiceAgents.Common;

namespace SageMobileSales.ServiceAgents.Services
{
    public class SyncCoordinatorService : ISyncCoordinatorService
    {
        private readonly ICustomerService _customerService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IOrderService _orderService;
        private readonly IProductAssociatedBlobService _productAssociatedBlobService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IProductService _productService;
        private readonly IQuoteService _quoteService;
        private string _log = string.Empty;

        public SyncCoordinatorService(IProductCategoryService productCategoryService, IProductService productService,
            IProductAssociatedBlobService productAssociatedBlobService, ICustomerService customerService,
            IQuoteService quoteService, IOrderService orderService, IEventAggregator eventAggregator)
        {
            _productCategoryService = productCategoryService;
            _productService = productService;
            _productAssociatedBlobService = productAssociatedBlobService;
            _customerService = customerService;
            _quoteService = quoteService;
            _orderService = orderService;
            _eventAggregator = eventAggregator;
        }

        /// <summary>
        ///     Starts actual sync process
        /// </summary>
        /// Main Sync / Common sync
        /// <returns></returns>
        public async Task StartSync()
        {
            if (Constants.ConnectedToInternet())
            {
                Constants.SyncProgress = true;
                _log = "Common Sync Started";
                AppEventSource.Log.Info(_log);

                if (!Constants.CustomersSyncProgress)
                {
                    _log = "Common Customer Sync Started";
                    AppEventSource.Log.Info(_log);
                    Constants.CustomersSyncProgress = true;
                    _eventAggregator.GetEvent<CustomerSyncChangedEvent>().Publish(Constants.CustomersSyncProgress);
                    await _customerService.StartCustomersSyncProcess();
                    Constants.CustomersSyncProgress = false;
                    _eventAggregator.GetEvent<CustomerSyncChangedEvent>().Publish(Constants.CustomersSyncProgress);
                    _log = "Common Customer Sync Started";
                    AppEventSource.Log.Info(_log);
                }

                if (!Constants.ProductsSyncProgress)
                {
                    _log = "Common Product Sync Started";
                    AppEventSource.Log.Info(_log);
                    Constants.ProductsSyncProgress = true;
                    _eventAggregator.GetEvent<ProductSyncChangedEvent>().Publish(Constants.ProductsSyncProgress);
                    await _productCategoryService.StartCategorySyncProcess();
                    await _productService.StartProductsSyncProcess();
                    await _productAssociatedBlobService.StartProductAssoicatedBlobsSyncProcess();
                    Constants.ProductsSyncProgress = false;
                    _eventAggregator.GetEvent<ProductSyncChangedEvent>().Publish(Constants.ProductsSyncProgress);
                    _log = "Common Product Sync Started";
                    AppEventSource.Log.Info(_log);
                }

                if (!Constants.QuotesSyncProgress)
                {
                    _log = "Common Quote Sync Started";
                    AppEventSource.Log.Info(_log);
                    Constants.QuotesSyncProgress = true;
                    _eventAggregator.GetEvent<QuoteSyncChangedEvent>().Publish(Constants.QuotesSyncProgress);
                    await _quoteService.StartQuoteSyncProcess();
                    Constants.QuotesSyncProgress = false;
                    _eventAggregator.GetEvent<QuoteSyncChangedEvent>().Publish(Constants.QuotesSyncProgress);
                    _log = "Common Quote Sync Started";
                    AppEventSource.Log.Info(_log);
                }

                if (!Constants.OrdersSyncProgress)
                {
                    _log = "Common Order Sync Started";
                    AppEventSource.Log.Info(_log);
                    Constants.OrdersSyncProgress = true;
                    _eventAggregator.GetEvent<OrderSyncChangedEvent>().Publish(Constants.OrdersSyncProgress);
                    await _orderService.StartOrdersSyncProcess();
                    Constants.OrdersSyncProgress = false;
                    _eventAggregator.GetEvent<OrderSyncChangedEvent>().Publish(Constants.OrdersSyncProgress);
                    _log = "Common Order Sync Started";
                    AppEventSource.Log.Info(_log);
                }

                Constants.SyncProgress = false;
                _log = "Common Sync Ended";
                AppEventSource.Log.Info(_log);
            }
            else
            {
                Constants.ShowMessageDialog(
                    ResourceLoader.GetForCurrentView("Resources").GetString("NoInternetConnection"));
            }
        }

        /// <summary>
        ///     Starts products sync
        /// </summary>
        /// <returns></returns>
        public async Task StartProductsSync()
        {
            Constants.ProductsSyncProgress = true;
            _log = "Product Sync Started";
            AppEventSource.Log.Info(_log);
            _eventAggregator.GetEvent<ProductSyncChangedEvent>().Publish(Constants.ProductsSyncProgress);
            await _productCategoryService.StartCategorySyncProcess();
            await _productService.StartProductsSyncProcess();
            await _productAssociatedBlobService.StartProductAssoicatedBlobsSyncProcess();
            Constants.ProductsSyncProgress = false;
            _log = "Product Sync Ended";
            AppEventSource.Log.Info(_log);
            _eventAggregator.GetEvent<ProductSyncChangedEvent>().Publish(Constants.ProductsSyncProgress);
        }

        /// <summary>
        ///     Starts quotes sync
        /// </summary>
        /// <returns></returns>
        public async Task StartQuotesSync()
        {
            Constants.QuotesSyncProgress = true;
            _log = "Quote Sync Started";
            AppEventSource.Log.Info(_log);
            _eventAggregator.GetEvent<QuoteSyncChangedEvent>().Publish(Constants.QuotesSyncProgress);
            await _quoteService.StartQuoteSyncProcess();
            Constants.QuotesSyncProgress = false;
            _log = "Quote Sync Ended";
            AppEventSource.Log.Info(_log);
            _eventAggregator.GetEvent<QuoteSyncChangedEvent>().Publish(Constants.QuotesSyncProgress);
        }

        /// <summary>
        ///     Starts orders sync
        /// </summary>
        /// <returns></returns>
        public async Task StartOrdersSync()
        {
            Constants.OrdersSyncProgress = true;
            _log = "Order Sync Started";
            AppEventSource.Log.Info(_log);
            _eventAggregator.GetEvent<OrderSyncChangedEvent>().Publish(Constants.OrdersSyncProgress);
            await _orderService.StartOrdersSyncProcess();
            Constants.OrdersSyncProgress = false;
            _log = "Order Sync Ended";
            AppEventSource.Log.Info(_log);
            _eventAggregator.GetEvent<OrderSyncChangedEvent>().Publish(Constants.OrdersSyncProgress);
        }
    }
}