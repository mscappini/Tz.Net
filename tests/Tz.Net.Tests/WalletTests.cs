using NUnit.Framework;
using System.Linq;
using Tz.Net.Security;

namespace Tz.Net.Tests
{
    [TestFixture(Category = "Wallet")]
    public class WalletTests
    {
        // I'm not sure why, but I couldn't get InternalsVisibleTo working with Tz.Net.Tests from Tz.Net.
        // So I'm just abandoning testing exact matches on the keys so long as the public hash checks out
        // and the key lengths are not null and length > 0.

        //string decryptedPublicKey = wallet.DecryptPublicKey(); // TODO: Make this work from Tz.Net.Tests.
        //string decryptedPrivateKey = wallet.DecryptPrivateKey(); // TODO: Make this work from Tz.Net.Tests.

        [Test]
        public void CreateWallet_NoSeed()
        {
            Crypto c = new Crypto();

            Wallet wallet = new Wallet();

            Assert.IsNotNull(wallet);
            Assert.IsNull(wallet.Mnemonic);
            Assert.IsNull(wallet.Passphrase);
            Assert.IsNotNull(wallet.Seed);
            Assert.IsTrue(wallet.Seed.Length == 32);
            Assert.IsNotNull(wallet.Keys.PublicHash);
            Assert.IsTrue(wallet.Keys.PublicHash.StartsWith("tz1"));
            Assert.IsNotNull(wallet.Keys.PublicKey);
            Assert.Greater(wallet.Keys.PublicKey.Length, 0);
            Assert.IsNotNull(wallet.Keys.PrivateKey);
            Assert.Greater(wallet.Keys.PrivateKey.Length, 0);
        }

        [Test]
        public void CreateWallet_WithMnemonic()
        {
            Crypto c = new Crypto();

            const string mnemonic = "feed ring nasty mean soon left mad certain rebel empty permit like session clutch robust";
            const string email = "watylroc.pcatxegk@tezos.example.org";
            const string password = "UBoCoEMCuS";

            Wallet wallet = new Wallet(mnemonic, email, password);

            Assert.IsNotNull(wallet);
            Assert.AreEqual(32, wallet.Seed.Length);
            Assert.AreEqual(mnemonic, string.Join(' ', wallet.Mnemonic));
            Assert.AreEqual(email + password, wallet.Passphrase);
            Assert.AreEqual("tz1hmK2ru6ism15MxXbnhKWWKGJ6hqWssMc5", wallet.Keys.PublicHash);
            Assert.IsNotNull(wallet.Keys.PublicKey);
            Assert.Greater(wallet.Keys.PublicKey.Length, 0);
            Assert.IsNotNull(wallet.Keys.PrivateKey);
            Assert.Greater(wallet.Keys.PrivateKey.Length, 0);
        }

        [Test]
        public void CreateWallet_WithPassphrase()
        {
            Crypto c = new Crypto();

            const string passphrase = "t3z0sR0ckz!";

            Wallet wallet = new Wallet(passphrase);

            Assert.IsNotNull(wallet);
            Assert.AreEqual(32, wallet.Seed.Length);
            Assert.AreEqual(15, wallet.Mnemonic.Count());
            Assert.AreEqual(passphrase, wallet.Passphrase);
            Assert.AreEqual(36, wallet.Keys.PublicHash.Length);
            Assert.IsNotNull(wallet.Keys.PublicKey);
            Assert.Greater(wallet.Keys.PublicKey.Length, 0);
            Assert.IsNotNull(wallet.Keys.PrivateKey);
            Assert.Greater(wallet.Keys.PrivateKey.Length, 0);
        }

        [Test]
        public void CreateWallet_WithSeed()
        {
            Crypto c = new Crypto();

            byte[] seed = { 144, 252, 66, 143, 33, 90, 245, 35, 86, 154, 51, 81, 72, 245, 25, 51, 228, 74, 189, 43, 87, 91, 100, 173, 188, 84, 16, 139, 104, 33, 11, 245 };

            Wallet wallet = new Wallet(seed);

            Assert.IsNotNull(wallet);
            Assert.IsTrue(wallet.Seed.Length == 32);
            Assert.IsNull(wallet.Mnemonic);
            Assert.IsNull(wallet.Passphrase);
            Assert.AreEqual("tz1hmK2ru6ism15MxXbnhKWWKGJ6hqWssMc5", wallet.Keys.PublicHash);
            Assert.IsNotNull(wallet.Keys.PublicKey);
            Assert.Greater(wallet.Keys.PublicKey.Length, 0);
            Assert.IsNotNull(wallet.Keys.PrivateKey);
            Assert.Greater(wallet.Keys.PrivateKey.Length, 0);
        }
    }
}