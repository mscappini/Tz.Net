using NUnit.Framework;
using Tz.Net.Security;

namespace Tz.Net.Tests
{
    [TestFixture(Category = "Mnemonics")]
    public class MnemonicTests
    {
        [Test]
        public void CreateBip39()
        {
            // Builds word list and compares internal hash
            new Bip39();
        }
    }
}