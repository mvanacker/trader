using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trader.Exchanges.Gui
{
    public abstract class DataTabPage : TabPage
    {
        public DataTabPage(Symbol symbol, string table, string label)
        {
            Symbol = symbol;
            Table = table;
            Label = label;
            SuspendLayout();
            Text = label.Capitalize();
            ResumeLayout(false);
        }

        public Symbol Symbol { get; set; }
        public string Table { get; set; }
        public string Label { get; set; }
        public string Query { get; set; }

        public abstract void Renew();
    }
}
