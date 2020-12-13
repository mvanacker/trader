using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace Trader.Exchanges
{
    public enum UndercutTactic
    {
        UndercutSmallestWall, UndercutGreatestWall, UndercutAnyone
    }

    public abstract class Strategy : BackgroundLoop
    {
        public Strategy(Exchange exchange, Symbol symbol, TradeBin tradeBin)
        {
            Exchange = exchange;
            Symbol = symbol;
            TradeBin = tradeBin;
            Timeout = Const.DIRECTIONAL_STRATEGY_TIMEOUT;
            LeveragePosted = false;
        }

        public Exchange Exchange { get; }
        public Symbol Symbol { get; }
        public TradeBin TradeBin { get; }
        public State State { get; set; }
        public double Pip { get; set; }
        public UndercutTactic UndercutTactic { get; set; }
        public int MinimumWallHeight { get; set; }

        protected List<double> Prices => Exchange.Prices[Symbol][TradeBin][Price.Close];
        private bool LeveragePosted { get; set; }
        protected virtual bool Ready => State.TablesCreated;

        public override void CancelAsync()
        {
            base.CancelAsync();
            State.Reset();
        }
        protected void Log(string message)
        {
            var timestamp = DateTime.Now.ToString(Const.DATETIME_FORMAT);
            //var total = (TotalChange * 100).ToString(Const.PRICE_CHANGE_FORMAT);
            //var spike = (SpikeChange * 100).ToString(Const.PRICE_CHANGE_FORMAT);
            Console.WriteLine($"[{Exchange.Name}:{Symbol} {timestamp}] {message}");
            //Console.WriteLine($"[{Exchange.Name}:{Symbol} {total}|{spike} {timestamp}] {message}");
        }
        protected override void Iterate()
        {
            try
            {
                if (!Ready) return;
                if (!LeveragePosted) // todo generalize to first-iteration operations
                {
                    var leverage = PostLeverage();
                    LeveragePosted = leverage >= State.Leverage;
                    if (LeveragePosted)
                    {
                        Log($"--Current leverage is {leverage}x");
                    }
                    else return;
                }
                State.Read();
                ProcessExecutions();
                Execute();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }
        private void ProcessExecutions()
        {
            foreach (var e in State.Executions.Values)
            {
                switch (e.ExecType)
                {
                    case ExecType.Trade:
                        var side = (Side)e.Side;
                        string firstPart(string adj)
                        {
                            return $"--{side.ToPastTense()} {adj} {e.LastQty} at {e.Price}";
                        }
                        switch (e.OrdStatus)
                        {
                            case OrdStatus.PartiallyFilled:
                                var adj = e.LastQty == e.CumQty ? "first" : "another";
                                Log($"{firstPart(adj)} leaving {e.LeavesQty}/{e.OrderQty}");
                                break;
                            case OrdStatus.Filled:
                                if (e.OrdType == OrdType.Stop)
                                {
                                    //PriceVolatile = true; 
                                    Log($"--{e.Side} stop triggered");
                                }
                                adj = e.LastQty == e.OrderQty ? "all" : "remaining";
                                Log(firstPart(adj));
                                break;
                        }
                        break;
                }
            }
        }
        public abstract void Execute();
        protected double CalculateUndercutPrice(Side side)
        {
            var offset = side == Side.Buy ? Pip : -Pip;
            var orderBook = State.OrderBooks[side];
            var quantities = new Dictionary<double, long>();
            foreach (var order in State.Orders.Values)
            {
                if (order.Price is double p && order.OrderQty is long q)
                {
                    quantities[p] = q;
                }
            }
            double price;
            switch (UndercutTactic)
            {
                case UndercutTactic.UndercutAnyone:
                    price = orderBook.Keys.First() + offset;
                    break;
                case UndercutTactic.UndercutGreatestWall:
                    var greatest = orderBook.Aggregate((prev, next) => prev.Value < next.Value ? next : prev);
                    price = greatest.Key + offset;
                    break;
                case UndercutTactic.UndercutSmallestWall:
                default:
                    var walls = orderBook.Where(order =>
                    {
                        var thisPrice = order.Key;
                        var own = quantities.ContainsKey(thisPrice) ? quantities[thisPrice] : 0;
                        var quantity = (thisPrice + offset) * State.Budget;
                        quantity = quantity < MinimumWallHeight ? MinimumWallHeight : quantity;
                        return quantity < order.Value - own;
                    });
                    if (walls.Count() > 0)
                    {
                        price = walls.First().Key + offset;
                    }
                    else
                    {
                        price = orderBook.Last().Key;
                    }
                    break;
            }
            if (price == State.OrderBooks[side.Opposite()].First().Key) price -= offset;
            return price;
        }
        private double PostLeverage()
        {
            Log($"Posting {State.Leverage}x leverage");
            var response = Exchange.Rest.PostLeverage(new Dictionary<string, object>
            {
                ["leverage"] = State.Leverage.ToString(),
                ["symbol"] = Symbol.ToString(),
            });
            var position = response.ToJson<Position>();
            if (position?.Error == null)
            {
                return (double)position.Leverage;
            }
            return double.NaN;
        }
    }
}
