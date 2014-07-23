using System;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Certificates;
using Windows.Storage;

namespace Sage.Authorisation.WinRT.Storage
{
    /// <summary>
    ///     CredentialStore handles retreiving and storing the client credentials
    /// </summary>
    internal class CredentialStore
    {
        /// <summary>
        ///     The key which is used to store the credential expiry in local application settings
        /// </summary>
        private const string CertificateExpiryKey = "CertificateExpiry";

        private readonly string _clientId;
        private readonly HttpHelper _httpHelper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CredentialStore" /> class.
        /// </summary>
        /// <param name="clientId">The client id.</param>
        /// <param name="_httpHelper">The http helper.</param>
        public CredentialStore(string clientId, HttpHelper httpHelper)
        {
            _httpHelper = httpHelper;
            _clientId = clientId;
        }

        /// <summary>
        ///     Determines whether this application has a valid credential.
        ///     <remarks>
        ///         There is no way to programatically delete the credentials, this application setting
        ///         may return false but a credential might actually exist. Clearing this flag will force a new credential
        ///         to be retrieved but the old credential may still be used if it's "in date" due to the automatic selection of
        ///         credentials.
        ///     </remarks>
        /// </summary>
        /// <returns>
        ///     <c>true</c> if there is a valid credential; otherwise, <c>false</c>.
        /// </returns>
        internal bool HasValidCredential()
        {
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey(_clientId + CertificateExpiryKey))
            {
                return false;
            }

            var expiryAsUtcFileTime =
                (long) ApplicationData.Current.LocalSettings.Values[_clientId + CertificateExpiryKey];
            DateTime expiry = DateTime.FromFileTimeUtc(expiryAsUtcFileTime);
            if (expiry > DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Clears the credential metadata, this will force a new credential to be retrieved during the next authorisation.
        ///     <remarks>This doesn't remove the actual credential. I can't find a programatic way to do that.</remarks>
        /// </summary>
        internal void ClearCredentialMetadata()
        {
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(_clientId + CertificateExpiryKey))
            {
                ApplicationData.Current.LocalSettings.Values.Remove(_clientId + CertificateExpiryKey);
            }
        }

        /// <summary>
        ///     Retrieves the credential from SageID and imports it into the local credential store.
        ///     <remarks>
        ///         Credentials are stored here...
        ///         C:\Users\{username}\AppData\Local\Packages\{AppPackageName}\AC\Microsoft\SystemCertificates\My\Certificates
        ///     </remarks>
        /// </summary>
        /// <param name="accessCode">The access code.</param>
        /// <param name="state">The state.</param>
        /// <returns>Task for asynchronus</returns>
        internal async Task RetrieveAndImportCredentialAsync(string accessCode, string deviceName, string state)
        {
            string password = PasswordGenerator.NewPassword(8);

            CertificateMetadata certificate =
                await _httpHelper.GetClientCredentialAsync(accessCode, password, deviceName, state);

            await ImportCertificateAsync(certificate.credential, password, certificate.friendly_name);

            StoreCertMetadata(certificate);
        }

        /// <summary>
        ///     Imports the certificate into the local store
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        /// <param name="password">The password.</param>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <returns></returns>
        private async Task ImportCertificateAsync(string certificate, string password, string friendlyName)
        {
            await CertificateEnrollmentManager.ImportPfxDataAsync(certificate
                , password
                , ExportOption.NotExportable
                , KeyProtectionLevel.NoConsent
                , InstallOptions.DeleteExpired
                , friendlyName);
        }

        /// <summary>
        ///     Stores the cert metadata in the application settings
        /// </summary>
        /// <param name="certificate">The certificate.</param>
        private void StoreCertMetadata(CertificateMetadata certificate)
        {
            ApplicationData.Current.LocalSettings.Values[_clientId + CertificateExpiryKey] =
                certificate.expiry.ToFileTimeUtc();
        }
    }
}