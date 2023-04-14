using IntersectSteam.Models.Orders;

namespace IntersectSteam.Models.Transactions
{
    public class TransactionRequest
    {
        public ulong SteamId { get; set; }
        public string Language { get; set; }
        public Order Order { get; set; }
        public string Currency { get; set; }

        public TransactionRequest() { }

        public TransactionRequest(ulong steamId, string language, Order order, string currency)
        {
            SteamId = steamId;
            Language = language;
            Order = order;
            Currency = currency;
        }
    }
}
