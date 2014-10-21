using System;
using Windows.Foundation;
using Windows.Storage;
using SageMobileSales.DataAccess.Entities;
using SageMobileSales.DataAccess.Model;
using SageMobileSales.ServiceAgents.Common;

namespace SageMobileSales.UILogic.Common
{
    public static class PageUtils
    {
        # region StaticVariables

        public static string IsAuthorised = "IsAuthorised";
        public static string TrackingId = string.Empty;
        public static string AccessToken = string.Empty;
        public static string IsLaunched = "IsLaunched";
        public static string CustomerName = "Customer Name";
        public static string SalesPerson = "Sales Person";
        public static string Date = "Date";
        public static string Status = "Status";
        public static string Amount = "Amount";
        public static string Scratch = "Scratch";
        public static string UnitPrice = "UnitPrice";
        public static string Quantity = "Quantity";
        public static string Invoice = "Invoice";
        public static string QuoteDetailsPage = "QuoteDetailsPage";
        public static string ItemDetailPage = "ItemDetailPage";
        public static string PreviousOrder = "Previous order";
        public static string PreviousPurchasedItems = "Previously purchased items";
        public static string ScratchText = "Create the quote from scratch";
        public static string PreviousOrderText = "Use an existing order as a template";

        public static string PreviousPurchasedItemsText =
            "Select from a list of items previously purchased by the customer";

        public static string Pending = "Pending";
        public static string OrderNumber = "Order Number";

        public static string Country = "USA";
        public static string Other = "Other";
        public static string SelectedQuoteId = string.Empty;
        public static Product SelectedProduct;
        public static bool CamefromQuoteDetails = false;
        public static bool CamefromItemDetails = false;
        public static string CustomerDetailPage = "CustomerDetailPage";
        public static string OrdersPage = "OrdersPage";
        public static string CreateQuotePage = "CreateQuotePage";
        public static string QuotesPage = "QuotesPage";
        public static string SeeMore = "See More";
        public static bool CamefromCustomerDetails = false;
        public static Customer SelectedCustomer = null;
        public static CustomerDetails SelectedCustomerDetails = null;
        public static IAsyncAction asyncActionCommon;
        public static IAsyncAction asyncActionQuotes;
        public static IAsyncAction asyncActionOrders;
        public static IAsyncAction asyncActionSalesRep;
        public static IAsyncAction asyncActionProducts;
        public static bool CameFromOrderDetail = false;
        public static string OrderSortType = string.Empty;
        public static string OrderSortBy = string.Empty;
        public static string QuoteSortType = string.Empty;
        public static string QuoteSortBy = string.Empty;
        public static bool CameFromQuoteDetail = false;

        # endregion

        # region Static Methods

        public static void GetApplicationData()
        {
            ApplicationDataContainer settingsLocal = ApplicationData.Current.LocalSettings;
            if (settingsLocal.Containers.ContainsKey("SageSalesContainer"))
            {
                Constants.AccessToken = settingsLocal.Containers["SageSalesContainer"].Values["AccessToken"]==null?null:settingsLocal.Containers["SageSalesContainer"].Values["AccessToken"].ToString();
                Constants.TrackingId = settingsLocal.Containers["SageSalesContainer"].Values["TrackingId"] == null ? null : settingsLocal.Containers["SageSalesContainer"].Values["TrackingId"].ToString();
                if (settingsLocal.Containers["SageSalesContainer"].Values["syncQueryEntity"] != null)
                {
                    Constants.syncQueryEntity =
                        settingsLocal.Containers["SageSalesContainer"].Values["syncQueryEntity"].ToString();
                }
            }
        }

        public static void GetConfigurationSettings()
        {
#if(PRODUCTION)  
             
            Constants.ClientId = @"7XtkBANAnzQ9a1aRqspNlHR5UtSRUP0J";
            Constants.Url = "https://mobilesales.na.sage.com/sdata/api/dynamic/-/";
            Constants.Scope = @"hrixtfgl();";
            Constants.RedirectUrl = "https://signon.sso.services.sage.com/oauth/native";
            Constants.IsSageIdProduction = true;
            Constants.SelectedType = "Production";
      #else

            Constants.ClientId = @"xeQaIFwQvbDjKZvwzb92kFOLzDCd9CCc";
            Constants.Url = "http://ashmsalessdcweb.cloudapp.net/sdata/api/msales/1.0/";
            Constants.Scope = @"k1mcudfb();";
            Constants.IsSageIdProduction = false;
            Constants.SelectedType = "Ash";
            Constants.RedirectUrl = "https://signon.sso.staging.services.sage.com/oauth/native";

#endif

            ApplicationDataContainer configSettings = ApplicationData.Current.LocalSettings;
            if (configSettings.Containers != null)
            {
                if (configSettings.Containers.ContainsKey("ConfigurationSettingsContainer"))
                {
                    Constants.ClientId =
                        configSettings.Containers["ConfigurationSettingsContainer"].Values["ClientId"].ToString();
                    Constants.Scope =
                        configSettings.Containers["ConfigurationSettingsContainer"].Values["Scope"].ToString();
                    Constants.Url = configSettings.Containers["ConfigurationSettingsContainer"].Values["Url"].ToString();
                    Constants.RedirectUrl =
                        configSettings.Containers["ConfigurationSettingsContainer"].Values["RedirectUrl"].ToString();
                    Constants.IsSageIdProduction =
                        Convert.ToBoolean(
                            configSettings.Containers["ConfigurationSettingsContainer"].Values["IsSageProduction"]);
                    Constants.SelectedType =
                        configSettings.Containers["ConfigurationSettingsContainer"].Values["SelectedType"].ToString();
                }
            }
        }

        public static void ResetLocalVariables()
        {
            try
            {
                // PageUtils.SelectedProduct = null;
                SelectedCustomer = null;
                SelectedCustomerDetails = null;
            }

            catch (Exception ex)
            {
                throw (ex);
            }
        }

        # endregion
    }
}