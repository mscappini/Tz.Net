using System.Runtime.InteropServices;

namespace Tz.Net.Internal
{
    internal class Sodium
    {
        //int crypto_sign_seed_keypair(unsigned char* pk, unsigned char* sk, const unsigned char* seed)
        //{
        //    return crypto_sign_ed25519_seed_keypair(pk, sk, seed);
        //}
        [DllImport("libsodium.dll")]
        public static extern int crypto_sign_seed_keypair(byte[] pk, byte[] sk, byte[] seed);

        //int crypto_generichash(unsigned char*out, size_t outlen, const unsigned char*in,
        //           unsigned long long inlen, const unsigned char* key,
        //           size_t keylen)
        //{
        //    return crypto_generichash_blake2b(out, outlen, in, inlen, key, keylen);
        //}
        [DllImport("libsodium.dll")]
        public static extern int crypto_generichash(byte[] outt, uint outlen, byte[] inn, ulong inlen, byte[] key, uint keylen);

        [DllImport("libsodium.dll")]
        public static extern byte[] crypto_generichash(int hashlength, byte[] key);
    }
}