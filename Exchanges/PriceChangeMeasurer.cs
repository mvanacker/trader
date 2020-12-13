using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Exchanges
{
    public class PriceChangeMeasurer : VolatilityMeasurer
    {
        public double TotalChange { get; set; }
        public double SpikeChange { get; set; }
        public List<double> Prices { get; set; }

        public override void Measure()
        {
            int start = Prices.Count - Const.VOLATILITY_CANDLE_RANGE;
            if (start > 0 && Prices.Count >= Const.VOLATILITY_MIN_PRICE_COUNT)
            {
                var lastPrices = Prices.GetRange(start, Const.VOLATILITY_CANDLE_RANGE);
                var first = lastPrices.First();
                var last = lastPrices.Last();
                TotalChange = last / first - 1;
                var lowest = lastPrices.Min();
                var lowestIndex = lastPrices.IndexOf(lowest);
                var highest = lastPrices.Max();
                var highestIndex = lastPrices.IndexOf(highest);
                if (lowestIndex < highestIndex)
                {
                    SpikeChange = highest / lowest - 1;
                }
                else
                {
                    SpikeChange = lowest / highest - 1;
                }
                IsPriceVolatile = Math.Abs(SpikeChange) >= Const.STOP_LOSS_PERCENTAGE;
                if (IsPriceVolatile) Console.WriteLine($"# Panicking");
                IsMeasuring = true;
            }
        }
    }
}
