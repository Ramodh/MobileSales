using System;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace Sage.Authorisation.WinRT.Storage
{
    /// <summary>
    ///     Encryption manager is a wrapper around symetric key encryption, it handles the key retreival and IVs transparently.
    /// </summary>
    internal class SymmetricEncryptor
    {
        /// <summary>
        ///     The iv data split char, this is used to obtain the IV from the front of the payload to decrypt
        /// </summary>
        private const char IvDataSplitChar = ':';

        private static readonly string EncryptionAlgorith = SymmetricAlgorithmNames.AesCbcPkcs7;

        /// <summary>
        ///     The key manager used to retrieve a key for encryption
        /// </summary>
        private readonly EncryptionKeyStore _keyManager;

        /// <summary>
        ///     The underlying encryption provider
        /// </summary>
        private readonly SymmetricKeyAlgorithmProvider _provider;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SymmetricEncryptor" /> class.
        /// </summary>
        internal SymmetricEncryptor()
        {
            _provider = SymmetricKeyAlgorithmProvider.OpenAlgorithm(EncryptionAlgorith);
            _keyManager = new EncryptionKeyStore(_provider);
        }

        /// <summary>
        ///     Asynchronously encrypts the specified plain text.
        /// </summary>
        /// <param name="plainText">The plain text.</param>
        /// <returns>
        ///     Returns a Task&lt;string&gt; which represents an asynchronus operation. The cypher text is returned on
        ///     completion of the Task
        /// </returns>
        internal async Task<string> EncryptAsync(string plainText)
        {
            Task<CryptographicKey> keyTask = _keyManager.GetCryptographicKeyAsync();

            IBuffer iv = CryptographicBuffer.GenerateRandom(_provider.BlockLength);

            CryptographicKey key = await keyTask;

            IBuffer cypherData = CryptographicEngine.Encrypt(
                key,
                CryptographicBuffer.ConvertStringToBinary(plainText, BinaryStringEncoding.Utf8),
                iv);

            string ivString = CryptographicBuffer.EncodeToBase64String(iv);
            string cypherText = CryptographicBuffer.EncodeToBase64String(cypherData);

            return ivString + IvDataSplitChar + cypherText;
        }

        /// <summary>
        ///     Asynchronously decrypts the specified data.
        /// </summary>
        /// <param name="data">The encrypted data.</param>
        /// <returns>
        ///     Returns a Task&lt;string&gt; which represents an asynchronus operation. The clear text is returned on
        ///     completion of the Task
        /// </returns>
        internal async Task<string> DecryptAsync(IBuffer data)
        {
            // start getting the key, we'll await this when we have done everything else
            Task<CryptographicKey> keyTask = _keyManager.GetCryptographicKeyAsync();

            string cypherText = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, data);

            // extracts the IV from the front of the cypher text
            string[] parts = cypherText.Split(IvDataSplitChar);
            if (parts.Length != 2)
            {
                throw new Exception("Cannot find IV for cypher text");
            }

            IBuffer iv = CryptographicBuffer.DecodeFromBase64String(parts[0]);
            IBuffer cypherData = CryptographicBuffer.DecodeFromBase64String(parts[1]);

            // now we've done everything else we need, ensure we've got the key
            CryptographicKey key = await keyTask;

            IBuffer clearData = CryptographicEngine.Decrypt(key, cypherData, iv);

            string clearText = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, clearData);
            return clearText;
        }
    }
}