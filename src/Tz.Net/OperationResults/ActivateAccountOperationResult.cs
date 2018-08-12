using Newtonsoft.Json.Linq;
using System.Numerics;

namespace Tz.Net
{
    public class ActivateAccountOperationResult : OperationResult
    {
        public ActivateAccountOperationResult()
        { }

        public ActivateAccountOperationResult(JToken data)
            : base(data)
        { }

        public BigFloat Change { get; internal set; }
    }
}