using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows.Forms;
using System.Threading;

namespace Trader.Exchanges.Gui
{
    using Database;

    public class GenericDataTableTabPage : DataTabPage
    {
        public GenericDataTableTabPage(Symbol symbol, string table, string label) : base(symbol, table, label)
        {
            Query = $"select * from [{Table}]";
            SuspendLayout();
            Grid = new DataGridView
            {
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing
            };
            Controls.Add(Grid);
            ResumeLayout(false);
            Task.Run(ThrottledRenew);
        }

        public DataGridView Grid { get; }
        protected DataTable Data { get; private set; }
        private bool RenewRequested { get; set; }

        public override void Renew() => RenewRequested = true;
        private void ThrottledRenew()
        {
            while (true)
            {
                if (RenewRequested)
                {
                    Invoke((System.Action)delegate
                    {
                        SuspendLayout();
                        Data = Database.Select(Query);
                        Grid.DataSource = Data;
                        ResumeLayout(false);
                    });
                    RenewRequested = false;
                }
                Thread.Sleep(Const.DATA_TAB_RENEW_COOLDOWN);
            }
        }
    }
}
