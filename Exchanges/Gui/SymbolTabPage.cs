using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trader.Exchanges.Gui
{
    public abstract class SymbolTabPage : TabPage
    {
        public SymbolTabPage(Exchange exchange, Symbol symbol)
        {
            Exchange = exchange;
            Symbol = symbol;
            SuspendLayout();
            Text = Symbol.ToString();
            ResumeLayout(false);
        }

        public Exchange Exchange { get; }
        public Symbol Symbol { get; }
        public DataTabControl DataTabControl { get; protected set; }
    }
}
