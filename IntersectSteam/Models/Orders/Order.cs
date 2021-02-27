using IntersectSteam.Models.Users;
using System.Collections.Generic;

namespace IntersectSteam.Models.Orders
{
    public class Order
    {
        public ulong Id { get; set; }
        public UserData User { get; set; }
        public List<OrderItem> Items { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
    }
}
