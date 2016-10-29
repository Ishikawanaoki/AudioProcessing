using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Math;

namespace WaveEditer
{
    public partial class Form1 : Form
    {
        Viewer vw;
        IEnumerable<double> str;
        int EnumCount = 0;
        public Form1()
        {
            InitializeComponent();
            vw = new Viewer();
            str = Enumerable.Range(0, 1).Select(c=>c*0.1);
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            vw.Plot(chart1,str, "hoge", "hoge", Wave.fs);
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void Encode_button_Click(object sender, EventArgs e)
        {
            int A = 1;
            var data = WaveReAndWr.WavReader("a1.wav");
            A = data.lDataList.Max();

            WaveReAndWr.DefWavWriter("mytest.wav",str.Select(c=>A*c));
        }
        
        private void FFT_button_Click(object sender, EventArgs e)
        {
            // FFTして、振幅スペクトルを配列に格納
            double[] y = Fourier.IEnumerableFourier.FFT(
                Fourier.IEnumerableFourier.Windowing(str, Fourier.WindowFunc.Hamming).
                    Select(c => new Fourier.Complex(c, 0)).ToArray()).
                Select(c => c.magnitude).ToArray();

            int length = y.Count();
            Console.WriteLine("Length = {0}", length);

            // 最大となる振幅スペクトルの大きさと、インデックスを順次一つ格納するタップル（無意味な初期化）
            Tuple<double, int> s = Tuple.Create(0.1,0);
            double max = 0;

            foreach(int i in Enumerable.Range(0, length))
            {
                if(max < y[i])
                {
                    max = y[i];
                    Console.WriteLine("changed {0}=>{1}", s.Item1, max);
                    s = Tuple.Create(max, i);
                }
            }

            label2.Text = ((double)(s.Item2 * length / Wave.fs)).ToString();

            vw.Plot(chart1, y.Where((_,i)=>i<length/2), "hoge", "hoge", Wave.fs);
        }


        private void CountUp_button_Click(object sender, EventArgs e)
        {
            EnumCount++; label1.Text = EnumCount.ToString();
            //str = str.Concat(Wave.SawtoothWave(A, 1));
            //str = str.Concat(Wave.TriangleWave(A, 1));
            //str = str.Concat(Wave.SinWave(A,1));
            str = str.Concat(Wave.QSinWave(1, 1));
        }

        private void Triangle_button_Click(object sender, EventArgs e)
        {
            EnumCount++; label1.Text = EnumCount.ToString();
            str = str.Concat(Wave.TriangleWave(1, 1));
        }

        private void Sawtoot_button_Click(object sender, EventArgs e)
        {
            EnumCount++; label1.Text = EnumCount.ToString();
            str = str.Concat(Wave.SawtoothWave(1, 1));
        }
    }
    public class Viewer
    {
        public class Axis
        {
            public readonly double time;        // 時間軸領域の1目盛り
            public readonly double frequency;   // 周波数軸領域の1目盛り
            public Axis(double sample_value, double sampling_frequency)
            {
                time = 1 / sampling_frequency;
                frequency = sampling_frequency / sample_value;
            }
            public void DoubleAxie(ref double[] x)
            {
                x[0] = frequency;
                for (int i = 1; i < x.Length; i++)
                    x[i] = x[i - 1] + frequency;
            }
            public void StrighAxie(ref string[] x)
            {
                int dimF = (int)frequency;
                int[] x2 = new int[x.Length];
                x2[0] = dimF;
                x[0] = dimF.ToString();
                for (int i = 1; i < x.Length; i++)
                {
                    x2[i] = x2[i - 1] + dimF;
                    x[i] = x2[i].ToString();
                }
            }
            public void intAxis(ref int[] x)
            {
                int dimF = (int)frequency;
                x[0] = dimF;
                for (int i = 1; i < x.Length; i++)
                    x[i] = x[i - 1] + dimF;
            }
        }
        public Axis Plot(Chart chart1,IEnumerable<double> y, string area, string title, int fs)
        {
            string[] xValues = new string[y.Count()];

            Axis plot_axis = new Axis(y.Count(), fs);
            plot_axis.StrighAxie(ref xValues);

            chart1.Titles.Clear();
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();

            chart1.Series.Add(area);
            chart1.ChartAreas.Add(new ChartArea(area));            // ChartArea作成
            chart1.ChartAreas[area].AxisX.Title = "title";  // X軸タイトル設定
            chart1.ChartAreas[area].AxisY.Title = "[/]";  // Y軸タイトル設定

            chart1.Series[area].ChartType = SeriesChartType.Line;

            int count = 0;
            foreach (var str in y)
            {
                DataPoint dp = new DataPoint();
                dp.SetValueXY(xValues[count++], str);  //XとYの値を設定
                dp.IsValueShownAsLabel = false;  //グラフに値を表示しないように指定
                chart1.Series[area].Points.Add(dp);   //グラフにデータ追加
            }

            return plot_axis;
        }
    }
    public static class Wave
    {
        public static double count = 1; // 周期]
        public static int fs = 100;
        /// <summary>
        /// get omega [rad per sec]
        /// return means 2nPI / fs [rad / (sec,sample)]
        /// </summary>
        /// <returns>theta line, contains a number of items , where length times sampling frequency </returns>
        private static IEnumerable<double> GetOneSesond()
        {
            double DivTheta = 2 * PI / fs;
            int num = (int)(fs * count);
            return Enumerable.Range(0, num).Select(c => c * DivTheta);
        }
        public static IEnumerable<double> SinWave(int A, double f0)
        {
            return GetOneSesond().Select(c => A * Sin(c * f0));
        }
        public static IEnumerable<double> QSinWave(int A, double f0)
        {
            int length = SinWave(A, f0).Count();
            return SinWave(A,f0).TakeWhile((val, index) => index < length / 2);
        }
        public static IEnumerable<double> SawtoothWave(int A, double f0)
        {
            return GetOneSesond().Select(c =>{
                return Enumerable.Range(1, 10)
                    .Select(k => A / k * Sin(c * k * f0))
                    .Sum();
            });
        }
        public static IEnumerable<double> filter(IEnumerable<double> str, int A)
        {
            return str.Select(c =>
            {
                if (c > A) return A;
                else if (c < A) return -A;
                else return c;
            });
        }
        public static IEnumerable<double> TriangleWave(int A, double f0)
        {
            return GetOneSesond().Select(c => {
                return Enumerable.Range(0, 10).Select(k => {
                    var m = 2 * k + 1; // m = 1,3,5,7,9,11,...
                    return Pow((-1), k) * (A / Pow(m, 2)) * Sin(c * k);
                }).Sum();
            });
        }
    }
}
