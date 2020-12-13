using System;
using System.Collections.Generic;

namespace Trader
{
    // https://app.quicktype.io/#l=cs&r=json2csharp
    public partial class Json
    {
        public List<Datum> Data { get; set; }
        // bitmex
        //public string Info { get; set; }
        //public string Version { get; set; }
        //public DateTimeOffset? Timestamp { get; set; }
        //public string Docs { get; set; }
        //public Limit Limit { get; set; }
        public bool? Success { get; set; }
        //public string Subscribe { get; set; }
        public string Request { get; set; }
        public string Table { get; set; }
        //public List<object> Keys { get; set; }
        //public Types Types { get; set; }
        //public ForeignKeys ForeignKeys { get; set; }
        //public Attributes Attributes { get; set; }
        public Action? Action { get; set; }
        // cex
        public string E { get; set; }
        public string Ok { get; set; }
        // hitbtc
        public string Jsonrpc { get; set; }
        public bool? Result { get; set; }
        public long? Id { get; set; }
        public string Method { get; set; }
        public Params Params { get; set; }
        // bitmex rest error
        public Error Error { get; set; }
    }

    public partial class Error
    {
        public string Message { get; set; }
        public string Name { get; set; }
    }

    public partial class Attributes
    {
        public string Timestamp { get; set; }
        public Symbol Symbol { get; set; }
    }

    public partial class Datum
    {
        public double Price { get; set; }
        // bitmex
        public DateTimeOffset Timestamp { get; set; }
        public string Symbol { get; set; }
        //public Symbol Symbol { get; set; }
        public Side Side { get; set; }
        public long Size { get; set; }
        public TickDirection TickDirection { get; set; }
        public string TrdMatchId { get; set; }
        public long? GrossValue { get; set; }
        public double? HomeNotional { get; set; }
        public double? ForeignNotional { get; set; }
        // cex
        public Symbol1 Symbol1 { get; set; }
        public Symbol2 Symbol2 { get; set; }
        public string Open24 { get; set; }
        public string Volume { get; set; }
        public string Ok { get; set; }
        public long Id { get; set; }
        public string Pair { get; set; }
        public long? Time { get; set; }
        public List<List<double>> Bids { get; set; }
        public List<List<double>> Asks { get; set; }
        //public long? Timestamp { get; set; }
        public string SellTotal { get; set; }
        public string BuyTotal { get; set; }
        // hitbtc
        public string Quantity { get; set; }
    }

    public partial class ForeignKeys
    {
        public Symbol Symbol { get; set; }
        public Side Side { get; set; }
        public string OrdStatus { get; set; }
    }

    public partial class Limit
    {
        public long Remaining { get; set; }
    }

    public partial class Request
    {
        public string Op { get; set; }
        public List<string> Args { get; set; }
    }

    public partial class Types
    {
        public string Timestamp { get; set; }
        public Symbol Symbol { get; set; }
        public Side Side { get; set; }
        public string Size { get; set; }
        public string Price { get; set; }
        public string TickDirection { get; set; }
        public string TrdMatchId { get; set; }
        public string GrossValue { get; set; }
        public string HomeNotional { get; set; }
        public string ForeignNotional { get; set; }
    }

    public partial class Params
    {
        public List<HitbtcOrder> Ask { get; set; }
        public List<HitbtcOrder> Bid { get; set; }
        public List<Datum> Data { get; set; }
        public Symbol Symbol { get; set; }
        public long Sequence { get; set; }
    }

    public partial class HitbtcOrder
    {
        public string Price { get; set; }
        public string Size { get; set; }
    }

    public enum Action { Insert, Partial, Update, Delete }

    public enum Currency { Usd, XBt }

    public enum TickDirection { MinusTick, PlusTick, ZeroMinusTick, ZeroPlusTick }

    public enum Symbol1 { Bch, Btc, Dash, Eth, Xlm, Xrp, Zec }

    public enum Symbol2 { Btc, Eur, Usd }

    public enum Ok { Ok }
}
