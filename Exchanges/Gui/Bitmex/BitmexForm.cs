using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trader.Exchanges.Gui.Bitmex
{
    public class BitmexForm : ExchangeForm
    {
        public BitmexForm(Exchange exchange) : base(exchange)
        {
            SuspendLayout();
            foreach (var symbol in exchange.Symbols)
            {
                var tab = new BitmexSymbolTabPage(exchange, symbol);
                void writeTick(object sender, TickEventArgs e)
                {
                    if (e.Symbol == symbol)
                    {
                        BeginInvoke((System.Action)delegate
                        {
                            tab.Text = $"{symbol} {e.Price}";
                        });
                    }
                }
                void plugTickWriter() => exchange.Socket.Tick += writeTick;
                plugTickWriter();
                exchange.Socketed += (s, e) => plugTickWriter();
                SymbolTabControl.SymbolTabPages.Add(tab);
            }
            ResumeLayout(false);
        }
    }
}
