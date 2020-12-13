using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Indicators
{
    public class Mc : IIndicator
    {
        // measured 5m change 2017-01-01 to 2018-06-01
        private const double MEAN_CHANGE = 0.00210031;

        public Mc(int period)
        {
            Period = period;
        }
        
        public int Period { get; }
        public List<double> UpperBand { get; } = new List<double>();
        public List<double> LowerBand { get; } = new List<double>();

        public void Update(Prices prices)
        {
            var closes = prices[Price.Close];
            if (closes.Count >= Period)
            {
                int start = closes.Count - Period;
                var lastCloses = closes.GetRange(start, Period);
                var avg = lastCloses.Average();
                var meanChange = closes.Last() * MEAN_CHANGE;
                UpperBand.Add(avg + meanChange);
                LowerBand.Add(avg - meanChange);
            }
        }
    }
}
