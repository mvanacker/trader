using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Trader.Exchanges
{
    using Database;

    public class State
    {
        public State(Symbol symbol)
        {
            Symbol = symbol;
        }

        public Symbol Symbol { get; }
        public double Leverage { get; set; }
        public int OrderBookDepth { get; set; }
        public double Weight { get; set; }

        private string OrderTable { get; set; }
        private string MarginTable { get; set; }
        private string PositionTable { get; set; }
        private string ExecutionTable { get; set; }
        private string OrderBookTable { get; set; }
        public bool TablesCreated => !string.IsNullOrEmpty(OrderTable)
            && !string.IsNullOrEmpty(MarginTable)
            && !string.IsNullOrEmpty(PositionTable)
            && !string.IsNullOrEmpty(ExecutionTable)
            && !string.IsNullOrEmpty(OrderBookTable);

        public void Prepare(object sender, TableEventArgs e)
        {
            switch (e.Label)
            {
                case "order":
                    if (string.IsNullOrEmpty(OrderTable)) OrderTable = e.Table;
                    break;
                case "margin":
                    if (string.IsNullOrEmpty(MarginTable)) MarginTable = e.Table;
                    break;
                case "position":
                    if (string.IsNullOrEmpty(PositionTable)) PositionTable = e.Table;
                    break;
                case "execution":
                    if (string.IsNullOrEmpty(ExecutionTable)) ExecutionTable = e.Table;
                    break;
                case "orderBookL2":
                    if (string.IsNullOrEmpty(OrderBookTable)) OrderBookTable = e.Table;
                    break;
            }
        }
        public void Reset()
        {
            OrderTable = string.Empty;
            MarginTable = string.Empty;
            PositionTable = string.Empty;
            ExecutionTable = string.Empty;
            OrderBookTable = string.Empty;
        }

        public Dictionary<Guid, Execution> Executions { get; } = new Dictionary<Guid, Execution>();
        public Dictionary<Guid, Order> Orders { get; } = new Dictionary<Guid, Order>();
        public Dictionary<Side, SortedDictionary<double, long>> OrderBooks { get; } = new Dictionary<Side, SortedDictionary<double, long>>
        {
            [Side.Sell] = new SortedDictionary<double, long>(),
            [Side.Buy] = new SortedDictionary<double, long>()
        };
        public Position Position { get; } = new Position();
        public Margin Margin { get; private set; } = new Margin();
        public double Budget => Margin.WalletBalance * Leverage * Weight * Const.BITCOIN_PER_SATOSHI;

        private DateTime LastExecution { get; set; } = DateTime.UtcNow;
        public Dictionary<Side, DateTime> LastFills { get; } = new Dictionary<Side, DateTime>
        {
            [Side.Sell] = Utils.Epoch,
            [Side.Buy] = Utils.Epoch
        };

        public void Read()
        {
            ReadExecutions();
            ReadOrders();
            ReadPosition();
            ReadOrderBooks();
            ReadMargin();
        }
        protected virtual void ReadExecutions()
        {
            var query = $"select [side], [execType], [ordStatus], [ordType], [price], [stopPx], [leavesQty], [cumQty], [orderQty], [lastQty], [execID], [orderID], [timestamp] from [{ExecutionTable}] where [timestamp] > '{LastExecution.ToString(Const.DATETIME_FORMAT)}'";
            var result = Database.Select(query).Rows;
            Executions.Clear();
            foreach (DataRow row in result)
            {
                var execution = new Execution(row);
                Executions[execution.OrderId] = execution;
                updateTimestamps(execution);
            }
            void updateTimestamps(Execution execution)
            {
                if (execution.Timestamp > LastExecution)
                {
                    LastExecution = execution.Timestamp;
                }
                if (execution.OrdStatus == OrdStatus.Filled)
                {
                    LastFills[(Side)execution.Side] = execution.Timestamp;
                }
            }
        }
        protected virtual void ReadOrders()
        {
            var query = $"select [side], [ordStatus], [ordType], [price], [stoppx], [leavesQty], [cumQty], [orderQty], [execInst], [orderID], [timestamp] from [{OrderTable}] where [symbol] = '{Symbol}' and ([ordStatus] = 'New' or [ordStatus] = 'PartiallyFilled')";
            var result = Database.Select(query).Rows;
            Orders.Clear();
            foreach (DataRow row in result)
            {
                var order = new Order(row);
                Orders[order.OrderId] = order;
            }
        }
        protected virtual void ReadPosition()
        {
            var query = $"select [currentQty], [leverage] from [{PositionTable}] where [symbol] = '{Symbol}'";
            var result = Database.Select(query).Rows[0];
            Position.CurrentQty = (long)result["currentQty"];
            Position.Leverage = (double)result["leverage"];
        }
        protected virtual void ReadMargin()
        {
            var query = $"select [walletBalance], [marginUsedPcnt] from [{MarginTable}]";
            var row = Database.Select(query).Rows[0];
            Margin = new Margin(row);
        }
        protected virtual void ReadOrderBooks()
        {
            OrderBooks[Side.Sell] = getOrderBook(Side.Sell);
            OrderBooks[Side.Buy] = getOrderBook(Side.Buy);
            SortedDictionary<double, long> getOrderBook(Side side)
            {
                string query;
                SortedDictionary<double, long> orders;
                switch (side)
                {
                    case Side.Buy:
                        query = $"select [price], [size] from [{OrderBookTable}] where [side] = '{side}' and [symbol] = '{Symbol}' and [price] >= (select top 1 [price] from [{OrderBookTable}] where [side] = '{side}' and [symbol] = '{Symbol}' order by [price] desc) - {OrderBookDepth}";
                        var comparer = Comparer<double>.Create((x, y) => y.CompareTo(x));
                        orders = new SortedDictionary<double, long>(comparer);
                        break;
                    case Side.Sell:
                        query = $"select [price], [size] from [{OrderBookTable}] where [side] = '{side}' and [symbol] = '{Symbol}' and [price] <= (select top 1 [price] from [{OrderBookTable}] where [side] = '{side}' and [symbol] = '{Symbol}' order by [price]) + {OrderBookDepth}";
                        orders = new SortedDictionary<double, long>();
                        break;
                    default:
                        throw new ArgumentException();
                }
                var result = Database.Select(query).Rows;
                foreach (DataRow row in result)
                {
                    var price = (double)row["price"];
                    var size = (long)row["size"];
                    orders[price] = size;
                }
                return orders;
            }
        }
    }
}
