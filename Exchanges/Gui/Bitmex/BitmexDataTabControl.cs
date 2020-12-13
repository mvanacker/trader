using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Trader.Exchanges.Gui.Bitmex
{
    public class BitmexDataTabControl : DataTabControl
    {
        public BitmexDataTabControl(Exchange exchange, Symbol symbol) : base(exchange, symbol)
        {
        }

        protected override void CreateDataTab(object sender, TableEventArgs e)
        {
            if (!DataTabPages.Contains(e.Label))
            {
                var tab = CreateBitmexDataTabPage(e.Table, e.Label);
                Invoke((System.Action)delegate
                {
                    DataTabPages.Add(tab);
                    if (tab == SelectedTab) tab.Renew();
                });
            }
            else
            {
                var tab = DataTabPages.Map[e.Label];
                BeginInvoke((System.Action)delegate
                {
                    if (tab == SelectedTab) tab.Renew();
                });
            }
        }
        protected override void UpdateDataTab(object sender, TableEventArgs e)
        {
            if (DataTabPages.ContainsKey(e.Label))
            {
                var tab = DataTabPages.Map[e.Label];
                var tradeTab = DataTabPages.Map["trade"];
                BeginInvoke((System.Action)delegate
                {
                    var selected = SelectedTab;
                    if (tab == selected || tab == tradeTab) tab.Renew();
                });
            }
        }
        private DataTabPage CreateBitmexDataTabPage(string table, string label)
        {
            if (label.StartsWith("tradeBin"))
            {
                return new PriceChartTabPage(Symbol, table, label)
                {
                    Prices = Exchange.Prices[Symbol][label.ToTradeBin()]
                };
            }
            else
            {
                var tab = new GenericDataTableTabPage(Symbol, table, label);
                var symbol = Symbol.ToString();
                switch (label)
                {
                    case "order":
                        tab.Query = $"select [Side], [OrdStatus], [Price], [LeavesQty], [CumQty], [OrderQty], [OrdType], [StopPx], [OrderID], [Timestamp] from [{table}] where ([workingIndicator] = 'true' or [ordType] = 'stop') and [symbol] = '{symbol}' order by [timestamp] desc";
                        break;
                    case "margin":
                        tab.Query = $"select [Amount], [RealisedPNL], [WalletBalance], [MaintMargin], [InitMargin], [AvailableMargin], [MarginUsedPcnt], [MarginLeverage] from [{table}]";
                        tab.Grid.DefaultCellStyle = new DataGridViewCellStyle { Format = "#,##0.##" };
                        break;
                    case "position":
                        tab.Query = $"select [CurrentQty], [AvgEntryPrice], [LiquidationPrice], [BankruptPrice], [Leverage] from [{table}] where [symbol] = '{symbol}'";
                        tab.Grid.DefaultCellStyle = new DataGridViewCellStyle { Format = "#,##0.##" };
                        break;
                    case "execution":
                        tab.Query = $"select top {Const.RECENT_EXECUTIONS} [OrdType], [Side], [ExecType], [OrdStatus], [Price], [LeavesQty], [CumQty], [OrderQty], [LastQty], [StopPx], [ExecCost], [ExecComm], [Timestamp], [ExecID], [OrderID] from [{table}] where [execType] <> 'Replaced' and [symbol] = '{symbol}' order by [timestamp] desc";
                        break;
                    case "trade":
                        tab.Query = $"select top {Const.RECENT_TRADES} [Side], [Price], [Size], [Timestamp] from [{table}] where [size] >= {Const.MINIMUM_TRADE_SIZE} order by [timestamp] desc";
                        break;
                    case "orderBookL2":
                        tab.Query = $"select [Side], [Price], [Size] from [{table}] where [side] = 'buy' and [symbol] = '{symbol}' and [price] >= (select top 1 [price] from [{table}] where [side] = 'buy' and [symbol] = '{symbol}' order by [price] desc) - {Const.ORDER_BOOK_DEPTH} union select [Side], [Price], [Size] from [{table}] where [side] = 'sell' and [symbol] = '{symbol}' and [price] <= (select top 1 [price] from [{table}] where [side] = 'sell' and [symbol] = '{symbol}' order by [price] asc) + {Const.ORDER_BOOK_DEPTH} order by [price] desc";
                        break;
                }
                return tab;
            }
        }
    }
}
