using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trader.Exchanges.Gui.Bitmex
{
    public class BitmexSymbolTabPage : SymbolTabPage
    {
        public BitmexSymbolTabPage(Exchange exchange, Symbol symbol) : base(exchange, symbol)
        {
            SuspendLayout();
            DataTabControl = new BitmexDataTabControl(exchange, symbol)
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(DataTabControl);
            ResumeLayout(false);
        }
    }
}
