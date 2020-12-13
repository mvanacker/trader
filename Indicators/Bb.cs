using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Indicators
{
    public class Bb : IIndicator
    {
        public Bb(double width, int period)
        {
            Width = width;
            Period = period;
        }

        public double Width { get; }
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
                var variance = lastCloses.Sum(p => p * p) / Period - avg * avg;
                var stdev = Math.Sqrt(variance);
                UpperBand.Add(avg + Width * stdev);
                LowerBand.Add(avg - Width * stdev);
            }
        }
    }
}
