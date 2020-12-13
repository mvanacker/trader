using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Authentication;
using WebSocketSharp;

namespace Trader
{
    public abstract class SecureWebSocket : WebSocket
    {
        public SecureWebSocket(string url, string key, string secret) : base(url)
        {
            Key = key;
            Secret = secret;
            SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12;
            //Log.Output = (a, b) => { }; // hack to disable built-in logging
        }

        protected string Key { get; }
        protected string Secret { get; }
    }
}
