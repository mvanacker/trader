using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Indicators
{
    public class Ema : IIndicator
    {
        public int Period { get; }
        public List<double> Values { get; }

        public Ema(int period)
        {
            Period = period;
            Values = new List<double>();
        }

        public void Update(Prices prices)
        {
            var closes = prices[Price.Close];
            if (closes.Count >= Period)
            {
                if (Values.Count == 0)
                {
                    // add a simple moving average as first element
                    Values.Add(closes.GetRange(0, Period).Average());
                }
                int n = closes.Count - Period + 1;
                //int i = closes.Count + Values.Count - 1 - n;
                int i = Values.Count + Period - 2;
                double multiplier = 2.0 / (Period + 1);
                while (Values.Count < n)
                {
                    double lastEma = Values.Last();
                    double ema = multiplier * (closes[i++] - lastEma) + lastEma;
                    Values.Add(ema);
                }
            }
        }
    }
}
