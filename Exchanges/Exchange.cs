using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace Trader.Exchanges
{
    using Database;

    public abstract class Exchange
    {
        public Exchange()
        {
            foreach (var symbol in Symbols)
            {
                Prices[symbol] = new Dictionary<TradeBin, Prices>();
                LatestTradeBinUpdates[symbol] = new Dictionary<TradeBin, DateTime>();
                foreach (var bin in TradeBins)
                {
                    Prices[symbol][bin] = new Prices();
                }
            }
            foreach (var index in Indices)
            {
                IndexPrices[index] = new Prices();
                LatestIndexUpdates[index] = Utils.Epoch;
            }
        }

        public Dictionary<Symbol, Strategy> Strategies { get; } = new Dictionary<Symbol, Strategy>();
        public Dictionary<Symbol, Dictionary<TradeBin, Prices>> Prices { get; } = new Dictionary<Symbol, Dictionary<TradeBin, Prices>>();
        public Dictionary<Symbol, Dictionary<TradeBin, DateTime>> LatestTradeBinUpdates { get; } = new Dictionary<Symbol, Dictionary<TradeBin, DateTime>>();
        public Dictionary<Index, Prices> IndexPrices { get; } = new Dictionary<Index, Prices>();
        public Dictionary<Index, DateTime> LatestIndexUpdates { get; } = new Dictionary<Index, DateTime>();

        public virtual string Name { get; }
        protected virtual string RestUrl { get; }
        protected virtual string SocketUrl { get; }
        protected virtual string Key { get; }
        protected virtual string Secret { get; }
        public virtual HashSet<Symbol> Symbols { get; } = new HashSet<Symbol>();
        public virtual HashSet<Index> Indices { get; } = new HashSet<Index>();
        public virtual HashSet<TradeBin> TradeBins { get; } = new HashSet<TradeBin>();

        public event EventHandler Socketed;

        public Rest Rest { get; protected set; }
        private SubscribedSocket _socket;
        public SubscribedSocket Socket
        {
            get => _socket; protected set
            {
                _socket = value;
                Socket.Subscribed += RunStrategies;
                Socket.OnClose += ResetStrategies;
                Socket.TableCreated += AddPrice;
                Socket.TableChanged += AddPrice;
                Socketed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void RunStrategies(object sender, EventArgs e)
        {
            foreach (var strategy in Strategies.Values.Where(s => !s.IsBusy))
            {
                strategy.RunWorkerAsync();
            }
        }
        private void ResetStrategies(object sender, CloseEventArgs e)
        {
            foreach (var strategy in Strategies.Values)
            {
                strategy.State.Reset();
            }
        }
        private void AddPrice(object sender, TableEventArgs e)
        {
            var label = e.Label;
            if (TradeBins.Select(t => t.ToLabel()).Contains(label))
            {
                var bin = label.ToTradeBin();
                var anyPrices = Prices.First().Value[bin][Price.Close];
                Func<Enum, TradeBin, DateTime> getLatest;
                if (anyPrices.Count == 0)
                {
                    getLatest = (symbol, tradeBin) => Const.TRADE_BIN_INITIAL_TIMESTAMP[tradeBin];
                }
                else
                {
                    getLatest = (symbol, tradeBin) =>
                    {
                        if (symbol is Symbol s)
                        {
                            return LatestTradeBinUpdates[s][tradeBin];
                        }
                        else if (symbol is Index i)
                        {
                            return LatestIndexUpdates[i];
                        }
                        else throw new InvalidOperationException();
                    };
                }
                UpdatePrices(e, getLatest);
            }
        }
        private void UpdatePrices(TableEventArgs e, Func<Enum, TradeBin, DateTime> getLatest)
        {
            var label = e.Label;
            var tradeBin = label.ToTradeBin();
            var table = $"{Name}_{label}";
            foreach (var symbol in Symbols)
            {
                var prices = Prices[symbol][tradeBin];
                var timestamp = getLatest(symbol, tradeBin);
                var query = $"select distinct [open], [close], [high], [low], [timestamp] from [{table}] where [symbol] = '{symbol}' and [timestamp] > '{timestamp}' order by [timestamp]";
                var result = Database.Select(query).Rows;
                foreach (DataRow row in result)
                {
                    var hloc = new object[]
                    {
                        row["high"], row["low"], row["open"], row["close"]
                    };
                    prices.AddHloc(hloc);
                    LatestTradeBinUpdates[symbol][tradeBin] = (DateTime)row["timestamp"];
                }
            }
            foreach (var index in Indices)
            {
                var prices = IndexPrices[index];
                var timestamp = getLatest(index, tradeBin);
                var query = $"select distinct [open], [close], [high], [low], [timestamp] from [{table}] where [symbol] = '{index.ToIndexString()}' and [timestamp] > '{timestamp}' order by [timestamp]";
                var result = Database.Select(query).Rows;
                foreach (DataRow row in result)
                {
                    var hloc = new object[]
                    {
                        row["high"], row["low"], row["open"], row["close"]
                    };
                    prices.AddHloc(hloc);
                    LatestIndexUpdates[index] = (DateTime)row["timestamp"];
                }
            }
        }

        public abstract Order PostOrder(Side side, Symbol symbol, double price, long quantity, bool reduceOnly = false);
        public abstract Order PostStop(Side side, Symbol symbol, double price);
        public abstract Order PostMarketOrder(Side side, Symbol symbol, long quantity);
        public abstract void AmendBulkOrder(Side side, double price, IEnumerable<Order> orders);
        public abstract void AmendOrder(Side side, double price, Order order);
        public abstract void AmendOrder(Side side, double price, long quantity, Order order);
        public abstract void AmendStop(Side side, double price, Order order);
        public abstract void Cancel(Order order);
        public abstract void Cancel(Guid orderId);
        public abstract void CancelAll();
        public abstract void CancelAllExceptStops(IEnumerable<Order> orders);
        public abstract void CancelAll(IEnumerable<Order> orders);
        public abstract void CancelAll(IEnumerable<Guid> orderIds);
    }
}
