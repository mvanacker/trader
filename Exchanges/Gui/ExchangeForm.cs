using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Trader.Exchanges.Gui
{
    public class ExchangeForm : Form
    {
        public ExchangeForm(Exchange exchange)
        {
            Exchange = exchange;
            SuspendLayout();
            Text = exchange.Name;
            ClientSize = new Size(Const.WINDOW_SIZE_X, Const.WINDOW_SIZE_Y);
            HandleCreated += (s, e) => Location = new Point(Const.WINDOW_LOCATION_X, Const.WINDOW_LOCATION_Y);
            SymbolTabControl = new SymbolTabControl
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(SymbolTabControl);
            VisibleChanged += (s, e) =>
            {
                if (Visible)
                {
                    SymbolTabControl.SelectedSymbolTab?.DataTabControl.SelectedDataTab?.Renew();
                }
            };
            ResumeLayout(false);
        }

        public Exchange Exchange { get; }
        protected SymbolTabControl SymbolTabControl { get; }
    }
}
