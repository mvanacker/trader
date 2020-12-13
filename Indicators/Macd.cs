using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Indicators
{
    public class Macd : IIndicator
    {
        public Ema FastEma { get; }
        public Ema SlowEma { get; }
        public int SignalLength { get; }
        public bool IsNormalized { get; }
        public List<double> Values { get; }
        public List<double> Signal { get; }
        public List<double> Momenta { get; }

        public Macd(Ema fastEma, Ema slowEma, int signalLength, bool isNormalized = Const.NORMALIZE_MACD)
        {
            FastEma = fastEma;
            SlowEma = slowEma;
            SignalLength = signalLength;
            IsNormalized = isNormalized;
            Values = new List<double>();
            Signal = new List<double>();
            Momenta = new List<double>();
        }

        // todo auto fill
        public void Update(Prices prices)
        {
            var closes = prices[Price.Close];
            if (closes.Count >= SlowEma.Period)
            {
                var fastEma = FastEma.Values.Last();
                var slowEma = SlowEma.Values.Last();
                var macd = fastEma - slowEma;
                if (IsNormalized) macd /= slowEma / 100;
                Values.Add(macd);
                if (closes.Count >= SlowEma.Period + SignalLength)
                {
                    UpdateSignal();
                    UpdateMomenta();
                }
            }
        }
        public void UpdateSignal()
        {
            int index = Values.Count - SignalLength;
            var subMacd = Values.GetRange(index, SignalLength);
            double signal = 0;
            foreach (var delta in subMacd)
            {
                signal += delta;
            }
            signal /= SignalLength;
            Signal.Add(signal);
        }
        public void UpdateMomenta()
        {
            var macd = Values.Last();
            var signal = Signal.Last();
            var momentum = macd - signal;
            Momenta.Add(momentum);
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
