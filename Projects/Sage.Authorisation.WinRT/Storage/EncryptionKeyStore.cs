using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography.DataProtection;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Sage.Authorisation.WinRT.Storage
{
    /// <summary>
    ///     KeyManager handles creating, storing and retreiving of keys to be used by the symetric encryption algorithm
    /// </summary>
    internal class EncryptionKeyStore
    {
        /// <summary>
        ///     The key length
        /// </summary>
        private const int KeyLength = 256;

        /// <summary>
        ///     The "key" used for storing the encrytpion keys in application data
        /// </summary>
        private const string EncryptionKeyKey = "sk.oa";

        /// <summary>
        ///     The DPAPI provider for encrypting the keys
        /// </summary>
        private readonly DataProtectionProvider _dpProvider = new DataProtectionProvider("LOCAL=user");

        /// <summary>
        ///     The encryption provider
        /// </summary>
        private readonly SymmetricKeyAlgorithmProvider _provider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EncryptionKeyStore" /> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        internal EncryptionKeyStore(SymmetricKeyAlgorithmProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        ///     Asynchronously gets the cryptographic key.
        /// </summary>
        /// <returns></returns>
        internal async Task<CryptographicKey> GetCryptographicKeyAsync()
        {
            CryptographicKey key;
            StorageFile file;

            try
            {
                // checks the local storage to see if a key already exists
                file = await ApplicationData.Current.LocalFolder.GetFileAsync(EncryptionKeyKey);
            }
            catch (FileNotFoundException)
            {
                key = CreateKey();

                return key;
            }
            catch
            {
                throw;
            }

            // load the contents of the key material into a buffer
            IBuffer encryptedKey = await FileIO.ReadBufferAsync(file);

            // un-encrypt it using the DPAPI
            IBuffer clearKey = await _dpProvider.UnprotectAsync(encryptedKey);

            // re-create the key
            key = _provider.CreateSymmetricKey(clearKey);

            return key;
        }

        /// <summary>
        ///     Creates the key.
        /// </summary>
        /// <returns>A CryptographicKey object</returns>
        private CryptographicKey CreateKey()
        {
            IBuffer keyMaterial = CryptographicBuffer.GenerateRandom(KeyLength);

            // not awaiting this - we don't need  to see it stored before we return the actual key for use
            StoreKeyMaterialAsync(keyMaterial);

            CryptographicKey key = _provider.CreateSymmetricKey(keyMaterial);

            return key;
        }

        /// <summary>
        ///     Asynchronously stores the key material in application data.
        /// </summary>
        /// <param name="keyMaterial">The key material.</param>
        /// <returns></returns>
        private async void StoreKeyMaterialAsync(IBuffer keyMaterial)
        {
            // protect the key material using the DPAPI
            IBuffer protectedKey = await _dpProvider.ProtectAsync(keyMaterial);

            // Create a file in application data area
            StorageFile file =
                await
                    ApplicationData.Current.LocalFolder.CreateFileAsync(EncryptionKeyKey,
                        CreationCollisionOption.ReplaceExisting);

            // write the key material to the file
            await FileIO.WriteBufferAsync(file, protectedKey);
        }
    }
}