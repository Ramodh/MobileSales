namespace SageMobileSales.DataAccess.Common
{
    public static class DataAccessUtils
    {
        public const string StoreAppsInfrastructureResourceMapId = "/Microsoft.Practices.Prism.StoreApps/Resources/";
        public static int CustomerTotalCount = 0;
        public static int CustomerReturnedCount = 0;
        public static int ProductCategoryTotalCount = 0;
        public static int ProductCategoryReturnedCount = 0;
        public static int ProductTotalCount = 0;
        public static int ProductReturnedCount = 0;
        public static int ProductAssociatedBlobsTotalCount = 0;
        public static int ProductAssociatedBlobsReturnedCount = 0;
        public static int QuotesTotalCount = 0;
        public static int QuotesReturnedCount = 0;
        public static int OrdersTotalCount = 0;
        public static int OrdersReturnedCount = 0;
        public static bool IsCustomerSyncCompleted = false;
        public static bool IsProductCategorySyncCompleted = false;
        public static bool IsProductSyncCompleted = false;
        public static bool IsProductAssociatedBlobsSyncCompleted = false;
        public static bool IsQuotesSyncCompleted = false;
        public static bool IsOrdersSyncCompleted = false;
        public static string Null = "Null";
        public static string IsOrderQuoteStatus = "IsOrder";
        public static string TemporaryQuoteStatus = "Temporary";
        public static string Pending = "Pending";
        public static string DraftQuote = "Draft";
        public static string SubmitQuote = "Submitted";
        public static string Quote = "Quote";
        public static string Scratch = "Scratch";
        public static string PreviousOrder = "Previous order";
        public static string PreviousPurchasedItems = "Previously purchased items";
        public static string SelectedServerType = string.Empty;
        public static bool IsServerChanged = false;
    }
}