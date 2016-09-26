using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BasicProcessing
{
    public partial class TestFunction : Form
    {
        public TestFunction()
        {
            InitializeComponent();
            //Plot(getWave(100));
            function.ActiveComplex ac = new function.ActiveComplex(getWave(1024));
            ac.FTransform(function.Fourier.ComplexFunc.FFT);
            //ac.FunctionTie();
            double[] data = ac.GetReality().ToArray();
            Plot(getWave(100));

            double max = -1;
            for(int i=0; i<data.Length; i++)
            {
                if (max <= data[i])
                {
                    max = data[i];
                    Console.WriteLine("{0} : {1}", i+1, data[i]);
                }
            }
        }

        private void TestFunction_Load(object sender, EventArgs e)
        {
        }
        private void Plot(double[] y)
        {
            //デフォルトで追加されているSeriesとLegendの初期化
            chart1.Series.Clear();
            chart1.Legends.Clear();

            //Seriesの作成
            Series test = new Series();
            //グラフのタイプを指定(今回は線)
            test.ChartType = SeriesChartType.Line;
            //凡例に表示される文字列を指定
            test.Name = "Wave Form";

            //グラフのデータを追加
            for (int i = 0; i < y.Length; i++)
            {
                test.Points.AddXY(i, y[i]);
            }

            chart1.Series.Add("Area1");
            chart1.ChartAreas.Add(new ChartArea("Area1"));            // ChartArea作成
            chart1.ChartAreas["Area1"].AxisX.Title = "時間 t [s]";  // X軸タイトル設定
            chart1.ChartAreas["Area1"].AxisY.Title = "[/]";  // Y軸タイトル設定
            chart1.Series["Area1"].ChartType = SeriesChartType.Line;

            //凡例の作成と位置情報の指定
            Legend leg = new Legend();
            leg.DockedToChartArea = "Area1";
            leg.Alignment = StringAlignment.Near;
            
            chart1.ChartAreas[0].AxisX.Interval = 0;
            chart1.ChartAreas[0].AxisX.IsStartedFromZero = true;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Crossing = 0;
            chart1.ChartAreas[0].AxisX.Minimum = 0;


            //作ったSeriesをchartコントロールに追加する
            chart1.Series.Add(test);
            chart1.Legends.Add(leg);
        }
        private double[] getWave(int size)
        {
            double[] x = new double[size];
            //double times = 19; double length; double str;

            for(int i=0; i<x.Length; i++)
            {
                //length = Math.Sin(2 * Math.PI * i / x.Length * 2)+1;
                //str = Math.Sin(2 * Math.PI * i / x.Length * (times*10));
                //str += Math.Sin(2 * Math.PI * i / x.Length * (times*50));
                //str += Math.Sin(2 * Math.PI * i / x.Length * (times*100));
                //x[i] = (Math.Sin(2 * Math.PI * i / x.Length * times) * length + str)/4;
                x[i] = Math.Sin(2 * Math.PI * (i / x.Length) * 5);
            }
            return x;
        }
    }
}
