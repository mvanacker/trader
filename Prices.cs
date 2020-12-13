using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader
{
    using Indicators;

    public enum Price
    {
        High, Low, Open, Close
    }

    public class PriceEventArgs : EventArgs
    {
        public PriceEventArgs(double[] hloc)
        {
            Hloc = hloc;
        }

        public double[] Hloc { get; set; }
    }

    public class Prices
    {
        public Prices()
        {
            Type = Price.Close;
            Map = new Dictionary<Price, List<double>>();
            foreach (Price type in Enum.GetValues(typeof(Price)))
            {
                Map[type] = new List<double>();
            }
            FastEma = new Ema(Const.DIRECTIONAL_FAST_EMA_LENGTH);
            SlowEma = new Ema(Const.DIRECTIONAL_SLOW_EMA_LENGTH);
            Bb = new Bb(Const.BB_WIDTH, Const.BB_PERIOD);
            //Kc = new Kc(Const.BB_WIDTH, Const.BB_PERIOD);
            //Mc = new Mc(Const.BB_PERIOD);
            Macd = new Macd(FastEma, SlowEma, Const.MACD_SIGNAL_LENGTH);
            Rsi = new Rsi(Const.RSI_LENGTH);
            StochRsi = new StochRsi(Rsi, Const.STOCH_RSI_LENGTH, Const.STOCH_RSI_D_LENGTH);
            Indicators = new List<IIndicator>
            {
                FastEma, SlowEma, Bb, /*Kc,*/ /*Mc,*/ Macd, Rsi, StochRsi
            };
        }

        public List<double> this[Price type] => Map[type];

        public Price Type { get; set; }
        public Dictionary<Price, List<double>> Map { get; }
        public Ema FastEma { get; }
        public Ema SlowEma { get; }
        public Bb Bb { get; }
        //public Kc Kc { get; }
        //public Mc Mc { get; }
        public Macd Macd { get; }
        public Rsi Rsi { get; }
        public StochRsi StochRsi { get; }
        public List<IIndicator> Indicators { get; }

        public event EventHandler<PriceEventArgs> Updated;

        public void AddHloc(double[] hloc)
        {
            Map[Price.High].Add(hloc[0]);
            Map[Price.Low].Add(hloc[1]);
            Map[Price.Open].Add(hloc[2]);
            Map[Price.Close].Add(hloc[3]);
            foreach (var indicator in Indicators)
            {
                indicator.Update(this);
            }
            Updated?.Invoke(this, new PriceEventArgs(hloc));
        }
        public void AddHloc(object[] hloc)
        {
            var fhloc = new double[hloc.Length];
            for (int i = 0; i < fhloc.Length; i++)
            {
                fhloc[i] = (double)hloc[i];
            }
            AddHloc(fhloc);
        }
    }
}
