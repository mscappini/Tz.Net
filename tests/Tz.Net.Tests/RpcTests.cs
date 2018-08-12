using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Numerics;
using System.Threading.Tasks;

namespace Tz.Net.Tests
{
    [TestFixture(Category = "RPC")]
    public class RpcTests
    {
        private Rpc rpc;

        [SetUp]
        public void SetUp()
        {
            rpc = new Rpc();
        }

        [Test]
        public async Task Describe()
        {
            JObject description = await rpc.Describe();
        }

        [Test]
        public async Task GetHead()
        {
            JObject head = await rpc.GetHead();
        }

        [Test]
        public async Task GetBalance()
        {
            BigFloat balance = await rpc.GetBalance("tz1hmK2ru6ism15MxXbnhKWWKGJ6hqWssMc5");
        }

        [Test]
        public async Task GetNetworkStat()
        {
            JObject stats = await rpc.GetNetworkStat();
        }

        [Test]
        [Ignore("This test only works once because a wallet can only be activated once. Use with care.")]
        public async Task Activate()
        {
            const string Address = "tz1MUDnYayTLJJwioLUEJVJj5wWVUrrSDpik";
            const string Secret = "ab03ec1f9e577ee9b1751c7223674203cf786f92";

            ActivateAccountOperationResult result = await new Rpc().Activate(Address, Secret);
        }

        [Test]
        [Ignore("This test actually transfer funds between two alphanet wallets. Use with care.")]
        public async Task Transfer()
        {
            const string Mnemonic = "feed ring nasty mean soon left mad certain rebel empty permit like session clutch robust";
            const string Email = "watylroc.pcatxegk@tezos.example.org";
            const string Password = "UBoCoEMCuS";

            const string From = "tz1hmK2ru6ism15MxXbnhKWWKGJ6hqWssMc5";
            const string To = "tz1VVbbeYzzY6aW9TvE39KmLRhGpLtfkMJ4k";

            Wallet wallet = new Wallet(Mnemonic, Email, Password);

            SendTransactionOperationResult result = await rpc.SendTransaction(wallet.Keys, From, To, 1.0123456789, 0);
        }
    }
}