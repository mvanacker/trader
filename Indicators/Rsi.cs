using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Indicators
{
    public class Rsi : IIndicator
    {
        public int Period { get; }
        public List<double> Values { get; }
        public List<double> AverageGains { get; }
        public List<double> AverageLosses { get; }

        public Rsi(int period)
        {
            Period = period;
            Values = new List<double>();
            AverageGains = new List<double>();
            AverageLosses = new List<double>();
        }

        public void Update(Prices prices)
        {
            var closes = prices[Price.Close];
            if (closes.Count >= Period + 1)
            {
                if (AverageGains.Count == 0) AverageGains.Add(GetAverageGains(closes));
                if (AverageLosses.Count == 0) AverageLosses.Add(GetAverageLosses(closes));
                int n = closes.Count - Period + 1;
                int i = closes.Count + Values.Count - 1 - n;
                while (Values.Count < n)
                {
                    double currentGain = 0, currentLoss = 0;
                    if (closes[i] > closes[i - 1])
                    {
                        currentGain = closes[i] - closes[i - 1];
                    }
                    else
                    {
                        currentLoss = closes[i - 1] - closes[i];
                    }
                    var lastAvgGain = AverageGains.Last();
                    var averageGain = (lastAvgGain * (Period - 1) + currentGain) / Period;
                    AverageGains.Add(averageGain);
                    var lastAvgLoss = AverageLosses.Last();
                    var averageLoss = (lastAvgLoss * (Period - 1) + currentLoss) / Period;
                    AverageLosses.Add(averageLoss);
                    var avgGain = AverageGains.Last();
                    var avgLoss = AverageLosses.Last();
                    var rsi = 1 - 1 / (1 + avgGain / avgLoss);
                    Values.Add(rsi);
                }
            }
        }
        private double GetAverageGains(List<double> prices)
        {
            double sum = 0;
            for (int i = prices.Count - 1; i >= prices.Count - Period; i--)
            {
                if (prices[i] > prices[i - 1])
                {
                    sum += prices[i] - prices[i - 1];
                }
            }
            return sum / Period;
        }
        private double GetAverageLosses(List<double> prices)
        {
            double sum = 0;
            for (int i = prices.Count - 1; i >= prices.Count - Period; i--)
            {
                if (prices[i] < prices[i - 1])
                {
                    sum += prices[i - 1] - prices[i];
                }
            }
            return sum / Period;
        }
    }
}
