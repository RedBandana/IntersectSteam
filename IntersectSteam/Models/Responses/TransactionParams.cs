
namespace IntersectSteam.Models.Api
{
    public class TransactionParams : IParams
    {
        public ulong OrderId { get; set; }
        public ulong TransId { get; set; }
    }

}
