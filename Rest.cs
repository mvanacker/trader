using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace Trader
{
    public abstract class Rest
    {
        public Rest(string url, string key, string secret)
        {
            Url = url;
            Key = key;
            Secret = secret;
        }

        protected string Url { get; set; }
        protected string Key { get; set; }
        protected string Secret { get; set; }

        private readonly object _key = new object();

        protected string Query(string method, string function, IDictionary<string, object> param = null, bool json = false, bool auth = false)
        {
            lock (_key)
            {
                LimitRate();
                var paramData = json ? Utils.BuildJSON(param) : Utils.BuildQuery(param);
                var url = $"/api/v1{function}";
                if (method == "GET" && !string.IsNullOrEmpty(paramData))
                {
                    url = $"{url}?{paramData}";
                }
                var postData = method == "GET" ? "" : paramData;
                var request = WebRequest.Create($"{Url}{url}");
                request.Method = method;
                if (auth)
                {
                    var nonce = Utils.Nonce.ToString();
                    var message = $"{method}{url}{nonce}{postData}";
                    var signature = Utils.Hmacsha256(Secret, message);
                    request.Headers.Add("api-nonce", nonce);
                    request.Headers.Add("api-key", Key);
                    request.Headers.Add("api-signature", signature);
                }
                WebResponse response;
                var error = false;
                try
                {
                    if (!string.IsNullOrEmpty(postData))
                    {
                        request.ContentType = json ? "application/json" : "application/x-www-form-urlencoded";
                        var data = Encoding.UTF8.GetBytes(postData);
                        using (var stream = request.GetRequestStream())
                        {
                            stream.Write(data, 0, data.Length);
                        }
                    }
                    response = request.GetResponse();
                }
                catch (WebException wex)
                {
                    response = wex.Response;
                    if (response == null)
                    {
                        Console.Error.WriteLine("Response was null. Are you connected?");
                        //throw;
                        return string.Empty;
                    }
                    error = true;
                }
                GetRate(response.Headers);
                string result;
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    try
                    {
                        result = reader.ReadToEnd();
                    }
                    catch (IOException e)
                    {
                        Console.Error.WriteLine(e);
                        result = string.Empty;
                    }
                }
                response.Close();
                //Console.WriteLine(result);
                if (error)
                {
                    var message = result.ToJson().Error.Message;
                    Console.Error.WriteLine($"{method} {function} {message}");
                }
                return result;
            }
        }
        protected abstract void LimitRate();
        protected abstract void GetRate(WebHeaderCollection headers);

        public abstract string PostOrder(IDictionary<string, object> param);
        public abstract string PutOrder(IDictionary<string, object> param);
        public abstract string PostLeverage(IDictionary<string, object> param);
        public virtual string DeleteOrder(Guid orderId)
        {
            return DeleteOrder(new Guid[] { orderId });
        }
        public abstract string DeleteOrder(IEnumerable<Guid> orderIds);
        public abstract string DeleteAllOrders();

        public abstract string GetTradeBin(TradeBin tradeBin, string symbol);
        public abstract string GetTradeBin(TradeBin tradeBin, string symbol, int start, int count = 500);
        public abstract string GetTradeBin(TradeBin tradeBin, string symbol, DateTime start);
        public abstract string GetTradeBin(TradeBin tradeBin, string symbol, DateTime start, DateTime end);
        public virtual string GetTradeBin(TradeBin tradeBin, Symbol symbol)
        {
            return GetTradeBin(tradeBin, symbol.ToString());
        }
        public virtual string GetTradeBin(TradeBin tradeBin, Symbol symbol, int start, int count = 500)
        {
            return GetTradeBin(tradeBin, symbol.ToString(), start, count);
        }
        public virtual string GetTradeBin(TradeBin tradeBin, Symbol symbol, DateTime start)
        {
            return GetTradeBin(tradeBin, symbol.ToString(), start);
        }
        public virtual string GetTradeBin(TradeBin tradeBin, Symbol symbol, DateTime start, DateTime end)
        {
            return GetTradeBin(tradeBin, symbol.ToString(), start, end);
        }
        public virtual string GetTradeBin(TradeBin tradeBin, Index index)
        {
            return GetTradeBin(tradeBin, index.ToString());
        }
        public virtual string GetTradeBin(TradeBin tradeBin, Index index, int start, int count = 500)
        {
            return GetTradeBin(tradeBin, index.ToString(), start, count);
        }
        public virtual string GetTradeBin(TradeBin tradeBin, Index index, DateTime start)
        {
            return GetTradeBin(tradeBin, index.ToString(), start);
        }
        public virtual string GetTradeBin(TradeBin tradeBin, Index index, DateTime start, DateTime end)
        {
            return GetTradeBin(tradeBin, index.ToString(), start, end);
        }
        public abstract string GetTradeBin(IDictionary<string, object> param);

        public abstract string GetTrade(string symbol, int start, int count = 500);
        public virtual string GetTrade(Symbol symbol, int start, int count = 500)
        {
            return GetTrade(symbol.ToString(), start, count);
        }
        public abstract string GetTrade(IDictionary<string, object> param);

        public abstract string GetTradeHistory(int start, int count = 500);
        public abstract string GetTradeHistory(IDictionary<string, object> param);
    }
}
