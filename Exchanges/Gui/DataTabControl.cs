using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trader.Exchanges.Gui
{
    public abstract class DataTabControl : TabControl
    {
        public DataTabControl(Exchange exchange, Symbol symbol)
        {
            Exchange = exchange;
            PlugIntoSocket();
            Exchange.Socketed += (s, e) => PlugIntoSocket();
            Symbol = symbol;
            DataTabPages = new DataTabPageCollection(this);
            TabPages = DataTabPages;
            Selected += (s, e) => SelectedDataTab.Renew();
        }

        public Exchange Exchange { get; }
        public Symbol Symbol { get; }
        public DataTabPageCollection DataTabPages { get; }
        public new TabPageCollection TabPages { get; }
        public DataTabPage SelectedDataTab => SelectedTab as DataTabPage;

        private void PlugIntoSocket()
        {
            Exchange.Socket.TableCreated += CreateDataTab;
            Exchange.Socket.TableChanged += UpdateDataTab;
        }
        protected abstract void CreateDataTab(object sender, TableEventArgs e);
        protected abstract void UpdateDataTab(object sender, TableEventArgs e);

        public class DataTabPageCollection : TabPageCollection
        {
            public DataTabPageCollection(TabControl owner) : base(owner)
            {
                Owner = owner;
                Map = new Dictionary<string, DataTabPage>();
            }

            private TabControl Owner { get; }
            public Dictionary<string, DataTabPage> Map { get; }
            public override TabPage this[string label] => Map[label];

            public void Add(DataTabPage value)
            {
                Map[value.Label] = value;
                Owner.Selected += (s, e) =>
                {
                    if (value == Owner.SelectedTab) value.Renew();
                };
                base.Add(value);
            }
            public bool Contains(string label) => Map.ContainsKey(label);
        }
    }
}
