using System.Collections.Generic;

namespace IntersectSteam.Models.Transactions
{
    public class FinalizeTransactionRequest
    {
        public string Key { get; set; }
        public ulong OrderId { get; set; }
        public uint AppId { get; set; }

        public FinalizeTransactionRequest(string key, ulong orderId, uint appId)
        {
            Key = key;
            OrderId = orderId;
            AppId = appId;
        }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>
            {
                { "key", Key },
                { "orderid", OrderId.ToString() },
                { "appid", AppId.ToString() }
            };
            return dict;
        }
    }
}
