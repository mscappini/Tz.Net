using Base58Check;
using System;

namespace Tz.Net.Internal
{
    /// <summary>
    /// Base58Check utility class.
    /// </summary>
    internal class B58C
    {
        /// <summary>
        /// Base58Check encoding.
        /// </summary>
        /// <param name="payload">The message to encode.</param>
        /// <param name="prefix">The optional prefix to prepend.</param>
        /// <returns>Base58 encoded prefix + payload.</returns>
        public static string Encode(byte[] payload, byte[] prefix = null)
        {
            int prefixLen = prefix?.Length ?? 0;

            byte[] msg = new byte[prefixLen + payload.Length];

            if (prefix != null)
            {
                Array.Copy(prefix, 0, msg, 0, prefix.Length);
            }

            Array.Copy(payload, 0, msg, prefixLen, payload.Length);

            return Base58CheckEncoding.Encode(msg);
        }

        /// <summary>
        /// Base58Check decoding.
        /// </summary>
        /// <param name="payload">The message to decode.</param>
        /// <param name="prefix">The prefix to truncate.</param>
        /// <returns>Base58 decoded message.</returns>
        public static byte[] Decode(string encoded, byte[] prefix = null)
        {
            int prefixLen = prefix?.Length ?? 0;

            byte[] msg = Base58CheckEncoding.Decode(encoded);

            byte[] result = new byte[msg.Length - prefixLen];

            Array.Copy(msg, prefixLen, result, 0, result.Length);

            return result;
        }
    }
}