using System;
using System.Runtime.InteropServices;
using System.Security;
using Tz.Net.Internal;

namespace Tz.Net.Security
{
    public sealed class Keys
    {
        private const int PKHASH_BIT_SIZE = 20 * 8;

        public SecureString PublicKey { get; internal set; }
        public SecureString PrivateKey { get; internal set; }
        public string PublicHash { get; internal set; }

        public Keys()
        { }

        public Keys(byte[] pk, byte[] sk)
        {
            PublicHash = B58C.Encode(Hashing.Generic(PKHASH_BIT_SIZE, pk), Prefix.tz1);

            PublicKey = new SecureString();
            PrivateKey = new SecureString();

            string encodedPK = B58C.Encode(pk, Prefix.edpk);
            foreach (char c in encodedPK)
            {
                PublicKey.AppendChar(c);
            }

            string encodedSK = B58C.Encode(sk, Prefix.edsk);
            foreach (char c in encodedSK)
            {
                PrivateKey.AppendChar(c);
            }
            
            // Quickly zero out the unneeded key arrays so it doesn't linger in memory before the GC can sweep it up.
            Array.Clear(pk, 0, pk.Length);
            Array.Clear(sk, 0, sk.Length);
        }

        /// <summary>
        /// Do not store this result on the heap!
        /// </summary>
        /// <returns>Decrypted public key</returns>
        internal string DecryptPublicKey()
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(PublicKey);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }

        /// <summary>
        /// Do not store this result on the heap!
        /// </summary>
        /// <returns>Decrypted private key</returns>
        internal string DecryptPrivateKey()
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(PrivateKey);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}