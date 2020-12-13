using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trader.Exchanges.Gui
{
    public class SymbolTabControl : TabControl
    {
        public SymbolTabControl()
        {
            SymbolTabPages = new SymbolTabPageCollection(this);
            TabPages = SymbolTabPages;
        }

        public SymbolTabPageCollection SymbolTabPages { get; }
        public new TabPageCollection TabPages { get; }
        public SymbolTabPage SelectedSymbolTab => SelectedTab as SymbolTabPage;

        public class SymbolTabPageCollection : TabPageCollection
        {
            public SymbolTabPageCollection(TabControl owner) : base(owner)
            {
                Owner = owner;
                Map = new Dictionary<Symbol, SymbolTabPage>();
            }

            private TabControl Owner { get; }
            public Dictionary<Symbol, SymbolTabPage> Map { get; }
            public SymbolTabPage this[Symbol symbol] => Map[symbol];
            public override TabPage this[string symbol] => this[symbol.ToEnum<Symbol>()];

            public void Add(SymbolTabPage value)
            {
                Map[value.Symbol] = value;
                Owner.Selected += (s, e) =>
                {
                    if (value == Owner.SelectedTab)
                    {
                        value.DataTabControl.SelectedDataTab.Renew();
                    }
                };
                base.Add(value);
            }
        }
    }
}
