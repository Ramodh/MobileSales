using System;
using Windows.Networking.Connectivity;
using Windows.Security.Cryptography;
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

        public static string CurrentUserEnitty = "CurrentUser";
        public static string GetSalesSettingsEntity = "Users";
        public static string CategoryEntity = "InventoryCategories";
        public static string ItemsEntity = "InventoryItems";
        public static string BlobsEntity = "Blobs";
        public static string CustomerEntity = "Customers";
        public static string ContactEntity = "Contacts";
        public static string QuoteEntity = "Quotes";
        public static string QuoteDetailEntity = "QuoteDetails";
        public static string OrderEntity = "Orders";
        public static string TenantEntity = "Tenants";
        public static string ClientId = string.Empty;

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

        public static string AssociatedItems = "AssociatedItems/id";
        public static string AssociatedBlobs = "AssociatedBlobs/*,RelatedItems/id";

        # endregion

        # region Static Methods

        /// <summary>
        ///     Gets the Device Unique Id
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceId()
        {
            HardwareToken token = HardwareIdentification.GetPackageSpecificToken(null);
            string deviceId = CryptographicBuffer.EncodeToBase64String(token.Id);
            return deviceId;
        }

        /// <summary>
        ///     Checks whether Internet is Connected (or) not
        /// </summary>
        /// <returns>bool</returns>
        public static bool ConnectedToInternet()
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (InternetConnectionProfile == null)
            {
                return false;
            }
            NetworkConnectivityLevel level = InternetConnectionProfile.GetNetworkConnectivityLevel();
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