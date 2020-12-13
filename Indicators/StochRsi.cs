using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Indicators
{
    public class StochRsi : IIndicator
    {
        public Rsi Rsi { get; }
        public int StochPeriod { get; }
        public List<double> K { get; }
        public int DPeriod { get; }
        public List<double> D { get; }
        public List<double> Momenta { get; }

        public StochRsi(Rsi rsi, int stochPeriod, int dPeriod)
        {
            Rsi = rsi;
            StochPeriod = stochPeriod;
            K = new List<double>();
            DPeriod = dPeriod;
            D = new List<double>();
            Momenta = new List<double>();
        }

        public void Update(Prices prices)
        {
            if (Rsi.Values.Count > StochPeriod)
            {
                int start = Rsi.Values.Count - StochPeriod;
                var subRsi = Rsi.Values.GetRange(start, StochPeriod);
                var minRsi = subRsi.Min();
                var maxRsi = subRsi.Max();
                var rsi = Rsi.Values.Last();
                var k = (rsi - minRsi) / (maxRsi - minRsi);
                K.Add(k);
                if (K.Count >= DPeriod)
                {
                    UpdateD();
                    UpdateMomenta();
                }
            }
        }
        public void UpdateD()
        {
            double d = 0;
            for (int i = K.Count - 1; i >= K.Count - DPeriod; i--)
            {
                d += K[i];
            }
            d /= DPeriod;
            D.Add(d);
        }
        public void UpdateMomenta()
        {
            var k = K.Last();
            var d = D.Last();
            var momentum = k - d;
            Momenta.Add(momentum);
        }
        public bool IsKIncreasing()
        {
            if (K.Count < 2) return false;
            var last = K.Last();
            var penLast = K[K.Count - 2];
            return last > penLast;
        }
        public bool IsMomentumIncreasing()
        {
            if (Momenta.Count < 2) return false;
            var last = Momenta.Last();
            var penLast = Momenta[Momenta.Count - 2];
            return last > penLast;
        }
    }
}
