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

namespace BasicProcessing
{
    public partial class WaveShow : Form
    {
        List<short> lDataList;
        List<short> rDataList;
        public WaveShow()
        {
            InitializeComponent();
        }

        public WaveShow(List<short> left, List<short> right)
        {
            InitializeComponent();
            this.lDataList = left;
            this.rDataList = right;

            Plot();
            test();
        }

        private void WaveShow_Load(object sender, EventArgs e)
        {

        }
        private void Plot()
        {
            String gname = "sample";

            chart1.Series.Clear();  //グラフ初期化
            chart1.Series.Add(gname);
            int[] xValues = new int[lDataList.Count];
            chart1.Series[gname].ChartType = SeriesChartType.Line;

            for (int i = 0; i < xValues.Length; i++)
            {
                //グラフに追加するデータクラスを生成
                DataPoint dp = new DataPoint();
                dp.SetValueXY(xValues[i], lDataList[i]);  //XとYの値を設定
                dp.IsValueShownAsLabel = false;  //グラフに値を表示するように指定
                chart1.Series[gname].Points.Add(dp);   //グラフにデータ追加
            }
        }
        /// <summary>
        /// DFTを実行し、そしてグラフ描写をするテストクラスです
        /// <para name = "sign">時間信号の複素数列</para>
        /// <para name = "do_dft">結果である複素数列</para>
        /// </summary>
        private void test() //本体
                            ///
        {
            Complex[] sign = new Complex[lDataList.Count];
            Complex[] do_dft = new Complex[lDataList.Count];
            //Complex[] do_fft = new Complex[Nmax];
            double seikika = 0;
            double[] y_out = new double[lDataList.Count];


            for (int i = 0; i < lDataList.Count; i++)
            {
                sign[i] = new Complex((double)lDataList[i], 0);
            }


            do_dft = Fourier.DFT(sign);

            for (int ii = 0; ii < lDataList.Count; ii++)
            {
                y_out[ii] = do_dft[ii].magnitude;
                y_out[ii] = Math.Log10(y_out[ii]) * 10;
                if (seikika < y_out[ii]) seikika = y_out[ii];
            }


            for (int iii = 0; iii < lDataList.Count; iii++)
                y_out[iii] = y_out[iii] / seikika * 100;

            String gname = "sample";

            chart2.Series.Clear();  //グラフ初期化
            chart2.Series.Add(gname);
            int[] xValues = new int[lDataList.Count];
            chart2.Series[gname].ChartType = SeriesChartType.Line;

            for (int i = 0; i < xValues.Length; i++)
            {
                //グラフに追加するデータクラスを生成
                DataPoint dp = new DataPoint();
                dp.SetValueXY(xValues[i], y_out[i]);  //XとYの値を設定
                dp.IsValueShownAsLabel = false;  //グラフに値を表示するように指定
                chart2.Series[gname].Points.Add(dp);   //グラフにデータ追加
            }
        }
        /// <summary>
        /// このフォームに紐づけされた2つの内部クラスの1つです。
        /// このクラスでは、DFTを行うために必要な複素数表現をする構造体と、その処理メソッドです。
        /// このクラスは以前プロジェクトの転用であり、現時点で必要のないメソッドを除いています。
        /// </summary>
        internal class Complex
        {
            public double real = 0.0;
            public double imag = 0.0;
            /// <summary>
            /// フィールドです。
            /// </summary>
            /// <param name="real">実部です。</param>
            /// <param name="img">虚部です。</param>
            public Complex(double real, double img)
            {
                this.real = real;
                this.imag = img;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="r"></param>
            /// <param name="radians"></param>
            /// <returns></returns>
            public static Complex from_polar(double r, double radians)
            {
                Complex data = new Complex(r * Math.Cos(radians), r * Math.Sin(radians));
                return data;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Complex operator +(Complex a, Complex b)
            {
                Complex data = new Complex(a.real + b.real, a.imag + b.imag);
                return data;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static Complex operator *(Complex a, Complex b)
            {
                Complex data = new Complex((a.real * b.real) - (a.imag * b.imag),
               (a.real * b.imag + (a.imag * b.real)));
                return data;
            }
            /// <summary>
            /// 
            /// </summary>
            public double magnitude
            {
                get
                {
                    return Math.Sqrt(Math.Pow(real, 2) + Math.Pow(imag, 2));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        internal class Fourier
        {

            /// <summary>
            /// 
            /// </summary>
            public enum WindowFunc
            {
                Hamming,
                Hanning,
                Blackman,
                Rectangular
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="x"></param>
            /// <returns></returns>
            public static Complex[] HannWindow(Complex[] x)
            {
                int N = x.Length;

                return x; //Disenable
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="data"></param>
            /// <param name="windowFunc"></param>
            /// <returns></returns>
            public static double[] Windowing(double[] data, WindowFunc windowFunc)
            {
                int size = data.Length;
                double[] windata = new double[size];

                for (int i = 0; i < size; i++)
                {
                    double winValue = 0;
                    // 各々の窓関数
                    if (WindowFunc.Hamming == windowFunc)
                    {
                        winValue = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (size - 1));
                    }
                    else if (WindowFunc.Hanning == windowFunc)
                    {
                        winValue = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (size - 1));
                    }
                    else if (WindowFunc.Blackman == windowFunc)
                    {
                        winValue = 0.42 - 0.5 * Math.Cos(2 * Math.PI * i / (size - 1))
                                        + 0.08 * Math.Cos(4 * Math.PI * i / (size - 1));
                    }
                    else if (WindowFunc.Rectangular == windowFunc)
                    {
                        winValue = 1.0;
                    }
                    else
                    {
                        winValue = 1.0;
                    }
                    // 窓関数を掛け算
                    windata[i] = data[i] * winValue;
                }
                return windata;
            }


            /// <summary>
            /// 
            /// </summary>
            /// <param name="x"></param>
            /// <returns></returns>
            public static Complex[] DFT(Complex[] x)
            {
                int N = x.Length;
                Complex[] X = new Complex[N];
                double d_theta = (-2) * Math.PI / N;
                for (int k = 0; k < N; k++)
                {
                    X[k] = new Complex(0, 0);
                    for (int n = 0; n < N; n++)
                    {
                        // Complex temp = Complex.from_polar(1, -2 * Math.PI * n * k / N);
                        Complex temp = Complex.from_polar(1, d_theta * n * k);
                        temp *= x[n]; //演算子 * はオーバーライドしたもの
                        X[k] += temp; //演算子 + はオーバーライドしたもの
                    }
                }
                return X;
            }
        }
    }
}