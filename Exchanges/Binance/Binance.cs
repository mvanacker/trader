using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Exchanges.Binance
{
    public class Binance : Exchange
    {
        public Binance()
        {
            Rest = new BinanceRest(RestUrl, Key, Secret);
            //var stuff = Rest.GetTradeBin(TradeBin.FiveMinute, Symbol.EOSBTC, Utils.Epoch);
            //Console.WriteLine(stuff);
            //<symbol>@kline_<interval>
            var ethbtc = "ethbtc@kline_1m";
            var eosbtc = "eosbtc@kline_1m";
            // raw stream
            var singleUrl = $"{SocketUrl}/ws/{ethbtc}";
            // combo stream
            var comboUrl = $"{SocketUrl}/stream?streams={ethbtc}/{eosbtc}";
            //
            Socket = new BinanceSocket(comboUrl)
            {
                Converter = new ScriptConverter($"BinanceConverter.py -prefix {Name}"),
                Name = Name
            };
        }

        public override string Name => "Binance";
        protected override string RestUrl => "https://api.binance.com";
        protected override string SocketUrl => "wss://stream.binance.com:9443";
        protected override string Key => "9QQi1KVklxMoJu3yKmWIJchWuh2hrDdPFgKP1HCwctmWLv66S0Y0GffMZSbiIQKk";
        protected override string Secret => "WAbbWTeCYfTa99Res7wlqEqPjANZrirnmgS6c1jI7Df8K3hDLNYz5H2Kpkw6ytp6";

        public override Order PostOrder(Side side, Symbol symbol, double price, long quantity, bool reduceOnly = false)
        {
            return new Order();
        }
        public override Order PostStop(Side side, Symbol symbol, double price)
        {
            return new Order();
        }
        public override Order PostMarketOrder(Side side, Symbol symbol, long quantity)
        {
            return new Order();
        }
        public override void AmendBulkOrder(Side side, double price, IEnumerable<Order> orders)
        {
        }
        public override void AmendOrder(Side side, double price, Order order)
        {
        }
        public override void AmendOrder(Side side, double price, long quantity, Order order)
        {
        }
        public override void AmendStop(Side side, double price, Order order)
        {
        }
        public override void Cancel(Order order)
        {
        }
        public override void Cancel(Guid orderId)
        {
        }
        public override void CancelAll()
        {
        }
        public override void CancelAllExceptStops(IEnumerable<Order> orders)
        {
        }
        public override void CancelAll(IEnumerable<Order> orders)
        {
        }
        public override void CancelAll(IEnumerable<Guid> orderIds)
        {
        }
    }
}
