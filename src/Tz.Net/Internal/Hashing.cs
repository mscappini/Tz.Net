using Konscious.Security.Cryptography;

namespace Tz.Net.Internal
{
    internal class Hashing
    {
        /// <summary>
        /// Blake2B hash a byte array.
        /// </summary>
        /// <param name="hashSizeInBits">Hash size in bits (8 is one byte).</param>
        /// <param name="msg">The message to hash.</param>
        /// <returns>Hashed message.</returns>
        public static byte[] Generic(int hashSizeInBits, byte[] msg)
        {
            HMACBlake2B blake2b = new HMACBlake2B(hashSizeInBits);

            blake2b.Initialize();

            return blake2b.ComputeHash(msg);
        }
    }
}