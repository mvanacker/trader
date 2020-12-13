//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Trader.Exchanges
//{
//    public class Hitbtc : Exchange
//    {
//        public Hitbtc()
//        {
//            Name = HitBTC;
//            Key = 3546ddabb12814870d2c93cbbcad6e78;
//            Secret = 6cd36490b822c15527a547e5491a1595;
//            SocketUri = wss://api.hitbtc.com/api/2/ws;
//            OldSubscriptions.Add({\method\:\subscribeTrades\,\params\:{\symbol\:\BTCUSD\}});
//            OldSubscriptions.Add({\method\:\subscribeOrderbook\,\params\:{\symbol\:\BTCUSD\}});
//        }

//        protected override string Authentication
//        {
//            get
//            {
//                string nonce = new Util().Nonce.ToString();
//                return string.Format({{\method\:\login\,\params\:{{\algo\:\HS256\,\pKey\:\{0}\,\nonce\:\{1}\,\signature\:\{2}\}}}}, Key, nonce, Utils.Hmacsha256(Secret, nonce));
//            }
//        }

//        protected override bool IsAuthenticated(Json json)
//        {
//            return json.Result;
//        }
//        //protected override void Receive(Json json)
//        //{
//        //    switch (json.Method)
//        //    {
//        //        case snapshotTrades:
//        //            break;
//        //        case updateTrades:
//        //            foreach (var datum in json.Params.Data)
//        //            {
//        //                OnTrade(this, new TradeEventArgs(datum.Price));
//        //            }
//        //            break;
//        //        case snapshotOrderbook:
//        //        case updateOrderbook:
//        //            foreach (var bid in json.Params.Bid)
//        //            {
//        //                OnOrder(this, new OrderBookEventArgs(new OrderBooks.Order()
//        //                {
//        //                    Side = Side.Buy,
//        //                    Price = double.Parse(bid.Price),
//        //                    Quantity = double.Parse(bid.Size)
//        //                }));
//        //            }
//        //            foreach (var ask in json.Params.Ask)
//        //            {
//        //                OnOrder(this, new OrderBookEventArgs(new OrderBooks.Order()
//        //                {
//        //                    Side = Side.Sell,
//        //                    Price = double.Parse(ask.Price),
//        //                    Quantity = double.Parse(ask.Size)
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
