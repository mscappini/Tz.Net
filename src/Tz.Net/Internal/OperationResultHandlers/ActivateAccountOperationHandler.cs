using Newtonsoft.Json.Linq;
using System.Numerics;

namespace Tz.Net.Internal.OperationResultHandlers
{
    internal class ActivateAccountOperationHandler : IOperationHandler
    {
        public string HandlesOperation => Operations.ActivateAccount;

        public OperationResult ParseApplyOperationsResult(JToken appliedOp)
        {
            ActivateAccountOperationResult result = new ActivateAccountOperationResult(appliedOp);

            JToken opResult = appliedOp["metadata"]?["balance_updates"];
            string change = opResult?.First["change"]?.ToString();
            if (change != null)
            {
                result.Change = new BigFloat(change);
                result.Succeeded = true;
            }

            return result;
        }
    }
}