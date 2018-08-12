using Newtonsoft.Json;
using Tz.Net.Internal;

namespace Tz.Net.Security
{
    public class SignedMessage
    {
        private const int HASH_SIZE_BITS = 32 * 8;

        [JsonProperty("bytes")]
        public byte[] Bytes { get; internal set; }
        [JsonProperty("sig")]
        public byte[] SignedHash { get; internal set; }
        [JsonProperty("edsig")]
        public string EncodedSignature { get; internal set; }
        [JsonProperty("sbytes")]
        public string SignedBytes { get; internal set; }

        public string HashBytes()
        {
            return B58C.Encode(Hashing.Generic(HASH_SIZE_BITS, Bytes));
        }
    }
}