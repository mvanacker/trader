using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Trader.Exchanges.Bitmex
{
    public class BitmexRest : Rest
    {
        public BitmexRest(string url, string key, string secret) : base(url, key, secret) { }

        private int RateRemaining { get; set; }
        private int RateLimit { get; set; }
        private bool RateLimited { get; set; }
        private long RateLimitReset { get; set; }
        private int RateLimitRemaining => (int)(1000 * (RateLimitReset - Utils.UtcTime));

        protected override void LimitRate()
        {
            if (RateLimited && RateLimitRemaining > 0)
            {
                Thread.Sleep(RateLimitRemaining);
            }
        }
        protected override void GetRate(WebHeaderCollection headers)
        {
            for (int i = 0; i < headers.Count; i++)
            {
                string key = headers.Keys[i], value = headers[key];
                if (key == "X-RateLimit-Remaining")
                {
                    RateRemaining = int.Parse(value);
                    RateLimited = RateRemaining <= 10;
                }
                else if (key == "X-RateLimit-Limit")
                {
                    RateLimit = int.Parse(value);
                }
                else if (key == "X-RateLimit-Reset")
                {
                    RateLimitReset = long.Parse(value);
                }
            }
            if (RateLimited)
            {
                Console.WriteLine($"[{RateRemaining}/{RateLimit}] Rate limit reached ({RateLimitRemaining} remaining)");
            }
        }

        public override string PostOrder(IDictionary<string, object> param)
        {
            return Query("POST", "/order", param, auth: true);
        }
        public override string PutOrder(IDictionary<string, object> param)
        {
            return Query("PUT", "/order", param, auth: true);
        }
        public override string PostLeverage(IDictionary<string, object> param)
        {
            return Query("POST", "/position/leverage", param, auth: true);
        }
        public override string DeleteOrder(IEnumerable<Guid> orderIds)
        {
            var param = new Dictionary<string, object>
            {
                ["orderID"] = string.Join(",", orderIds)
            };
            return Query("DELETE", "/order", param, json: true, auth: true);
        }
        public override string DeleteAllOrders()
        {
            return Query("DELETE", "/order/all", auth: true);
        }

        public override string GetTradeBin(TradeBin tradeBin, string symbol)
        {
            return GetTradeBin(new Dictionary<string, object>
            {
                ["binSize"] = tradeBin.ToLabel().Split('n')[1],
                ["partial"] = false,
                ["symbol"] = symbol,
                ["count"] = 500
            });
        }
        public override string GetTradeBin(TradeBin tradeBin, string symbol, int start, int count = 500)
        {
            return GetTradeBin(new Dictionary<string, object>
            {
                ["binSize"] = tradeBin.ToLabel().Split('n')[1],
                ["partial"] = false,
                ["symbol"] = symbol,
                ["start"] = start,
                ["count"] = 500
            });
        }
        public override string GetTradeBin(TradeBin tradeBin, string symbol, DateTime start)
        {
            return GetTradeBin(new Dictionary<string, object>
            {
                ["binSize"] = tradeBin.ToLabel().Split('n')[1],
                ["partial"] = false,
                ["symbol"] = symbol,
                ["count"] = 500,
                ["startTime"] = start.ToString(Const.DATETIME_FORMAT)
            });
        }
        public override string GetTradeBin(TradeBin tradeBin, string symbol, DateTime start, DateTime end)
        {
            return GetTradeBin(new Dictionary<string, object>
            {
                ["binSize"] = tradeBin.ToLabel().Split('n')[1],
                ["partial"] = false,
                ["symbol"] = symbol,
                ["count"] = 500,
                ["startTime"] = start.ToString(Const.DATETIME_FORMAT),
                ["endTime"] = end.ToString(Const.DATETIME_FORMAT)
            });
        }
        public override string GetTradeBin(IDictionary<string, object> param)
        {
            return Query("GET", "/trade/bucketed", param, auth: true);
        }

        public override string GetTrade(string symbol, int start, int count = 500)
        {
            return GetTrade(new Dictionary<string, object>
            {
                ["symbol"] = symbol,
                ["start"] = start.ToString(),
                ["count"] = count.ToString(),
            });
        }
        public override string GetTrade(IDictionary<string, object> param)
        {
            return Query("GET", "/trade", param, auth: true);
        }

        public override string GetTradeHistory(int start, int count = 500)
        {
            var param = new Dictionary<string, object>
            {
                ["start"] = start.ToString(),
                ["count"] = count.ToString(),
            };
            return GetTradeHistory(param);
        }
        public override string GetTradeHistory(IDictionary<string, object> param)
        {
            return Query("GET", "/execution/tradeHistory", param, auth: true);
        }
    }
}
