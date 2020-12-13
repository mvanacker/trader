using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Exchanges.Bitmex
{
    public class BitmexTest : Bitmex
    {
        public BitmexTest()
        {
            foreach (var symbol in Symbols)
            {
                Strategies[symbol].MinimumWallHeight = 9999;
            }
        }

        public override string Name => "BitMEXTest";
        protected override string RestUrl =>"https://testnet.bitmex.com";
        protected override string SocketUrl => "wss://testnet.bitmex.com/realtime";
        //protected override string Key => "mUJ-wPF97d5Z0oM7SOL0JUWY";
        //protected override string Secret => "FeJIg9PBLqCs2I3Qtd8hSxqeorAtj-kzQxzZx4wkP0PN7yIP";
        //protected override string Key => "ImDJHApphSdcL8FreEHP7rjd";
        //protected override string Secret => "XrUIgWLNcN4KYZOLqxBYQWVoD5jG3wDs-ZsnvHbQVPvGgp1a";
        protected override string Key => "VXrWq7aW48lc7LCujd_OfzL_";
        protected override string Secret => "OXsJcnev4t23SbJw2KcfIvQxZ0wtVXfcvJi1Q92REPyYS_zp";
        public override HashSet<Symbol> Symbols => new HashSet<Symbol>
        {
             Symbol.XBTUSD, //Symbol.XBTM18, Symbol.XBTU18, //Symbol.ADAM18, Symbol.BCHM18, Symbol.ETHM18, Symbol.LTCM18, Symbol.XRPM18
        };
    }
}
