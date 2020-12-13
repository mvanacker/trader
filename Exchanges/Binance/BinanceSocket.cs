using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Exchanges.Binance
{
    public class BinanceSocket : SubscribedSocket
    {
        public BinanceSocket(string url) : base(url)
        {
            OnMessage += (s, e) =>
            {
                //Console.WriteLine(e.Data);
            };
        }
        
        protected override bool IsAuthenticated(Json json)
        {
            return true;
        }
        protected override void ConfirmSubscription(Json json)
        {

        }
    }
}
