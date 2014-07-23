using System.Text;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace Sage.Authorisation.WinRT.Storage
{
    /// <summary>
    ///     Base32 encoder is for generating encoding random data in a safe way to use as filenames. Base32 encoding isn't
    ///     available by default in WinRT.
    ///     This is taken from a SSG project (I think, ask Darrell)
    /// </summary>
    internal class Base32Encoder
    {
        // Base 32 encoding constants
        private const int IN_BYTE_SIZE = 8;
        private const int OUT_BYTE_SIZE = 5;
        private static readonly char[] alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

        /// <summary>
        ///     Encode to base 32 according to RFC3548
        /// </summary>
        /// <param name="buffer">buffer to encode</param>
        /// <returns>
        ///     The buffer encoded as a string
        /// </returns>
        internal static string Base32Encode(IBuffer buffer)
        {
            byte[] data = null;

            CryptographicBuffer.CopyToByteArray(buffer, out data);

            int i = 0;
            int index = 0;
            int digit = 0;
            int current_byte;
            int next_byte;

            var result = new StringBuilder();

            while (i < data.Length)
            {
                // unsign
                current_byte = (data[i] >= 0) ? data[i] : (data[i] + 256);

                // is the current digit going to span a byte boundary
                if (index > (IN_BYTE_SIZE - OUT_BYTE_SIZE))
                {
                    if ((i + 1) < data.Length)
                        next_byte = (data[i + 1] >= 0) ? data[i + 1] : (data[i + 1] + 256);
                    else
                        next_byte = 0;

                    digit = current_byte & (0xFF >> index);
                    index = (index + OUT_BYTE_SIZE)%IN_BYTE_SIZE;
                    digit <<= index;
                    digit |= next_byte >> (IN_BYTE_SIZE - index);
                    i++;
                }
                else
                {
                    digit = (current_byte >> (IN_BYTE_SIZE - (index + OUT_BYTE_SIZE))) & 0x1F;
                    index = (index + OUT_BYTE_SIZE)%IN_BYTE_SIZE;
                    if (index == 0)
                        i++;
                }

                result.Append(alphabet[digit]);
            }

            // trim the result to a specific size
            return result.ToString();

            //return result.ToString().Substring(0, size <= result.Length ? size : result.Length);
        }
    }
}