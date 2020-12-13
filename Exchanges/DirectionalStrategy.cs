using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Exchanges
{
    using Logger;
    using Technician;

    public class DirectionalStrategy : Strategy
    {
        private double StochTreshold { get; } = 0.2;
        private double StopLoss { get; } = 0.000625;//0.01;
        private double TakeProfit { get; } = 0.01;//0.05;
        // logging label
        private const string _name = "strategy";

        public DirectionalStrategy(Exchange exchange, Symbol symbol = Symbol.XBTUSD, TradeBin tradeBin = TradeBin.FiveMinute, int fastEmaLength = Const.DIRECTIONAL_FAST_EMA_LENGTH, int slowEmaLength = Const.DIRECTIONAL_SLOW_EMA_LENGTH, int stochLength = Const.DIRECTIONAL_STOCH_LENGTH) : base(exchange, symbol, tradeBin)
        {
            //
            var prices = Exchange.Prices[Symbol][TradeBin];
            Macd = new MovingMacd(fastEmaLength, slowEmaLength, prices[Price.Close]);
            prices.Updated += (s, e) => Macd.Update();
            //
            var volatilities = Exchange.IndexPrices[Index.BVOL24H];
            Stoch = new MovingStochastic(stochLength, volatilities[Price.Close]);
            volatilities.Updated += (s, e) => Stoch.Update();
            //
            Measurer = new IndexStochasticMeasurer
            {
                Stoch = Stoch,
                StochTreshold = StochTreshold
            };
            volatilities.Updated += (s, e) => Measurer.Measure();
            //
            Timeout = Const.DIRECTIONAL_STRATEGY_TIMEOUT;
        }

        private MovingStochastic Stoch { get; }
        private MovingMacd Macd { get; }
        private List<double> Volatility => Exchange.IndexPrices[Index.BVOL24H][Price.Close];
        private VolatilityMeasurer Measurer { get; }

        public override void Execute()
        {
            var macd = Macd.Get();
            if (State.Position.IsFlat)
            {
                Exchange.CancelAll();
                if (!Measurer.IsPriceVolatile)
                {
                    // open position in direction of trend
                    var side = macd < 0 ? Side.Sell : Side.Buy;
                    EnterPosition(side);
                }
            }
            else
            {
                //
            }
        }
        private void EnterPosition(Side side)
        {
            var price = State.OrderBooks[side.Opposite()].Keys.First();
            var quantity = (long)(price * State.Budget);
            var profit = (price * (1 - (int)side * TakeProfit)).RoundToPip(Pip);
            var stop = (price * (1 + (int)side * StopLoss)).RoundToPip(Pip);
            Exchange.PostMarketOrder(side, Symbol, quantity);
            Exchange.PostOrder(side.Opposite(), Symbol, profit, quantity);
            Exchange.PostStop(side.Opposite(), Symbol, stop);
        }
    }
}
