using Newtonsoft.Json.Linq;

namespace Tz.Net.Internal.OperationResultHandlers
{
    internal interface IOperationHandler
    {
        string HandlesOperation { get; }
        OperationResult ParseApplyOperationsResult(JToken appliedOp);
    }
}