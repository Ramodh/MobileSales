using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace Sage.Authorisation.WinRT
{
    /// <summary>
    ///     This class is used to generate cryptographically secure passwords that
    ///     will be used to for the PFX containers.
    /// </summary>
    internal static class PasswordGenerator
    {
        private const string PasswordCharacters = "ABCDEFGHJKLMNPQRSTWXYZabcdefgijkmnopqrstwxyz123456789*$-+?_&=!%/";

        internal static string NewPassword(uint length)
        {
            var password = new char[length];
            var randomBytes = new byte[length];

            IBuffer random = CryptographicBuffer.GenerateRandom(length);
            CryptographicBuffer.CopyToByteArray(random, out randomBytes);

            for (int i = 0; i < length; i++)
            {
                password[i] = PasswordCharacters[randomBytes[i]%PasswordCharacters.Length];
            }

            return new string(password);
        }
    }
}