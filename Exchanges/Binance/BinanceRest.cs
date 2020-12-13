using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Exchanges.Binance
{
    public class BinanceRest : Rest
    {
        public BinanceRest(string url, string key, string secret) : base(url, key, secret) { }

        protected override void LimitRate()
        {
        }
        protected override void GetRate(WebHeaderCollection headers)
        {
        }

        public override string PostOrder(IDictionary<string, object> param)
        {
            throw new NotImplementedException();
        }
        public override string PutOrder(IDictionary<string, object> param)
        {
            throw new NotImplementedException();
        }
        public override string PostLeverage(IDictionary<string, object> param)
        {
            throw new NotImplementedException();
        }
        public override string DeleteOrder(IEnumerable<Guid> orderIds)
        {
            throw new NotImplementedException();
        }
        public override string DeleteAllOrders()
        {
            throw new NotImplementedException();
        }

        public override string GetTrade(string symbol, int start, int count = 500)
        {
            throw new NotImplementedException();
        }
        public override string GetTrade(IDictionary<string, object> param)
        {
            throw new NotImplementedException();
        }

        public override string GetTradeBin(TradeBin tradeBin, string symbol)
        {
            return GetTradeBin(new Dictionary<string, object>
            {
                ["symbol"] = symbol,
                ["interval"] = tradeBin.ToShortLabel(),
            });
        }
        public override string GetTradeBin(TradeBin tradeBin, string symbol, int start, int count = 500)
        {
            throw new NotImplementedException();
        }
        public override string GetTradeBin(TradeBin tradeBin, string symbol, DateTime start)
        {
            return GetTradeBin(new Dictionary<string, object>
            {
                ["symbol"] = symbol,
                ["interval"] = tradeBin.ToShortLabel(),
                ["startTime"] = Utils.SinceEpochMs(start)
            });
        }
        public override string GetTradeBin(TradeBin tradeBin, string symbol, DateTime start, DateTime end)
        {
            return GetTradeBin(new Dictionary<string, object>
            {
                ["symbol"] = symbol,
                ["interval"] = tradeBin.ToShortLabel(),
                ["startTime"] = Utils.SinceEpochMs(start),
                ["endTime"] = Utils.SinceEpochMs(end)
            });
        }
        public override string GetTradeBin(IDictionary<string, object> param)
        {
            return Query("GET", "/klines", param);
        }

        public override string GetTradeHistory(int start, int count = 500)
        {
            throw new NotImplementedException();
        }
        public override string GetTradeHistory(IDictionary<string, object> param)
        {
            throw new NotImplementedException();
        }
    }
}
