using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Trader.Exchanges.Gui
{
    public class PriceChartTabPage : DataTabPage
    {
        private TableLayoutPanel Panel { get; set; }

        public PriceChartTabPage(Symbol symbol, string table, string label) : base(symbol, table, label)
        {
            SuspendLayout();
            Panel = new TableLayoutPanel();
            Panel.SuspendLayout();
            Panel.Dock = DockStyle.Fill;
            //Panel.ColumnCount = 2;
            //Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 80f));
            //Panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20f));
            Controls.Add(Panel);
            InitializeCandleChart();
            InitializeMacdChart();
            InitializeStochRsiChart();
            Panel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #region General
        private int _x = 0;
        public override void Renew() { }
        private Prices _prices;
        public Prices Prices
        {
            get => _prices; set
            {
                _prices = value;
                HandleCreated += (s, e) =>
                {
                    var map = _prices.Map;
                    for (int i = 0; i < map.First().Value.Count; i++)
                    {
                        UpdateCharts(new double[]
                        {
                            map[Price.High][i],
                            map[Price.Low][i],
                            map[Price.Open][i],
                            map[Price.Close][i]
                        });
                    }
                };
                _prices.Updated += (s, e) => UpdateCharts(e.Hloc);
            }
        }
        private void UpdateCharts(double[] hloc)
        {
            Invoke((System.Action)delegate
            {
                SuspendLayout();
                CandleSeries.Points.AddXY(_x, hloc[0], hloc[1], hloc[2], hloc[3]);
                // todo don't base these updates on last value (or only by default)
                UpdateCandleSeries();
                UpdateMacdSeries();
                UpdateStochRsiChart();
                if (_x >= Const.CANDLES_PER_VIEW)
                {
                    CandleChartArea.AxisX.ScaleView.Position++;
                    MacdChartArea.AxisX.ScaleView.Position++;
                    StochRsiChartArea.AxisX.ScaleView.Position++;
                }
                ResumeLayout(false);
                _x++;
            });
        }
        #endregion

        #region Candles
        private ChartArea CandleChartArea { get; set; }
        private Series CandleSeries { get; set; }
        private Series FastEmaSeries { get; set; }
        private Series SlowEmaSeries { get; set; }
        private Series UpperBbSeries { get; set; }
        private Series LowerBbSeries { get; set; }
        //private Series UpperKcSeries { get; set; }
        //private Series LowerKcSeries { get; set; }
        //private Series UpperMcSeries { get; set; }
        //private Series LowerMcSeries { get; set; }
        private Chart CandleChart { get; set; }
        private void UpdateCandleSeries()
        {
            if (_x >= Const.DIRECTIONAL_FAST_EMA_LENGTH)
            {
                double fastEma = Prices.FastEma.Values[_x - Const.DIRECTIONAL_FAST_EMA_LENGTH];
                FastEmaSeries.Points.AddXY(_x, fastEma);
                if (_x >= Const.DIRECTIONAL_SLOW_EMA_LENGTH)
                {
                    double slowEma = Prices.SlowEma.Values[_x - Const.DIRECTIONAL_SLOW_EMA_LENGTH];
                    SlowEmaSeries.Points.AddXY(_x, slowEma);
                }
            }
            if (_x >= Const.BB_PERIOD)
            {
                double upperBand = Prices.Bb.UpperBand[_x - Const.BB_PERIOD];
                UpperBbSeries.Points.AddXY(_x, upperBand);
                double lowerBand = Prices.Bb.LowerBand[_x - Const.BB_PERIOD];
                LowerBbSeries.Points.AddXY(_x, lowerBand);
            }
            //if (_x >= Const.BB_PERIOD)
            //{
            //    double upperBand = Prices.Kc.UpperBand[_x - Const.BB_PERIOD];
            //    UpperKcSeries.Points.AddXY(_x, upperBand);
            //    double lowerBand = Prices.Kc.LowerBand[_x - Const.BB_PERIOD];
            //    LowerKcSeries.Points.AddXY(_x, lowerBand);
            //}
            //if (_x >= Const.BB_PERIOD)
            //{
            //    double upperBand = Prices.Mc.UpperBand[_x - Const.BB_PERIOD];
            //    UpperMcSeries.Points.AddXY(_x, upperBand);
            //    double lowerBand = Prices.Mc.LowerBand[_x - Const.BB_PERIOD];
            //    LowerMcSeries.Points.AddXY(_x, lowerBand);
            //}
        }
        private void InitializeCandleChart()
        {
            CandleChartArea = CreateChartArea("CandleChartArea");
            CandleChartArea.AxisY.IsStartedFromZero = false;

            CandleChart = CreateChart(CandleChartArea);
            Panel.Controls.Add(CandleChart);

            CandleSeries = CreateSeries("CandleChartArea");
            CandleSeries.ChartType = SeriesChartType.Candlestick;
            CandleSeries.YValuesPerPoint = 4;
            CandleChart.Series.Add(CandleSeries);

            FastEmaSeries = CreateSeries("CandleChartArea");
            FastEmaSeries.ChartType = SeriesChartType.Line;
            CandleChart.Series.Add(FastEmaSeries);

            SlowEmaSeries = CreateSeries("CandleChartArea");
            SlowEmaSeries.ChartType = SeriesChartType.Line;
            CandleChart.Series.Add(SlowEmaSeries);

            UpperBbSeries = CreateSeries("CandleChartArea");
            UpperBbSeries.ChartType = SeriesChartType.Line;
            UpperBbSeries.Color = Color.Black;
            CandleChart.Series.Add(UpperBbSeries);

            LowerBbSeries = CreateSeries("CandleChartArea");
            LowerBbSeries.ChartType = SeriesChartType.Line;
            LowerBbSeries.Color = Color.Black;
            CandleChart.Series.Add(LowerBbSeries);

            //UpperKcSeries = CreateSeries("CandleChartArea");
            //UpperKcSeries.ChartType = SeriesChartType.Line;
            //UpperKcSeries.Color = Color.Red;
            //CandleChart.Series.Add(UpperKcSeries);

            //LowerKcSeries = CreateSeries("CandleChartArea");
            //LowerKcSeries.ChartType = SeriesChartType.Line;
            //LowerKcSeries.Color = Color.Red;
            //CandleChart.Series.Add(LowerKcSeries);

            //UpperMcSeries = CreateSeries("CandleChartArea");
            //UpperMcSeries.ChartType = SeriesChartType.Line;
            //UpperMcSeries.Color = Color.Red;
            //CandleChart.Series.Add(UpperMcSeries);

            //LowerMcSeries = CreateSeries("CandleChartArea");
            //LowerMcSeries.ChartType = SeriesChartType.Line;
            //LowerMcSeries.Color = Color.Red;
            //CandleChart.Series.Add(LowerMcSeries);
        }
        #endregion

        #region Macd
        private ChartArea MacdChartArea { get; set; }
        private Series MacdSeries { get; set; }
        private Series MacdSignalSeries { get; set; }
        private Series MacdHistogram { get; set; }
        private Chart MacdChart { get; set; }
        private void UpdateMacdSeries()
        {
            if (_x >= Const.DIRECTIONAL_SLOW_EMA_LENGTH)
            {
                double macd = Prices.Macd.Values[_x - Const.DIRECTIONAL_SLOW_EMA_LENGTH];
                MacdSeries.Points.AddXY(_x, macd);
                if (_x >= Const.DIRECTIONAL_SLOW_EMA_LENGTH + Const.MACD_SIGNAL_LENGTH)
                {
                    double macdSignal = Prices.Macd.Signal[_x - Const.DIRECTIONAL_SLOW_EMA_LENGTH - Const.MACD_SIGNAL_LENGTH];
                    MacdSignalSeries.Points.AddXY(_x, macdSignal);
                    double momentum = Prices.Macd.Momenta[_x - Const.DIRECTIONAL_SLOW_EMA_LENGTH - Const.MACD_SIGNAL_LENGTH];
                    MacdHistogram.Points.AddXY(_x, momentum);
                }
            }
        }
        private void InitializeMacdChart()
        {
            MacdChartArea = CreateChartArea("MacdChartArea");
            MacdChart = CreateChart(MacdChartArea);
            MacdSeries = CreateSeries("MacdChartArea");
            MacdSeries.ChartType = SeriesChartType.Line;
            MacdChart.Series.Add(MacdSeries);
            MacdSignalSeries = CreateSeries("MacdChartArea");
            MacdSignalSeries.ChartType = SeriesChartType.Line;
            MacdChart.Series.Add(MacdSignalSeries);
            MacdHistogram = CreateSeries("MacdChartArea");
            MacdHistogram.ChartType = SeriesChartType.Column;
            MacdChart.Series.Add(MacdHistogram);
            Panel.Controls.Add(MacdChart);
        }
        #endregion

        #region StochRsi
        private ChartArea StochRsiChartArea { get; set; }
        private Series StochRsiKSeries { get; set; }
        private Series StochRsiDSeries { get; set; }
        private Series StochRsiHistogram { get; set; }
        private Chart StochRsiChart { get; set; }
        private void UpdateStochRsiChart()
        {
            int min = Const.RSI_LENGTH + Const.STOCH_RSI_LENGTH + 1;
            if (_x >= min)
            {
                //double rsiK = PriceModel.Rsi.Values[_x-1];
                double rsiK = Prices.StochRsi.K[_x - min];
                StochRsiKSeries.Points.AddXY(_x, rsiK);
                if (_x >= min + Const.STOCH_RSI_D_LENGTH)
                {
                    double rsiD = Prices.StochRsi.D[_x - min - Const.STOCH_RSI_D_LENGTH];
                    StochRsiDSeries.Points.AddXY(_x, rsiD);
                    double momentum = Prices.StochRsi.Momenta[_x - min - Const.STOCH_RSI_D_LENGTH];
                    StochRsiHistogram.Points.AddXY(_x, momentum);
                }
            }
        }
        private void InitializeStochRsiChart()
        {
            StochRsiChartArea = CreateChartArea("StochRsiChartArea");
            StochRsiChartArea.AxisY.Minimum = -1;
            StochRsiChartArea.AxisY.Maximum = 1;
            StochRsiChart = CreateChart(StochRsiChartArea);
            StochRsiKSeries = CreateSeries("StochRsiChartArea");
            StochRsiKSeries.ChartType = SeriesChartType.Line;
            StochRsiChart.Series.Add(StochRsiKSeries);
            StochRsiDSeries = CreateSeries("StochRsiChartArea");
            StochRsiDSeries.ChartType = SeriesChartType.Line;
            StochRsiChart.Series.Add(StochRsiDSeries);
            StochRsiHistogram = CreateSeries("StochRsiChartArea");
            StochRsiHistogram.ChartType = SeriesChartType.Column;
            StochRsiChart.Series.Add(StochRsiHistogram);
            Panel.Controls.Add(StochRsiChart);
        }
        #endregion

        #region Factory
        //private TextBox CreateAnalysisTextBox()
        //{
        //    return new TextBox()
        //    {
        //        Dock = DockStyle.Fill,
        //        Multiline = true,
        //        ReadOnly = true
        //    };
        //}
        private ChartArea CreateChartArea(string name)
        {
            var chartArea = new ChartArea()
            {
                Name = name
            };
            chartArea.AxisX.MajorGrid.Enabled = false;
            chartArea.AxisX.ScrollBar.Enabled = true;
            chartArea.AxisX.ScrollBar.ButtonStyle = ScrollBarButtonStyles.SmallScroll;
            chartArea.AxisY.MajorGrid.Enabled = false;
            chartArea.AxisX.ScaleView.Size = Const.CANDLES_PER_VIEW;
            chartArea.AxisX.ScaleView.Position = 0;
            //chartArea.AxisX.ScrollBar.ButtonColor = Color.Cyan;
            //chartArea.AxisX.ScrollBar.BackColor = Color.White;
            //chartArea.AxisX.ScrollBar.LineColor = Color.White;
            return chartArea;
        }
        private Series CreateSeries(string chartAreaName)
        {
            return new Series
            {
                ChartArea = chartAreaName
            };
        }
        private Chart CreateChart(ChartArea chartArea)
        {
            var chart = new Chart();
            chart.BeginInit();
            chart.ChartAreas.Add(chartArea);
            chart.Anchor = AnchorStyles.Top;
            chart.Dock = DockStyle.Fill;
            chart.Location = new Point(0, 0);
            chart.EndInit();
            return chart;
        }
        #endregion
    }
}
