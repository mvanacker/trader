using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Trader.Exchanges
{
    public class Margin
    {
        public Margin()
        {

        }
        public Margin(DataRow row)
        {
            WalletBalance = (long)row["walletBalance"];
            MarginUsedPcnt = (double)row["marginUsedPcnt"];
        }

        public long WalletBalance { get; set; }
        public double MarginUsedPcnt { get; set; }
    }
}
