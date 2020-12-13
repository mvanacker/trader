using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Trader
{
    using Exchanges;

    public class PriceSummaryForm : Form
    {
        public Exchange Exchange { get; set; }
        private TableLayoutPanel Table { get; set; }
        private string[] _columns = { "Symbol", "Bin", "Trend", "Momentum", "MomentumDelta", "Beat", "Momentum", "MomentumDelta" };

        public PriceSummaryForm()
        {
            SuspendLayout();
            Name = "PriceSummary";
            Table = new TableLayoutPanel();
            Table.SuspendLayout();
            Table.Dock = DockStyle.Fill;
            Table.ColumnCount = 8;
            ClientSize = new Size(1000, 400);
            Controls.Add(Table);
            Table.ResumeLayout(false);
            ResumeLayout(false);
        }

        public void Refill()
        {
            SuspendLayout();
            Table.Controls.Clear();
            foreach (var column in _columns)
            {
                Table.Controls.Add(new Label() { Text = column });
            }
            foreach (var symbol in Exchange.Prices.Keys)
            {
                foreach (var bin in Exchange.Prices[symbol].Keys)
                {
                    var model = Exchange.Prices[symbol][bin];
                    var priceCount = model.Map.First().Value.Count;
                    if (priceCount < Const.DIRECTIONAL_SLOW_EMA_LENGTH + Const.MACD_SIGNAL_LENGTH) break;
                    Table.Controls.Add(new Label() { Text = symbol.ToString() });
                    Table.Controls.Add(new Label() { Text = bin.ToLabel() });
                    AddModel(model);
                }
            }
            ResumeLayout(false);
        }
        private void AddModel(Prices model)
        {
            // macd stuff
            var uptrend = model.Macd.Values.Last() > 0;
            AddGenericCell(uptrend, "Uptrend", "Downtrend");
            var positiveMomentum = model.Macd.Momenta.Last() > 0;
            AddGenericCell(positiveMomentum, "Positive", "Negative");
            var increasingMomentum = model.Macd.IsMomentumIncreasing();
            AddGenericCell(increasingMomentum, "Increasing", "Decreasing");
            // srsi stuff
            var upbeat = model.StochRsi.K.Last() > Const.STOCH_RSI_SIGNAL_TOP;
            var downbeat = model.StochRsi.K.Last() < Const.STOCH_RSI_SIGNAL_BOTTOM;
            var beatLabel = new Label()
            {
                Text = upbeat ? "Upbeat" : downbeat ? "Downbeat" : "Interbeat"
            };
            if (upbeat || downbeat)
            {
                beatLabel.BackColor = upbeat ? Color.LightGreen : Color.LightPink;
            }
            Table.Controls.Add(beatLabel);
            var srsiPositiveMomentum = model.StochRsi.Momenta.Last() > 0;
            AddGenericCell(srsiPositiveMomentum, "Positive", "Negative");
            var srsiIncreasingMomentum = model.StochRsi.IsKIncreasing();
            AddGenericCell(srsiIncreasingMomentum, "Increasing", "Decreasing");
        }
        private void AddGenericCell(bool condition, string trueString, string falseString)
        {
            Table.Controls.Add(new Label()
            {
                Text = condition ? trueString : falseString,
                BackColor = condition ? Color.LightGreen : Color.LightPink
            });
        }
    }
}
