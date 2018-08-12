using NUnit.Framework;
using Tz.Net.Security;

namespace Tz.Net.Tests
{
    [TestFixture(Category = "Crypto")]
    public class CryptoTests
    {
        [Test]
        public void CheckAddress_Invalid()
        {
            Crypto c = new Crypto();

            bool valid = c.CheckAddress("tz1.GoodHash.BadHash.ImTheGuyWithTheGun."); // Nope.

            Assert.IsFalse(valid);
        }

        [Test]
        public void CheckAddress_Valid()
        {
            Crypto c = new Crypto();

            bool valid = c.CheckAddress("tz1hmK2ru6ism15MxXbnhKWWKGJ6hqWssMc5"); // Yep.

            Assert.IsTrue(valid);
        }

        [Test]
        public void CreateMnemonic()
        {
            Crypto c = new Crypto();

            string words = c.GenerateMnemonic();

            Assert.IsNotNull(words);
            Assert.IsNotEmpty(words);
            Assert.AreEqual(15, words.Split(' ').Length);
        }
    }
}