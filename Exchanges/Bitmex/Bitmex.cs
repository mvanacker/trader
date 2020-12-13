using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using ServiceStack.Text;
using WebSocketSharp;

namespace Trader.Exchanges.Bitmex
{
    using Database;

    public class Bitmex : Exchange
    {
        public Bitmex()
        {
            foreach (var symbol in Symbols)
            {
                Strategies[symbol] = new ChannelStrategy(this, symbol, TradeBin.FiveMinute)
                {
                    Pip = 0.5,
                    StepHeight = 0.05,
                    UndercutTactic = UndercutTactic.UndercutSmallestWall,
                    MinimumWallHeight = 199999,
                    //MaximumOrderQuantity = 9999,
                    State = new State(symbol)
                    {
                        Leverage = 6,
                        OrderBookDepth = 10,
                        Weight = 0.99,
                    }
                };
                //Strategies[symbol] = new DirectionalStrategy(this)
                //{
                //    Pip = 0.5,
                //    State = new State(symbol)
                //    {
                //        Leverage = 1,
                //        OrderBookDepth = 10,
                //        Weight = 0.99,
                //    }
                //};
            }
            Rest = new BitmexRest(RestUrl, Key, Secret);
            Socket = CreateSocket();
        }

        public override string Name => "BitMEX";
        protected override string RestUrl => "https://www.bitmex.com";
        protected override string SocketUrl => "wss://www.bitmex.com/realtime";
        protected override string Key => "4Dnd8bU48s0zoonOynFv3tg2";
        protected override string Secret => "dgeyJxhB820e0FdferJzy3O7hHOgw3g4kRadhNHIspkSrC7k";
        //protected override string Key => "zNZBY7nSAuxjFAI2q6p6R0ih";
        //protected override string Secret => "0he7WSfHhv_3cGtYAT7EQf0SwUbfK85j17dvRtI8UJj9DUaI";
        //protected override string Key => "ZVB5veO0y7ePuF_axhyYyM4C";
        //protected override string Secret => "8pm2iiRAIGoJn2_lxLST8jB2EYD-vJ0jrTfsQHJRwR9pmsvC";
        public override HashSet<Symbol> Symbols { get; } = new HashSet<Symbol>
        {
             Symbol.XBTUSD, //Symbol.XBTM18, Symbol.XBTU18, //Symbol.ADAM18, Symbol.BCHM18, Symbol.ETHM18, Symbol.LTCM18, Symbol.XRPM18
            //Symbol.XBTM18//, Symbol.XBTUSD, Symbol.XBTU18, //Symbol.ADAM18, Symbol.BCHM18, Symbol.ETHM18, Symbol.LTCM18, Symbol.XRPM18
        };
        public override HashSet<Index> Indices { get; } = new HashSet<Index>()
        {
            Index.BVOL24H,
        };
        public override HashSet<TradeBin> TradeBins { get; } = new HashSet<TradeBin>
        {
            TradeBin.OneMinute, TradeBin.FiveMinute, TradeBin.OneHour, TradeBin.OneDay
        };

        private BitmexSocket CreateSocket()
        {
            var socket = new BitmexSocket(SocketUrl, Key, Secret)
            {
                Converter = new ScriptConverter($"BitmexConverter.py -prefix {Name}"),
                Name = Name
            };
            foreach (var symbol in Symbols)
            {
                socket.TableCreated += Strategies[symbol].State.Prepare;
            }
            socket.Subscriptions.Add(CreateSubscription());
            socket.TradeBinPartial += InsertTradeBinData;
            socket.OnClose += AutoReconnect;
            return socket;
        }
        private Subscription CreateSubscription()
        {
            var subscription = new Subscription();
            subscription.AddTopic("order");
            subscription.AddTopic("margin");
            subscription.AddTopic("position");
            subscription.AddTopic("execution");
            var symbols = Symbols.Select(s => s.ToString());
            foreach (var symbol in symbols)
            {
                subscription.AddTopic("trade", symbol);
                subscription.AddTopic("orderBookL2", symbol);
                foreach (var bin in TradeBins.Select(t => t.ToLabel()))
                {
                    subscription.AddTopic(bin, symbol);
                }
            }
            var indices = Indices.Select(i => i.ToIndexString());
            foreach (var index in indices)
            {
                subscription.AddTopic("tradeBin5m", index);
            }
            return subscription;
        }
        private void InsertTradeBinData(object sender, TradeBinEventArgs e)
        {
            var label = e.TradeBin.ToLabel();
            var table = $"{Name}_{label}";
            var symbols = Symbols.Select(s => s.ToString());
            IEnumerable<string> cat;
            if (e.TradeBin == TradeBin.FiveMinute)
            {
                var indices = Indices.Select(i => i.ToIndexString());
                cat = symbols.Concat(indices);
            }
            else
            {
                cat = symbols;
            }
            foreach (var symbol in cat)
            {
                var response = string.Empty;
                while (response != "[]")
                {
                    if (e.EmptyPartial)
                    {
                        // todo what if tradebin table is still empty?
                        response = GetTradeBinsAfterLatest(e.TradeBin, symbol);
                    }
                    else
                    {
                        response = GetTradeBinsBetweenLatest(e.TradeBin, symbol);
                    }
                    EncodeAndStore(response, label);
                }
            }
        }
        private string GetTradeBinsAfterLatest(TradeBin tradeBin, string symbol)
        {
            var table = $"{Name}_{tradeBin.ToLabel()}";
            var query = $"select top 1 [timestamp] from [{table}] where [symbol] = '{symbol}' order by [timestamp] desc";
            var result = Database.Select(query).Rows;
            var start = (DateTime)result[0]["timestamp"] + Const.TIME_UNIT;
            return Rest.GetTradeBin(tradeBin, symbol, start);
        }
        private string GetTradeBinsBetweenLatest(TradeBin tradeBin, string symbol)
        {
            var table = $"{Name}_{tradeBin.ToLabel()}";
            var query = $"select distinct top 2 [timestamp] from [{table}] where [symbol] = '{symbol}' order by [timestamp] desc";
            var result = Database.Select(query).Rows;
            var end = (DateTime)result[0]["timestamp"] - Const.TIME_UNIT;
            var start = (DateTime)result[1]["timestamp"] + Const.TIME_UNIT;
            return Rest.GetTradeBin(tradeBin, symbol, start, end);
        }
        private void EncodeAndStore(string response, string label)
        {
            var data = response.EncodeData(label);
            Socket.Store(data);
        }
        private void AutoReconnect(object sender, CloseEventArgs e)
        {
            if (Const.AUTO_RECONNECT)
            {
                while (DateTime.Now < Socket.StartTime + Const.RECONNECTION_TIMEOUT)
                {
                    Thread.Sleep(Const.SHORT_TIMEOUT);
                }
                Socket = CreateSocket();
                Socket.ConnectAsync();
            }
        }

        public override Order PostOrder(Side side, Symbol symbol, double price, long quantity, bool reduceOnly = false)
        {
            var postOnly = "ParticipateDoNotInitiate";
            var execInst = reduceOnly ? $"{postOnly},ReduceOnly" : postOnly;
            var response = Rest.PostOrder(new Dictionary<string, object>
            {
                ["side"] = side,
                ["price"] = price.ToString(),
                ["orderQty"] = quantity.ToString(),
                ["symbol"] = symbol.ToString(),
                ["execInst"] = execInst
            });
            var order = response.ToJson<Order>();
            if (order?.Error == null)
            {
                if (order.OrdStatus == OrdStatus.New)
                {
                    //Log($"{side} {quantity} at {price}");
                }
                else
                {
                    //Log($"-{side} bounced at {price}");
                }
            }
            return order;
        }
        public override Order PostStop(Side side, Symbol symbol, double price)
        {
            var response = Rest.PostOrder(new Dictionary<string, object>
            {
                ["side"] = side,
                ["stopPx"] = price.ToString(),
                ["symbol"] = symbol.ToString(),
                ["ordType"] = "Stop",
                ["execInst"] = "Close,LastPrice"
            });
            var order = response.ToJson<Order>();
            if (order?.Error == null)
            {
                if (order.OrdStatus == OrdStatus.New)
                {
                    //Log($"({side} stop placed at {price})");
                }
                else
                {
                    //Log($"! {side} stop failed at {price}");
                }
            }
            return order;
        }
        public override Order PostMarketOrder(Side side, Symbol symbol, long quantity)
        {
            var response = Rest.PostOrder(new Dictionary<string, object>
            {
                ["side"] = side,
                ["orderQty"] = quantity.ToString(),
                ["symbol"] = symbol.ToString(),
                ["ordType"] = "Market",
            });
            var order = response.ToJson<Order>();
            if (order?.Error == null)
            {
                //Log($"{side} {quantity} at market price");
            }
            else
            {
                PostMarketOrder(side, symbol, quantity);
            }
            return order;
        }
        public override void AmendBulkOrder(Side side, double price, IEnumerable<Order> orders)
        {
            var joinedIds = string.Join(",", orders.Select(order => order.OrderId));
            if (orders.Any(order => order.Price != price))
            {
                //Log($"{side} amended to {price} for {joinedIds}");
                var response = Rest.PutOrder(new Dictionary<string, object>
                {
                    ["orderID"] = joinedIds,
                    ["price"] = price.ToString()
                });
            }
        }
        public override void AmendOrder(Side side, double price, Order order)
        {
            AmendOrder(side, price, order.LeavesQty, order);
        }
        public override void AmendOrder(Side side, double price, long quantity, Order order)
        {
            if (order.Price != price || order.LeavesQty != quantity)
            {
                var response = Rest.PutOrder(new Dictionary<string, object>
                {
                    ["orderID"] = order.OrderId,
                    ["price"] = price.ToString(),
                    ["orderQty"] = (quantity + order.CumQty).ToString(),
                    ["ordType"] = order.OrdType,
                });
                var json = response.ToJson<Order>();
                if (json?.Error == null)
                {
                    var qty = json.LeavesQty == json.OrderQty ? $"{json.OrderQty}" : $"{json.LeavesQty}/{json.OrderQty}";
                    var type = json.OrdType == OrdType.Stop ? " stop" : "";
                    //Log($"{side}{type} amended to {qty} at {price}");
                }
            }
        }
        public override void AmendStop(Side side, double price, Order order)
        {
            if (order.StopPx != price)
            {
                var response = Rest.PutOrder(new Dictionary<string, object>
                {
                    ["orderID"] = order.OrderId,
                    ["stopPx"] = price.ToString(),
                });
                var json = response.ToJson();
                if (json?.Error == null)
                {
                    //Log($"({side} stop amended to {price})");
                }
            }
        }
        public override void Cancel(Order order) => Cancel(order.OrderId);
        public override void Cancel(Guid orderId) => Rest.DeleteOrder(orderId);
        public override void CancelAll()
        {
            Rest.DeleteAllOrders();
        }
        public override void CancelAllExceptStops(IEnumerable<Order> orders)
        {
            CancelAll(orders.Where(o => o.OrdType != OrdType.Stop));
        }
        public override void CancelAll(IEnumerable<Order> orders)
        {
            CancelAll(orders.Select(o => o.OrderId));
        }
        public override void CancelAll(IEnumerable<Guid> orderIds)
        {
            // todo go back to bulk delete
            foreach (var id in orderIds)
            {
                Rest.DeleteOrder(id);
            }
        }
    }
}
