
using System.Collections.Generic;

namespace IntersectSteam.Models.Transactions
{
    public class InitializeTransactionRequest
    {
        public string Key { get; set; }
        public ulong OrderId { get; set; }
        public ulong SteamId { get; set; }
        public uint AppId { get; set; }
        public uint ItemCount { get; set; }
        public string Language { get; set; }
        public string Currency { get; set; }
        public string UserSession { get; set; }
        public string IpAddress { get; set; }
        public uint BundleCount { get; set; }
        public TransactionItem[] Items { get; set; }
        public TransactionBundle[] Bundles { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>
            {
                { "key", Key },
                { "orderid", OrderId.ToString() },
                { "steamid", SteamId.ToString() },
                { "appid", AppId.ToString() },
                { "itemcount", ItemCount.ToString() },
                { "language", Language },
                { "currency", Currency },
                { "usersession", UserSession },
                { "ipaddress", IpAddress },
                { "bundlecount", BundleCount.ToString() }
            };

            if (Items != null)
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    dict.Add($"itemid[{i}]", Items[i].ItemId.ToString());
                    dict.Add($"qty[{i}]", Items[i].Qty.ToString());
                    dict.Add($"amount[{i}]", Items[i].Amount.ToString());
                    dict.Add($"description[{i}]", Items[i].Description);
                    dict.Add($"category[{i}]", Items[i].Category);
                    dict.Add($"associated_bundle[{i}]", Items[i].AssociatedBundle.ToString());
                    dict.Add($"billingtype[{i}]", Items[i].BillingType);
                    dict.Add($"startdate[{i}]", Items[i].StartDate);
                    dict.Add($"enddate[{i}]", Items[i].EndDate);
                    dict.Add($"period[{i}]", Items[i].Period);
                    dict.Add($"frequency[{i}]", Items[i].Frequency.ToString());
                    dict.Add($"recurringamt[{i}]", Items[i].RecurringAmt.ToString());
                }
            }

            if (Bundles != null)
            {
                for (int i = 0; i < Bundles.Length; i++)
                {
                    dict.Add($"bundleid[{i}]", Bundles[i].BundleId.ToString());
                    dict.Add($"bundle_qty[{i}]", Bundles[i].BundleQty.ToString());
                    dict.Add($"bundle_desc[{i}]", Bundles[i].BundleDesc);
                    dict.Add($"bundle_category[{i}]", Bundles[i].BundleCategory);
                }
            }

            return dict;
        }
    }
}
