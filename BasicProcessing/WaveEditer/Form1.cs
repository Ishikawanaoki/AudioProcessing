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

            // プロト実行
            //Eigenvalue.CS1101.Exacute();
            /*
            var ar= Enumerable.Range(0, 4).Select(pivot =>{return Enumerable.Range(pivot + 1, 4 - pivot).ToArray();}).ToArray();

            foreach (var item in ar)
            {foreach (var item1 in item)
                {Console.Write(string.Format("{0,14:F10}\t", item1) + " ");}
                Console.WriteLine();
            }
            */
        }

        private void start_button_Click(object sender, EventArgs e)
        {
            if(EnumCount==0)
                testShow();
            else
                vw.Plot(chart1,str.Skip(1), "hoge", "hoge", Wave.fs);

            
        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        void testShow()
        {
            int length = 2048;
            var x1 //= str.Skip(1).ToArray();
                = Enumerable.Range(0, length).Select(c => 30.0);

            var x2 //= Fourier.IEnumerableFourier.GasussianWindow(length);
                 = Fourier.IEnumerableFourier.Window(length, Fourier.WindowFunc.Blackman);

            var Outsignal = Fourier.IEnumerableFourier.Product(
                x1,x2
                );

            vw.Plot(chart1, x2.Take(x2.Count()), "hoge", "hoge", Wave.fs);
        }
        private void Encode_button_Click(object sender, EventArgs e)
        {
            //var tmp 
            //= Wave.Serialization(Enumerable.Range(0, 10).Select((_,c) => getOnesMNote(c, 0.01)));
            //= Kaeru(30, 3);
            //= getOnesMNote(3, 0.5);
            //vw.Plot(chart1, tmp, "hoge", "hoge", Wave.fs);
            //Console.WriteLine("Count ={0}\n Continue", tmp.Count());

            //var data = WaveReAndWr.WavReader("a1.wav");
            //int A = data.lDataList.Max();
            //Console.WriteLine(A.ToString());
            //WaveReAndWr.DefWavWriter("mytest_t.wav",tmp.Select(c=>A*c).ToArray());


            var tmp = Kaeru(30, 3);

            WaveReAndWr.DefWavWriter("mytest_t.wav", tmp.Select(c => (short.MaxValue / 2) * c).ToArray());


            Console.WriteLine("ファイル書き込み終了!!");
        }
        private IEnumerable<double> Kaeru(int T, int select)
        {
            int[] mNote = 
            {
                0,1,2,3,2,1,0,-1,
                2,3,4,5,4,3,2,-1,
                0,0,0,0,-1,
                0,2,3,2,1,0
            };

            var dimention =
                mNote.Select(Note => getOnesMNote(Note, T / mNote.Length, select));

            return Wave.Serialization(dimention);
        }
        private IEnumerable<ushort> ConvertDoubleToUInt16(IEnumerable<double> x)
        {
            return x.Select(c =>
            {
                short stmp = 0; double dtmp = c;
                if (dtmp > short.MaxValue / 2) stmp = short.MaxValue / 2 - 1;
                else stmp = Convert.ToInt16(dtmp);

                return BitConverter.ToUInt16(BitConverter.GetBytes(stmp), 0); ;
            });
        }
        private IEnumerable<double> ChangeRange(IEnumerable<double> x)
        {
            double max = x.Max();
            double min = x.Min(); min = min < 0 ? min : 0;
            return x.Select(c => c / (max + min) * (short.MaxValue / 2));
        }
        private IEnumerable<double> getOnesMNote(int A0, double sec, int select)
        {
            Wave.fs = 44100; //以降の処理では、サンプルレート44100に変更
            double fr = 27.5 * Math.Pow(2, A0 / 12);
            var ans = Enumerable.Range(0, 0).Select(c => 0.0);

            if (A0 > 0)
            {
                switch (select) {
                    case 1:
                        ans = Wave.GetOneNote(1, fr, sec);
                        break;
                    case 2:
                        ans = Wave.GetOneNoteS(1, fr, sec);
                        break;
                    case 3:
                        ans = Wave.GetOneNoteT(1, fr, sec);
                        break;
                }
            }
            else
            {
                switch (select)
                {
                    case 1:
                        ans = Wave.GetOneNote(0, 1, sec);
                        break;
                    case 2:
                        ans = Wave.GetOneNoteS(0, 1, sec);
                        break;
                    case 3:
                        ans = Wave.GetOneNoteT(0, 1, sec);
                        break;
                }
            }

            //Console.WriteLine("A0={0}, count={1}, sec={2}", A0, ans.Count(), sec);
            return ans;
        }
        private IEnumerable<double> FFT(IEnumerable<double> x)
        {
            return Fourier.IEnumerableFourier.FFT(
                // 次をフーリエ変換
                    
                Fourier.IEnumerableFourier.Windowing(x, Fourier.WindowFunc.Hamming).
                // 窓関数の適応

                        Select(c => new Fourier.Complex(c, 0)).ToArray()).
                // 複素数列挙型へ変換

                Select(c => c.magnitude);
                // 振幅スぺクトルへ変換
        }
        private void FFT_button_Click(object sender, EventArgs e)
        {
            // FFTして、振幅スペクトルを配列に格納
            double[] y = FFT(str.Skip(1)).ToArray();

            int length = y.Count();

            // 最大となる振幅スペクトルの大きさと、インデックスを順次一つ格納するタップル（無意味な初期化）
            Tuple<double, int> s = Tuple.Create(0.1,0);
            double max = 0;

            foreach(int i in Enumerable.Range(0, length))
            {
                if(max < y[i])
                {
                    max = y[i];
                    s = Tuple.Create(max, i);
                }
            }

            label2.Text = ((double)(s.Item2 * length / Wave.fs)).ToString();

            vw.Plot(chart1, ClipWave(y), "hoge", "hoge", Wave.fs);
            //ClipWave(y);
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
        private IEnumerable<double> ClipWave(IEnumerable<double> x)
        {
            int maxI = 0;
            double maxV = x.Max();

            var a = x.Take(x.Count() / 2)
                .Select((v, i) => new { Value = v, Index = i })
                .Reverse()
                .Where(c => c.Value > 1) // 無意味な情報を弾く条件
                .Select(c =>
                {
                    if (c.Index > maxI) maxI = c.Index;
                    return c;
                })
                .Reverse();

            int cursol = 0;

            foreach (var sys in a)
                Console.WriteLine("{0}:{1}", sys.Index, sys.Value);

            return Enumerable.Range(0, maxI).Select(c =>
             {
                 if (c == a.ElementAt(cursol).Index)
                     return a.ElementAt(cursol++).Value;
                 else
                     return 0.0;
             }
             )
             .Select(c=> { Console.WriteLine(c); return c; });
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
    
}
