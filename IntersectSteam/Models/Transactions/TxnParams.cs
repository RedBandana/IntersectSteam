
namespace IntersectSteam.Models.Transactions
{
    public class TxnParams
    {
        public ulong Orderid { get; set; }
        public ulong Transid { get; set; }
        public string SteamUrl { get; set; }
        public string[] Agreements { get; set; }
    }
}
