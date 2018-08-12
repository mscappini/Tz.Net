using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Tz.Net.Extensions;
using Tz.Net.Security;

namespace Tz.Net
{
    public class Wallet
    {
        /// <summary>
        /// Create wallet in a non-deterministic fashion.
        /// </summary>
        public Wallet()
        {
            byte[] seed = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                // Fill the array with a random seed
                rng.GetBytes(seed);
            }

            FromSeed(seed);
        }

        /// <summary>
        /// Create wallet in a deterministic fashion. Mnemonic is randomly generated and accessible.
        /// </summary>
        /// <param name="passphrase">Passphrase to generate seed</param>
        public Wallet(string passphrase)
        {
            Crypto c = new Crypto();

            string mnemonic = c.GenerateMnemonic();

            FromMnemonic(mnemonic, passphrase);
        }

        /// <summary>
        /// Create wallet from mnemonic, email, and password with deterministic seed.
        /// </summary>
        /// <param name="mnemonic">15 word mnemonic</param>
        /// <param name="email">Email address</param>
        /// <param name="password">Wallet password</param>
        public Wallet(string mnemonic, string email, string password)
            : this(mnemonic, email + password)
        { }

        /// <summary>
        /// Create wallet from mnemonic and passphrase with deterministic seed.
        /// </summary>
        /// <param name="mnemonic">15 word mnemonic</param>
        /// <param name="passphrase">Passphrase to generate seed</param>
        public Wallet(string mnemonic, string passphrase)
        {
            FromMnemonic(mnemonic, passphrase);
        }

        /// <summary>
        /// Create wallet from mnemonic words, email, and password with deterministic seed.
        /// </summary>
        /// <param name="mnemonic">15 word mnemonic</param>
        /// <param name="email">Email address</param>
        /// <param name="password">Wallet password</param>
        public Wallet(IEnumerable<string> words, string email, string password)
            : this(words, email + password)
        { }

        /// <summary>
        /// Create wallet from mnemonic and passphrase with deterministic seed.
        /// </summary>
        /// <param name="mnemonic">15 word mnemonic</param>
        /// <param name="passphrase">Usually email + password.</param>
        public Wallet(IEnumerable<string> words, string passphrase)
        {
            FromMnemonic(words, passphrase);
        }

        /// <summary>
        /// Generate wallet from seed.
        /// </summary>
        /// <param name="seed">Seed to generate keys with.</param>
        public Wallet(byte[] seed)
        {
            FromSeed(seed);
        }

        private void FromMnemonic(string mnemonic, string passphrase)
        {
            if (string.IsNullOrEmpty(mnemonic))
            {
                throw new ArgumentException("Mnemonic must be 15 words", nameof(mnemonic));
            }

            string[] words = mnemonic.Split(' ');

            if (words.Length != 15)
            {
                throw new ArgumentException("Mnemonic must be 15 words", nameof(mnemonic));
            }

            FromMnemonic(words, passphrase);
        }

        private void FromMnemonic(IEnumerable<string> words, string passphrase)
        {
            if (words?.Any() == false)
            {
                throw new ArgumentException("Words required", nameof(words));
            }

            if (string.IsNullOrWhiteSpace(passphrase))
            {
                throw new ArgumentException("Passphrase required", nameof(passphrase));
            }

            Mnemonic = words;

            Passphrase = passphrase;

            Bip39 bip39 = new Bip39();

            byte[] seed = bip39.ToSeed(words, passphrase).CopyOfRange(0, 32);

            FromSeed(seed);
        }

        private void FromSeed(byte[] seed)
        {
            if (seed?.Any() == false)
            {
                throw new ArgumentException("Seed required", nameof(seed));
            }

            Seed = seed;

            // Create key pair (PK and SK) from a seed.
            byte[] pk = new byte[32];

            byte[] sk = new byte[64];

            try
            {
                Ed25519.KeyPairFromSeed(out pk, out sk, Seed);

                // Also creates the tz1 PK hash.
                Keys = new Keys(pk, sk);
            }
            finally
            {
                // Keys should clear this, but clear again for good measure.
                Array.Clear(pk, 0, pk.Length);
                Array.Clear(sk, 0, sk.Length);
            }
        }

        /// <summary>
        /// Activate this wallet on the blockchain. This can only be done once.
        /// </summary>
        /// <param name="activationCode">The blinded publich hash used to activate this wallet.</param>
        /// <returns>The result of the activation operation.</returns>
        public async Task<OperationResult> Activate(string activationCode)
        {
            return await new Rpc().Activate(Keys.PublicHash, activationCode);
        }

        /// <summary>
        /// Get the balance of the wallet.
        /// </summary>
        /// <returns>The balance of the wallet.</returns>
        public async Task<BigFloat> GetBalance()
        {
            return await new Rpc().GetBalance(Keys.PublicHash);
        }

        /// <summary>
        /// Transfer funds from one wallet to another.
        /// </summary>
        /// <param name="from">From where to transfer the funds.</param>
        /// <param name="to">To where the funds should be transferred.</param>
        /// <param name="amount">The amount to transfer.</param>
        /// <param name="fee">The fee to transfer.</param>
        /// <param name="gasLimit">The gas limit to transfer.</param>
        /// <param name="storageLimit">The storage limit to transfer.</param>
        /// <returns>The result of the transfer operation.</returns>
        public async Task<OperationResult> Transfer(string from, string to, BigFloat amount, BigFloat fee, BigFloat gasLimit = null, BigFloat storageLimit = null)
        {
            return await new Rpc().SendTransaction(Keys, from, to, amount, fee, gasLimit, storageLimit);
        }

        /// <summary>
        /// The seed used to generate the wallet keys.
        /// </summary>
        public byte[] Seed { get; internal set; }

        /// <summary>
        /// The mnemonic used to generate the seed.
        /// </summary>
        public IEnumerable<string> Mnemonic { get; internal set; }

        /// <summary>
        /// The passphrase used to generate the seed.
        /// </summary>
        public string Passphrase { get; internal set; }

        /// <summary>
        /// The encrypted public/private keys.
        /// </summary>
        public Keys Keys { get; internal set; }
    }
}