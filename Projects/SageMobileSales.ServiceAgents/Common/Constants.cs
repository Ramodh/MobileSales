using System;
using Windows.Networking.Connectivity;
using Windows.Storage.Streams;
using Windows.System.Profile;
using Windows.UI.Popups;
using Newtonsoft.Json;

namespace SageMobileSales.ServiceAgents.Common
{
    public static class Constants
    {
        public static string IsAuthorised = "IsAuthorised";
        public static string TrackingId = string.Empty;
        public static string AccessToken = string.Empty;
        public static bool IsSyncAvailable = false;
        public static string Pending = "Pending";
        public static bool IsDbDeleted = false;
        public static string SubmitQuote = "Submitted";

        #region SyncCheck

        public static bool SyncProgress = false;
        public static bool ProductsSyncProgress = false;
        public static bool QuotesSyncProgress = false;
        public static bool OrdersSyncProgress = false;
        public static bool CustomersSyncProgress = false;

        #endregion

        # region Entities

        public static string TenantId = string.Empty;
        public static string CurrentUserEnitty = "-/CurrentUser";
        public static string AppSalesUser = "AppSalesUser";
        public static string CompanySettings = "CompanySettings";
        public static string UpdateUserEntity = "SalesTeamMembers";
        public static string GetSalesSettingsEntity = "Users";
        public static string CategoryEntity = "InventoryCategories";
        public static string ItemsEntity = "InventoryItems";
        public static string BlobsEntity = "Files";
        public static string CustomerEntity = "MyCustomers";
        public static string CustomerDetailEntity = "AppSalesCustomers";
        //public static string CustomerDetailEntity = "Customers";
        public static string ContactEntity = "Contacts";
        public static string QuoteEntity = "MyQuotes";
        public static string QuoteDetailEntity = "Quotes";
        public static string OrderEntity = "MyOrders";
        public static string OrderDetailEntity = "Orders";
        public static string TenantEntity = "Tenants";
        public static string ClientId = string.Empty;

        public static string DraftQuotes = "DraftQuotes";
        public static string SubmitQuoteEntity = "QuoteRequests";
        public static string Address = "Addresses";
        public static string FrequentlyPurchasedItem = "FrequentlyPurchasedByCustomers";
        public static string QuoteToOrder = "QuoteToOrderRequests";
        public static string CustomerSalesHistory = "CustomerSalesHistory";


        public static string Scope = String.Empty;
        public static string Url = string.Empty;
        public static string Server = string.Empty;
        public static string RedirectUrl = string.Empty;
        public static bool IsSageIdProduction = false;
        public static string SelectedType = string.Empty;

        # endregion

        # region QueryEntities

        public static string GetSalesSettingsQueryEntity = "/$queries/GetSalesSettings";
        public static string syncDigestQueryEntity = "$SyncDigest";
        public static string syncSourceQueryEntity = "$SyncSource";
        public static string syncQueryEntity = string.Empty;

        # endregion

        #region AssociatedItems

        public static string AssociatedItems = "Parent&select=*,AssociatedItems/Id,Parent/Id";
        public static string AssociatedBlobs = "Images/*,RelatedItems/Id";

        # endregion

        # region Static Methods

        /// <summary>
        ///     Gets the Device Unique Id
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceId()
        {
            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;
            var dataReader = DataReader.FromBuffer(hardwareId);

            //var bytes = new byte[hardwareId.Length];
            //dataReader.ReadBytes(bytes);
            var deviceId = dataReader.ReadGuid().ToString();

            //return BitConverter.ToString(bytes);
            return deviceId;
        }

        /// <summary>
        ///     Checks whether Internet is Connected (or) not
        /// </summary>
        /// <returns>bool</returns>
        public static bool ConnectedToInternet()
        {
            var InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (InternetConnectionProfile == null)
            {
                return false;
            }
            var level = InternetConnectionProfile.GetNetworkConnectivityLevel();
            return level == NetworkConnectivityLevel.InternetAccess;
        }

        public static async void ShowMessageDialog(string messageText)
        {
            var msgDialog = new MessageDialog(messageText);
            await msgDialog.ShowAsync();
        }

        // <summary>
        /// Parses a json string and return an instance of T.
        /// </summary>
        /// <typeparam name="T">The expected type described by the JSON string</typeparam>
        /// <param name="jsonString">The json String.</param>
        /// <returns></returns>
        public static T ParseJsonString<T>(string jsonString)
        {
            var settings = new JsonSerializerSettings();

            settings.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
            settings.DateParseHandling = DateParseHandling.DateTime;
            settings.MissingMemberHandling = MissingMemberHandling.Ignore;

            var result = JsonConvert.DeserializeObject<T>(jsonString, settings);
            return result;
        }

        # endregion
    }
}