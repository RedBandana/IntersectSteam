using IntersectSteam.Models.Errors;

namespace IntersectSteam.Models.Transactions
{
    public class TxnResponseField
    {
        public string Result { get; set; }
        public TxnParams Params { get; set; }
        public ApiError Error { get; set; }
    }
}
