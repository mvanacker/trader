using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Trader
{
    public class Execution
    {
        public Execution(DataRow row)
        {
            var side = (string)row["side"];
            if (!string.IsNullOrEmpty(side)) Side = side.ToEnum<Side>();
            ExecType = ((string)row["execType"]).ToEnum<ExecType>();
            OrdStatus = ((string)row["ordStatus"]).ToEnum<OrdStatus>();
            OrdType = ((string)row["ordType"]).ToEnum<OrdType>();
            Price = row["price"] as double?;
            StopPx = row["stopPx"] as double?;
            LeavesQty = (long)row["leavesQty"];
            CumQty = (long)row["cumQty"];
            OrderQty = row["orderQty"] as long?;
            LastQty = row["lastQty"] as long?;
            ExecId = (Guid)row["execID"];
            OrderId = (Guid)row["orderID"];
            Timestamp = (DateTime)row["timestamp"];
        }

        public Side? Side { get; set; }
        public ExecType ExecType { get; set; }
        public OrdStatus OrdStatus { get; set; }
        public OrdType OrdType { get; set; }
        public double? Price { get; set; }
        public double? StopPx { get; set; }
        public long LeavesQty { get; set; }
        public long CumQty { get; set; }
        public long? OrderQty { get; set; }
        public long? LastQty { get; set; }
        public Guid ExecId { get; set; }
        public Guid OrderId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
