using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Exchanges
{
    using Technician;

    public class ChannelStrategy : Strategy
    {
        public ChannelStrategy(Exchange exchange, Symbol symbol = Symbol.XBTUSD, TradeBin tradeBin = TradeBin.FiveMinute) : base(exchange, symbol, tradeBin)
        {
            //
            Measurer = new PriceChangeMeasurer { Prices = Prices };
            Exchange.Prices[Symbol][TradeBin].Updated += (s,e)=> Measurer.Measure();
            //
            Band = new MovingDeviation(Const.BB_PERIOD, Prices);
            Exchange.Prices[Symbol][TradeBin].Updated += (s, e) => Band.Update();
            //
            Timeout = Const.CHANNEL_STRATEGY_TIMEOUT;
        }

        protected override bool Ready => base.Ready && Measurer.IsMeasuring;
        public double StepHeight { get; set; }
        private MovingDeviation Band { get; }
        private VolatilityMeasurer Measurer { get; }

        private Dictionary<Side, Guid> StopIds { get; } = new Dictionary<Side, Guid>
        {
            [Side.Sell] = Guid.Empty,
            [Side.Buy] = Guid.Empty
        };
        private Dictionary<Side, Guid> OrderIds { get; } = new Dictionary<Side, Guid>
        {
            [Side.Sell] = Guid.Empty,
            [Side.Buy] = Guid.Empty
        };
        private Dictionary<Side, DateTime> LastSizeChanges { get; } = new Dictionary<Side, DateTime>
        {
            [Side.Sell] = Utils.Epoch,
            [Side.Buy] = Utils.Epoch
        };

        public override void Execute()
        {
            if (Measurer.IsPriceVolatile) Panic();
            else
            {
                if (OrderIds[Side.Sell] == Guid.Empty || OrderIds[Side.Buy] == Guid.Empty)
                {
                    Exchange.CancelAll();
                }
                ManageOrder(Side.Sell);
                ManageOrder(Side.Buy);
                if (State.Position.IsShort) ManageStop(Side.Buy);
                else if (State.Position.IsLong) ManageStop(Side.Sell);
            }
        }
        private void Panic()
        {
            var orders = State.Orders.Values.Where(o => o.OrdType != OrdType.Stop);
            if (orders.Count() > 0)
            {
                Exchange.CancelAll(orders);
                Log("Panic pulled orders");
            }
        }
        private void ManageOrder(Side side)
        {
            // price
            var first = State.OrderBooks[side].First().Key;
            var price = Band.Get(first, (int)side * Const.BB_WIDTH).RoundToPip(Pip);
            var shouldUndercut = side == Side.Sell ? price <= first : price >= first;
            if (shouldUndercut) price = CalculateUndercutPrice(side);
            // quantity
            var step = State.Budget * StepHeight;
            var quantity = (long)(price * step);
            if ((side == Side.Sell && State.Position.IsLong)
                || (side == Side.Buy && State.Position.IsShort))
            {
                quantity += (int)side * State.Position.CurrentQty;
            }
            // amend
            var sizeChangeCooldownUp = DateTime.UtcNow - LastSizeChanges[side] > Const.SIZE_CHANGE_COOLDOWN;
            var fillCooldownUp = DateTime.UtcNow - State.LastFills[side] > Const.SIZE_CHANGE_COOLDOWN;
            if (State.Orders.ContainsKey(OrderIds[side]) && State.Orders[OrderIds[side]].IsActive)
            {
                var order = State.Orders[OrderIds[side]];
                if (sizeChangeCooldownUp)
                {
                    Exchange.AmendOrder(side, price, quantity, order);
                    LastSizeChanges[side] = DateTime.UtcNow;
                }
                else
                {
                    Exchange.AmendOrder(side, price, order);
                }
            }
            // post
            else if (fillCooldownUp)
            {
                var order = Exchange.PostOrder(side, Symbol, price, quantity);
                if (order.OrdStatus == OrdStatus.New)
                {
                    OrderIds[side] = order.OrderId;
                    LastSizeChanges[side] = DateTime.UtcNow;
                }
            }
        }
        private void ManageStop(Side side)
        {
            int start = Prices.Count - Const.VOLATILITY_CANDLE_RANGE;
            if (start > 0 && Prices.Count >= Const.VOLATILITY_MIN_PRICE_COUNT)
            {
                var first = Prices.ElementAt(start);
                var stopLossPcnt = Const.STOP_LOSS_PERCENTAGE * (1 - State.Margin.MarginUsedPcnt);
                var price = (first * (1 - (int)side * stopLossPcnt)).RoundToPip(Pip);
                if (State.Orders.ContainsKey(StopIds[side]) && State.Orders[StopIds[side]].IsActive)
                {
                    Exchange.AmendStop(side, price, State.Orders[StopIds[side]]);
                }
                else
                {
                    StopIds[side] = Exchange.PostStop(side, Symbol, price).OrderId;
                }
                Exchange.CancelAll(State.Orders.Values.Where(o => o.OrdType == OrdType.Stop && o.OrderId != StopIds[side]));
            }
        }
    }
}
