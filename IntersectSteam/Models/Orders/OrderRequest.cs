
namespace IntersectSteam.Models.Orders
{
    public class OrderRequest
    {
        public ulong SteamId { get; set; }
        public string Language { get; set; }
        public string Currency { get; set; }
        public Order Order { get; set; }

        public OrderRequest() { }

        public OrderRequest(ulong steamId, string language, string currency, Order order)
        {
            SteamId = steamId;
            Language = language;
            Currency = currency;
            Order = order;
        }
    }
}
