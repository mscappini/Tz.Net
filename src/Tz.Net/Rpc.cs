using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Threading.Tasks;
using Tz.Net.Extensions;
using Tz.Net.Internal;
using Tz.Net.Internal.OperationResultHandlers;
using Tz.Net.Security;

namespace Tz.Net
{
    // http://doc.tzalpha.net/api/rpc_proposal.html#rpc-changes-june-2018

    public enum Chain
    {
        Main = 0,
        Test = 1
        // Are there more?
    }

    public class Rpc
    {
        public const string DefaultProvider = "http://localhost:8732";

        private static readonly HttpClient _client = new HttpClient();
        private static Dictionary<string, IOperationHandler> _opHandlers = new Dictionary<string, IOperationHandler>
        {
            {  Operations.ActivateAccount, new ActivateAccountOperationHandler() },
            {  Operations.Transaction, new TransactionOperationHandler() }
        };

        private readonly string _provider;
        private readonly string _chain;

        public Rpc()
            : this(DefaultProvider)
        { }

        public Rpc(string provider)
            : this(provider, Chain.Main)
        { }

        public Rpc(string provider, Chain chain)
            : this(provider, chain.ToString().ToLower())
        { }

        public Rpc(string provider, string chain)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentException("Provider required", nameof(provider));
            }
            else if (string.IsNullOrWhiteSpace(chain))
            {
                throw new ArgumentException("Chain required", nameof(chain));
            }
            _provider = provider;
            _chain = chain;
        }

        public async Task<JObject> Describe()
        {
            // There is curently a weird situation in alpha where the RPC will not honor any request without a recurse=true arg. // 8 Aug 2018
            return await QueryJ<JObject>("describe?recurse=true");
        }

        public async Task<JObject> GetHead()
        {
            return await QueryJ<JObject>($"chains/{_chain}/blocks/head");
        }

        public async Task<JObject> GetHeader()
        {
            return await QueryJ<JObject>($"chains/{_chain}/blocks/head/header");
        }

        public async Task<JObject> GetAccountForBlock(string blockHash, string address)
        {
            return await QueryJ<JObject>($"chains/{_chain}/blocks/{blockHash}/context/contracts/{address}");
        }

        public async Task<BigFloat> GetBalance(string address)
        {
            JToken response = await QueryJ($"chains/{_chain}/blocks/head/context/contracts/{address}/balance");

            return new BigFloat(response.ToString());
        }

        public async Task<JObject> GetNetworkStat()
        {
            return await QueryJ<JObject>("network/stat");
        }

        public async Task<int> GetCounter(string address)
        {
            JToken counter = await QueryJ($"chains/{_chain}/blocks/head/context/contracts/{address}/counter");
            return Convert.ToInt32(counter.ToString());
        }

        public async Task<JToken> GetManagerKey(string address)
        {
            return await QueryJ($"chains/{_chain}/blocks/head/context/contracts/{address}/manager_key");
        }

        public async Task<ActivateAccountOperationResult> Activate(string address, string secret)
        {
            JObject activateOp = new JObject();

            activateOp["kind"] = Operations.ActivateAccount;
            activateOp["pkh"] = address;
            activateOp["secret"] = secret;

            List<OperationResult> sendResults = await SendOperations(activateOp, null);

            return sendResults.LastOrDefault() as ActivateAccountOperationResult;
        }

        public async Task<SendTransactionOperationResult> SendTransaction(Keys keys, string from, string to, BigFloat amount, BigFloat fee, BigFloat gasLimit = null, BigFloat storageLimit = null)
        {
            gasLimit = gasLimit ?? 200;
            storageLimit = storageLimit ?? 0;

            JObject head = await GetHeader();
            JObject account = await GetAccountForBlock(head["hash"].ToString(), from);

            int counter = int.Parse(account["counter"].ToString());

            JArray operations = new JArray();

            JToken managerKey = await GetManagerKey(from);

            string gas = gasLimit.ToString();
            string storage = storageLimit.ToString();

            if (keys != null && managerKey["key"] == null)
            {
                JObject revealOp = new JObject();
                operations.AddFirst(revealOp);

                revealOp["kind"] = "reveal";
                revealOp["fee"] = "0";
                revealOp["public_key"] = keys.DecryptPublicKey();
                revealOp["source"] = from;
                revealOp["storage_limit"] = storage;
                revealOp["gas_limit"] = gas;
                revealOp["counter"] = (++counter).ToString();
            }

            JObject transaction = new JObject();
            operations.Add(transaction);

            transaction["destination"] = to;
            transaction["amount"] = new BigFloat(amount.ToMicroTez().ToString(6)).Round().ToString(); // Convert to microtez, truncate at 6 digits, round up
            transaction["storage_limit"] = storage;
            transaction["gas_limit"] = gas;
            transaction["counter"] = (++counter).ToString();
            transaction["fee"] = fee.ToString();
            transaction["source"] = from;
            transaction["kind"] = Operations.Transaction;

            JObject parameters = new JObject();
            transaction["parameters"] = parameters;
            parameters["prim"] = "Unit";
            parameters["args"] = new JArray(); // No args for this contract.

            List<OperationResult> sendResults = await SendOperations(operations, keys, head);

            return sendResults.LastOrDefault() as SendTransactionOperationResult;
        }

        private async Task<List<OperationResult>> SendOperations(JToken operations, Keys keys, JObject head = null)
        {
            JObject result = new JObject();

            if (head == null)
            {
                head = await GetHeader();
            }

            JArray arrOps = operations as JArray;
            if (arrOps == null)
            {
                arrOps = new JArray(operations);
            }

            JToken forgedOpGroup = await ForgeOperations(head, arrOps);

            SignedMessage signedOpGroup;

            if (keys == null)
            {
                signedOpGroup = new SignedMessage
                {
                    SignedBytes = forgedOpGroup.ToString() + "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000",
                    EncodedSignature = "edsigtXomBKi5CTRf5cjATJWSyaRvhfYNHqSUGrn4SdbYRcGwQrUGjzEfQDTuqHhuA8b2d8NarZjz8TRf65WkpQmo423BtomS8Q"
                };
            }
            else
            {
                Crypto c = new Crypto();
                signedOpGroup = c.Sign(forgedOpGroup.ToString(), keys, Watermark.Generic);
            }

            List<OperationResult> opResults = await PreApplyOperations(head, arrOps, signedOpGroup.EncodedSignature);

            if (opResults.All(op => op.Succeeded))
            {
                JToken injectedOperation = await InjectOperations(signedOpGroup.SignedBytes);
            }

            return opResults;
        }

        private async Task<JToken> ForgeOperations(JObject blockHead, JArray operations)
        {
            JObject contents = new JObject();

            contents["branch"] = blockHead["hash"];
            contents["contents"] = operations;

            return await QueryJ($"chains/{_chain}/blocks/head/helpers/forge/operations", contents);
        }

        private async Task<List<OperationResult>> PreApplyOperations(JObject head, JArray operations, string signature)
        {
            JArray payload = new JArray();
            JObject jsonObject = new JObject();
            payload.Add(jsonObject);

            jsonObject["protocol"] = head["protocol"];
            jsonObject["branch"] = head["hash"];
            jsonObject["contents"] = operations;
            jsonObject["signature"] = signature;

            JArray result = await QueryJ<JArray>($"chains/{_chain}/blocks/head/helpers/preapply/operations", payload);

            return ParseApplyOperationsResult(result);
        }

        private async Task<JToken> InjectOperations(string signedBytes)
        {
            return await QueryJ<JValue>($"injection/operation?chain={_chain}", new JRaw($"\"{signedBytes}\""));
        }

        private List<OperationResult> ParseApplyOperationsResult(JArray appliedOps)
        {
            List<OperationResult> operationResults = new List<OperationResult>();

            if (appliedOps?.Count > 0)
            {
                JArray contents = appliedOps.First["contents"] as JArray;

                foreach (JToken content in contents)
                {
                    string kind = content["kind"].ToString();

                    if (!string.IsNullOrWhiteSpace(kind))
                    {
                        IOperationHandler handler = _opHandlers[kind];

                        if (handler != null)
                        {
                            OperationResult opResult = handler.ParseApplyOperationsResult(content);

                            if (opResult != null)
                            {
                                operationResults.Add(opResult);
                            }
                        }
                    }
                }
            }

            return operationResults;
        }

        private async Task<JToken> QueryJ(string ep, JToken data = null)
        {
            return await QueryJ<JToken>(ep, data);
        }

        private async Task<JType> QueryJ<JType>(string ep, JToken data = null)
            where JType : JToken
        {
            return (JType)JToken.Parse(await Query(ep, data?.ToString(Formatting.None)));
        }

        private async Task<string> Query(string ep, object data = null)
        {
            bool get = data == null;

            HttpRequestMessage request = new HttpRequestMessage(get ? HttpMethod.Get : HttpMethod.Post, $"{_provider}/{ep}")
            {
                Version = HttpVersion.Version11 // Tezos node does not like the default v2.
            };

            if (!get)
            {
                request.Content = new StringContent(data.ToString());
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            HttpResponseMessage response = await _client.SendAsync(request);

            string responseBody = await response.Content.ReadAsStringAsync();

            if (response?.IsSuccessStatusCode == false)
            {
                // If failed, throw the body as the exception message.
                if (!string.IsNullOrWhiteSpace(responseBody))
                {
                    throw new HttpRequestException(responseBody);
                }
                else
                {
                    // Otherwise, throw a generic exception.
                    response.EnsureSuccessStatusCode();
                }
            }

            return responseBody;
        }
    }
}