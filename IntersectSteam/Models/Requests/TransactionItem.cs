
namespace IntersectSteam.Models.Transactions
{
    public class TransactionItem
    {
        public uint ItemId { get; set; }
        public short Qty { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public uint AssociatedBundle { get; set; }
        public string BillingType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Period { get; set; }
        public uint Frequency { get; set; }
        public long RecurringAmt { get; set; }
    }
}
