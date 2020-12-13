using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Indicators
{
    public class Kc : IIndicator
    {
        public Kc(int width, int period)
        {
            Width = width;
            Period = period;
        }

        public int Width { get; }
        public int Period { get; }
        public List<double> UpperBand { get; } = new List<double>();
        public List<double> LowerBand { get; } = new List<double>();

        public void Update(Prices prices)
        {
            var closes = prices[Price.Close];
            if (closes.Count >= Period + 1)
            {
                int start = closes.Count - Period - 1;
                var lastLows = prices[Price.Low].GetRange(start, Period + 1);
                var lastHighs = prices[Price.High].GetRange(start, Period + 1);
                var lastCloses = closes.GetRange(start, Period + 1);
                var avg = lastCloses.Skip(1).Average();
                var ranges = new List<double>();
                for (int i = 0; i < lastCloses.Count; i++)
                {
                    var lowRange = Math.Abs(lastCloses[i] - lastLows[i]);
                    var highRange = Math.Abs(lastCloses[i] - lastHighs[i]);
                    var range = Math.Max(lowRange, highRange);
                    ranges.Add(range);
                }
                var atr = ranges.Average();
                UpperBand.Add(avg + Width * atr);
                LowerBand.Add(avg - Width * atr);
            }
        }
    }
}
