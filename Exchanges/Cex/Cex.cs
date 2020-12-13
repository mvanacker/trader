//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Trader.Exchanges
//{
//    public class Cex : Exchange
//    {
//        public Cex()
//        {
//            Name = Cex;
//            Key = IAKWoJ6OFzTQqCoBJ7AZRuWVmvc;
//            Secret = q0KY8nnd5VkFX4IiYJYkn5TAk;
//            SocketUri = wss://ws.cex.io/ws/;
//            Heartbeat = {\e\:\pong\};
//            OldSubscriptions.Add({\e\:\order-book-subscribe\,\data\:{\pair\:[\BTC\,\USD\],\subscribe\:true,\depth\:0}});
//            OldSubscriptions.Add({\e\:\subscribe\,\rooms\:[\tickers\]});
//        }

//        protected override string Authentication
//        {
//            get
//            {
//                long time = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds();
//                return string.Format({{\e\:\auth\,\auth\:{{\key\:\{0}\,\signature\:\{1}\,\timestamp\:{2}}}}}, Key, Utils.Hmacsha256(Secret, time + Key), time);
//            }
//        }

//        protected override bool IsHeartbeat(Json json)
//        {
//            return json.E == ping;
//        }
//        protected override bool IsAuthenticated(Json json)
//        {
//            return json.E == auth && json.Ok == ok;
//        }
//        //protected override void Receive(Json json)
//        //{
//        //    Datum datum = json.Data[0];
//        //    switch (json.E)
//        //    {
//        //        case tick:
//        //            if (datum.Symbol1 == Symbol1.Btc && datum.Symbol2 == Symbol2.Usd)
//        //            {
//        //                OnTrade(this, new TradeEventArgs(datum.Price));
//        //            }
//        //            break;
//        //        case md_update:
//        //        case order-book-subscribe:
//        //            foreach (var tuple in datum.Bids)
//        //            {
//        //                OnOrder(this, new OrderBookEventArgs(new OrderBooks.Order()
//        //                {
//        //                    Side = Side.Buy,
//        //                    Id = datum.Id,
//        //                    Price = tuple[0],
//        //                    Quantity = tuple[1]
//        //                }));
//        //            }
//        //            foreach (var tuple in datum.Asks)
//        //            {
//        //                OnOrder(this, new OrderBookEventArgs(new OrderBooks.Order()
//        //                {
//        //                    Side = Side.Sell,
//        //                    Id = datum.Id,
//        //                    Price = tuple[0],
//        //                    Quantity = tuple[1]
//        //                }));
//        //            }
//        //            break;
//        //    }
//        //}
//        public override string PostOrder(IDictionary<string, object> param)
//        {
//            throw new NotImplementedException();
//        }
//        public override string PutOrder(IDictionary<string, object> param)
//        {
//            throw new NotImplementedException();
//        }
//        public override string PostLeverage(IDictionary<string, object> param)
//        {
//            throw new NotImplementedException();
//        }
//        public override string DeleteOrders(IDictionary<string, object> param)
//        {
//            throw new NotImplementedException();
//        }

//        protected override void ConfirmSubscription(Json json)
//        {
//            throw new NotImplementedException();
//        }

//        protected override void FireTableEvents(Json json)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
