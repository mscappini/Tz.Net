using Chaos.NaCl;
using System;
using System.Collections.Generic;
using Tz.Net.Extensions;
using Tz.Net.Internal;

namespace Tz.Net.Security
{
    public class Crypto
    {
        /// <summary>
        /// Checks that a tz1 address is valid.
        /// </summary>
        /// <param name="tz1">tz1 address to check for validity.</param>
        /// <returns>True if valid address.</returns>
        public bool CheckAddress(string tz1)
        {
            try
            {
                B58C.Decode(tz1, Prefix.tz1);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Generate a new 15-word mnemonic code.
        /// </summary>
        /// <returns>15 words</returns>
        public string GenerateMnemonic()
        {
            Bip39 bip39 = new Bip39();

            byte[] bytes = new byte[20];

            new Random().NextBytes(bytes);

            List<string> code = bip39.ToMnemonic(bytes);

            return string.Join(" ", code);
        }

        /// <summary>
        /// Sign message with private key.
        /// </summary>
        /// <param name="bytes">Message to sign.</param>
        /// <param name="sk">Secret key used to sign.</param>
        /// <param name="watermark">Watermark</param>
        /// <returns>Signed message.</returns>
        public SignedMessage Sign(string bytes, Keys keys, string watermark = null)
        {
            return Sign(bytes, keys, watermark?.HexToByteArray());
        }

        /// <summary>
        /// Sign message with private key.
        /// </summary>
        /// <param name="bytes">Message to sign.</param>
        /// <param name="sk">Secret key used to sign.</param>
        /// <param name="watermark">Watermark</param>
        /// <returns>Signed message.</returns>
        public SignedMessage Sign(string bytes, Keys keys, byte[] watermark = null)
        {
            byte[] bb = bytes.HexToByteArray();

            if (watermark?.Length > 0)
            {
                byte[] bytesWithWatermark = new byte[bb.Length + watermark.Length];

                Array.Copy(watermark, 0, bytesWithWatermark, 0, watermark.Length);
                Array.Copy(bb, 0, bytesWithWatermark, watermark.Length, bb.Length);

                bb = bytesWithWatermark;
            }

            // TODO: See if there's a way to further reduce potential attack vectors.
            string sk = keys.DecryptPrivateKey();

            byte[] dsk = B58C.Decode(sk, Prefix.edsk);
            byte[] hash = Hashing.Generic(32 * 8, bb);
            byte[] sig = Ed25519.Sign(hash, dsk);
            string edsignature = B58C.Encode(sig, Prefix.edsig);
            string sbytes = bytes + sig.ToHexString();

            return new SignedMessage
            {
                Bytes = bb,
                SignedHash = sig,
                EncodedSignature = edsignature,
                SignedBytes = sbytes
            };
        }
        
        /// <summary>
        /// Verify a signed message with public key.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="sig">Signed message to verify.</param>
        /// <param name="pk">Public key used for verification.</param>
        /// <returns></returns>
        public bool Verify(string bytes, byte[] sig, string pk)
        {
            byte[] bb = bytes.HexToByteArray();

            byte[] edpk = B58C.Decode(pk, Prefix.edpk);

            return Ed25519.Verify(sig, bb, edpk);
        }
    }
}