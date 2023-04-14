using System.Collections.Generic;

namespace IntersectSteam.Models.Orders
{
    public class Order
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public List<OrderItem> Items { get; set; }

        public Order() { }

        public Order(ulong id, string name, List<OrderItem> items)
        {
            Id = id;
            Name = name;
            Items = items;
        }
    }
}

