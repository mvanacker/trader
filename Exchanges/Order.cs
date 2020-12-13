using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Trader
{
    public class Order
    {
        public Order()
        {
            OrderId = Guid.NewGuid();
        }
        public Order(DataRow row)
        {
            Side = ((string)row["side"]).ToEnum<Side>();
            OrdStatus = ((string)row["ordStatus"]).ToEnum<OrdStatus>();
            OrdType = ((string)row["ordType"]).ToEnum<OrdType>();
            Price = row["price"] as double?;
            StopPx = row["stoppx"] as double?;
            LeavesQty = (long)row["leavesQty"];
            CumQty = (long)row["cumQty"];
            OrderQty = row["orderQty"] as long?;
            Timestamp = (DateTime)row["timestamp"];
            OrderId = (Guid)row["orderID"];
            ExecInst = new HashSet<ExecInst>(((string)row["execInst"]).Split(',')
                .Select(inst => inst.ToEnum<ExecInst>()));
        }

        public Side Side { get; set; }
        public OrdStatus OrdStatus { get; set; }
        public OrdType OrdType { get; set; }
        public double? Price { get; set; }
        public double? StopPx { get; set; }
        public long LeavesQty { get; set; }
        public long CumQty { get; set; }
        public long? OrderQty { get; set; }
        public Guid OrderId { get; set; }
        public DateTime Timestamp { get; set; }
        public HashSet<ExecInst> ExecInst { get; set; } = new HashSet<ExecInst>();

        public Error Error { get; set; }

        public bool IsActive => OrdStatus == OrdStatus.New || OrdStatus == OrdStatus.PartiallyFilled;

        // misc props
        public string ClOrdId { get; set; }
        public string ClOrdLinkId { get; set; }
        public long? Account { get; set; }
        public Symbol? Symbol { get; set; }
        public float SimpleOrderQty { get; set; }
        public float DisplayQty { get; set; }
        public float PegOffsetValue { get; set; }
        public string PegPriceType { get; set; }
        public string Currency { get; set; }
        public string SettlCurrency { get; set; }
        public string TimeInForce { get; set; }
        public string ContingencyType { get; set; }
        public string ExDestination { get; set; }
        public string Triggered { get; set; }
        public bool? WorkingIndicator { get; set; }
        public string OrdRejReason { get; set; }
        public double? SimpleLeavesQty { get; set; }
        public long? SimpleCumQty { get; set; }
        public float AvgPx { get; set; }
        public string MultiLegReportingType { get; set; }
        public string Text { get; set; }
        public DateTime? TransactTime { get; set; }
    }
}
