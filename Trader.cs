using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace Trader
{
    using Exchanges;
    using Exchanges.Gui;
    using Exchanges.Bitmex;
    using Exchanges.Gui.Bitmex;
    using Exchanges.Binance;
    using Exchanges.Gui.Binance;

    internal sealed class Trader : ApplicationContext
    {
        private Trader() => MainForm = ControlPanel;

        public static ControlPanel ControlPanel { get; } = new ControlPanel();

        [STAThread]
        private static void Main(string[] args)
        {
            var program = new Trader();
            program.ConnectExchanges();
            bool finished()
            {
                try
                {
                    Application.Run(program);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    return false;
                }
            }
            while (!finished()) Console.WriteLine("Restarting fully...");
        }

        private void ConnectExchanges()
        {
            var bm = new BitmexTest();
            //var bm = new Bitmex();
            //var bn = new Binance();
            var exchanges = new HashSet<Exchange>
            {
                bm,
                //bn,
            };
            var forms = new Dictionary<Exchange, ExchangeForm>
            {
                [bm] = new BitmexForm(bm),
                //[bn] = new BinanceForm(bn),
            };
            foreach (var exchange in exchanges)
            {
                var exchangeForm = forms[exchange];
                exchangeForm.Show();
                ControlPanel.Add(exchangeForm);
                ControlPanel.FormClosing += (s, e) =>
                {
                    var socket = exchange.Socket;
                    if (socket != null && socket.IsAlive) socket.Close();
                };
                exchange.Socket?.ConnectAsync();
            }
        }
    }
}
