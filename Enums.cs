using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader
{
    public enum Symbol
    {
        // bitmex
        XBTUSD, XBTM18, XBTU18, ADAM18, BCHM18, ETHM18, LTCM18, XRPM18,
        // binance
        ETHBTC, EOSBTC, ICXBTC, BNBBTC, TRXBTC, ETCBTC, ADABTC, XLMBTC, LTCBTC, ARNBTC, BCCBTC, XRPBTC, VENBTC, ONTBTC, ZRXBTC, CMTBTC, ZILBTC, SNTBTC, QKCBTC, XMRBTC, WANBTC, NEOBTC, INSBTC, BQXBTC, GVTBTC, GTOBTC, LUNBTC, NAVBTC, XVGBTC, QLCBTC, BRDBTC, EDOBTC, ELFBTC, RCNBTC, ENGBTC, ZECBTC, BCNBTC, SUBBTC, BATBTC, SKYBTC, ZENBTC, OMGBTC, NXSBTC, CVCBTC, GNTBTC, WTCBTC, VIBBTC, POABTC, AGIBTC, PPTBTC, QSPBTC, RDNBTC, BLZBTC, DGDBTC, BTGBTC, XEMBTC, LSKBTC, ARKBTC, MDABTC, CNDBTC, BTSBTC, REQBTC, CDTBTC, ENJBTC, WPRBTC, HSRBTC, KNCBTC, TNTBTC, POEBTC, XZCBTC, GASBTC, LRCBTC, MCOBTC, FUNBTC, GXSBTC, RPXBTC, DNTBTC, TNBBTC, RLCBTC, AMBBTC, ASTBTC, EVXBTC, SNMBTC, SYSBTC, REPBTC, DLTBTC, MTLBTC, OSTBTC, GRSBTC, BNTBTC, KMDBTC, MTHBTC, BCDBTC, OAXBTC, ADXBTC, VIABTC, ICNBTC, MODBTC,
    }

    public enum Index
    {
        // bitmex
        BVOL24H,
    }

    public enum TradeBin
    {
        OneMinute = 1,
        ThreeMinute = 3,
        FiveMinute = 5,
        FifteenMinute = 15,
        ThirtyMinute = 30,
        OneHour = 60,
        TwoHour = 120,
        FourHour = 240,
        SixHour = 360,
        EightHour = 480,
        TwelveHour = 720,
        OneDay = 1440,
        ThreeDay = 4320,
        OneWeek = 10080,
        OneMonth
    }

    public enum Side
    {
        Buy = -1,
        Sell = 1
    }

    public enum OrdType
    {
        Limit, Stop, Market
    }

    public enum OrdStatus
    {
        New, Filled, PartiallyFilled, Canceled
    }

    public enum ExecInst
    {
        Close, LastPrice, ReduceOnly, ParticipateDoNotInitiate
    }

    public enum ExecType
    {
        New, Trade, Funding, Replaced, Canceled, Restated, TriggeredOrActivatedBySystem
    }

    public static class EnumExtensions
    {
        public static T ToEnum<T>(this string s) where T : Enum
        {
            try
            {
                return (T)Enum.Parse(typeof(T), s, true);
            }
            catch (ArgumentException)
            {
                return default;
            }
        }
        public static TradeBin ToTradeBin(this string s)
        {
            switch (s)
            {
                case "tradeBin1m": return TradeBin.OneMinute;
                case "tradeBin5m": return TradeBin.FiveMinute;
                case "tradeBin1h": return TradeBin.OneHour;
                case "tradeBin1d": return TradeBin.OneDay;
                default: throw new InvalidOperationException();
            }
        }
        public static string ToLabel(this TradeBin t)
        {
            switch (t)
            {
                case TradeBin.OneMinute: return "tradeBin1m";
                case TradeBin.FiveMinute: return "tradeBin5m";
                case TradeBin.OneHour: return "tradeBin1h";
                case TradeBin.OneDay: return "tradeBin1d";
                default: throw new InvalidOperationException();
            }
        }
        public static string ToShortLabel(this TradeBin t)
        {
            switch (t)
            {
                case TradeBin.OneMinute: return "1m";
                case TradeBin.ThreeMinute: return "3m";
                case TradeBin.FiveMinute: return "5m";
                case TradeBin.FifteenMinute: return "15m";
                case TradeBin.ThirtyMinute: return "30m";
                case TradeBin.OneHour: return "1h";
                case TradeBin.TwoHour: return "2h";
                case TradeBin.FourHour: return "4h";
                case TradeBin.SixHour: return "6h";
                case TradeBin.EightHour: return "8h";
                case TradeBin.TwelveHour: return "12h";
                case TradeBin.OneDay: return "1d";
                case TradeBin.ThreeDay: return "3d";
                case TradeBin.OneWeek: return "1w";
                case TradeBin.OneMonth: return "1M";
                default: throw new InvalidOperationException();
            }
        }
        public static string ToPastTense(this Side s)
        {
            switch (s)
            {
                case Side.Buy: return "Bought";
                case Side.Sell: return "Sold";
                default: throw new InvalidOperationException();
            }
        }
        public static Side Opposite(this Side s)
        {
            return s == Side.Buy ? Side.Sell : Side.Buy;
        }
        public static string ToIndexString(this Index i)
        {
            return $".{i}";
        }
    }
}
