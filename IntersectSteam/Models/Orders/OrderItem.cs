using System;

namespace IntersectSteam.Models.Orders
{
    public class OrderItem
    {
        public Guid ItemId { get; set; }
        public uint Id { get; set; }
        public uint Quantity { get; set; }
        public string Description { get; set; }
        public ulong Amount { get; set; }

        public OrderItem() { }

        public OrderItem(Guid itemId, uint id, uint quantity, ulong amount, string description)
        {
            ItemId = itemId;
            Id = id;
            Quantity = quantity;
            Amount = amount;
            Description = description;
        }
    }
}
