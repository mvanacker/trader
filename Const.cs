using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader
{
    public static class Const
    {
        // general
        public const int SATOSHI_PER_BITCOIN = 100000000;
        public const double BITCOIN_PER_SATOSHI = 0.00000001;
        public static readonly TimeSpan MILLISECOND = new TimeSpan(10000);
        // control panel
        public const bool CONTROL_PANEL_TOP_MOST = false;
        // gui
        public static readonly TimeSpan DATA_TAB_RENEW_COOLDOWN = new TimeSpan(0, 0, 0, 0, 250);
        // exchange
        public static readonly Dictionary<TradeBin, DateTime> TRADE_BIN_INITIAL_TIMESTAMP = new Dictionary<TradeBin, DateTime>
        {
            [TradeBin.OneMinute] = DateTime.UtcNow - new TimeSpan(1, 0, 0),
            [TradeBin.FiveMinute] = DateTime.UtcNow - new TimeSpan(0, 5 * 7500, 0),
            [TradeBin.OneHour] = DateTime.UtcNow - new TimeSpan(60, 0, 0),
            [TradeBin.OneDay] = DateTime.UtcNow - new TimeSpan(60, 0, 0, 0),
        };
        // exchange socket
        public static readonly TimeSpan RECONNECTION_TIMEOUT = new TimeSpan(0, 0, 3);
        public static readonly TimeSpan GENERAL_SOCKET_TIMEOUT = new TimeSpan(0, 6, 0);

        // bitmex
        public const bool AUTO_RECONNECT = true;
        // bitmex rest
        public const string DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss.fff";
        public static readonly TimeSpan TIME_UNIT = new TimeSpan(0, 0, 0, 0, 1);
        // bitmex gui
        public const int
            RECENT_EXECUTIONS = 9,
            RECENT_TRADES = 10,
            MINIMUM_TRADE_SIZE = 300000, // todo measure volume/time
            ORDER_BOOK_DEPTH = 2;

        // background loop
        public const int
            STANDARD_TIMEOUT = 1000,
            SHORT_TIMEOUT = 100,
            CHANNEL_STRATEGY_TIMEOUT = 2500,
            DIRECTIONAL_STRATEGY_TIMEOUT = 300000;
        // strategy
        public const int
            VOLATILITY_MIN_PRICE_COUNT = 60,
            VOLATILITY_CANDLE_RANGE = 5;
        public const double
            STOP_LOSS_PERCENTAGE = 0.0141875;// * 2 / 3;
        public const string
            PRICE_CHANGE_FORMAT = "+0.00;-0.00";
        // bitmex strategy
        public const int
            BB_PERIOD = 20;
        public const double
            BB_WIDTH = 1.8;
        public static readonly TimeSpan SIZE_CHANGE_COOLDOWN = new TimeSpan(0, 2, 30);

        // price models
        public const bool NORMALIZE_MACD = true;
        // price chart data forms
        public const int
            DIRECTIONAL_FAST_EMA_LENGTH = 288 * 9,
            DIRECTIONAL_SLOW_EMA_LENGTH = 288 * 26,
            MACD_SIGNAL_LENGTH = 9,
            DIRECTIONAL_STOCH_LENGTH = 288 * 7,
            RSI_LENGTH = 14,
            STOCH_RSI_LENGTH = 14,
            STOCH_RSI_D_LENGTH = 6,
            //BB_WIDTH = 2,
            //BB_PERIOD = 20,
            CANDLES_PER_VIEW = 30;
        public const float
            STOCH_RSI_SIGNAL_TOP = 0.8f,
            STOCH_RSI_SIGNAL_BOTTOM = 0.2f;
        // forms
        public const int
            WINDOW_SIZE_X = 775,
            WINDOW_SIZE_Y = 325,
            WINDOW_LOCATION_X = 1920,
            WINDOW_LOCATION_Y = 0;
    }
}
