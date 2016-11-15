using function;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BasicProcessing
{
    /// <summary>
    /// 第２番目にあたるウィンドウ、および処理記述
    /// </summary>
    public partial class WaveShow : Form
    {
        private bool chart1_state;
        private bool chart2_state;

        public IEnumerable<double> lDataList;
        public IEnumerable<double> rDataList;
        public WaveReAndWr.WavHeader header;
        private string root = @"..\..\音ファイル";
        public readonly int[] rank;
        public const int divnum = 200;
        public double fru_base;
        public static double fs = 44100.0;
        public static double heldz_base = 0;
        /// <summary>
        /// 引数なしのコンストラクタは無効
        /// （フィールドは空）
        /// </summary>
        public WaveShow()
        {
            InitializeComponent();
            rank = new int[3]{1,2,3};
        }
        /// <summary>
        /// 第１番目のウィンドウから基本呼び出される。
        /// 
        /// クラス内部ではデータ列（右・左）はdouble型にして持ち、
        /// 高精度での数的処理をする、
        /// またWaveへの書き込みには大幅な丸めこみをした値に直す。
        /// </summary>
        /// <param name="lDataList1">short型をコレクションとするList</param>
        /// <param name="rDataList1">short型をコレクションとするList</param>
        /// <param name="header"></param>
        public WaveShow(List<short> lDataList1, List<short> rDataList1, WaveReAndWr.WavHeader header, bool str1, bool str2)
        {
            InitializeComponent();

            chart1_state = str1;
            chart2_state = str2;

            rank = new int[3] { 2, 3, 4 };

            lDataList = lDataList1.Select(c => (double)c);
            rDataList = rDataList1.Select(c => (double)c);

            // ヘッダーエラーは保留とする
            //これによると、不適切なファイルとして再生できないソフトもある。
            // リニアPCMによる要件を纏める必要がある（保留）
            this.header = header;

            // 以下、左側のデータ列を扱う
            //左グラフには時系列、右グラフには高速フーリエ変換結果を出す
            if (chart1_state)
            {
                Plot(chart1, lDataList);
            }
            
            if (chart2_state)
            {
                ActiveComplex acomp = new ActiveComplex(lDataList.ToArray(), function.Fourier.WindowFunc.Hamming);
                acomp.FTransform(function.Fourier.ComplexFunc.FFT);
                var mag = acomp.GetMagnitude();
                int halfL = mag.Count() / 2;


                fru_base = mag.Take(halfL).Skip(1).Max();
                foreach (var item in mag.Take(halfL).Skip(1).Select((c,i) => new { Index = i, Val = c }))
                {
                    if (item.Val >= fru_base)
                    {
                        heldz_base = (item.Index+1)/fs; break;
                    }
                }

                Plot(chart2, mag.Take(halfL));
             }
            Console.WriteLine("Max of Spectrol : {0} :{1}ms", fru_base, heldz_base*1E3 );
        }
        #region conPane
        private void WaveShow_Load(object sender, EventArgs e)
        {

        }
        private void WaveShow_Load_1(object sender, EventArgs e) { }
        /// <summary>
        /// 現在のchar1を保存。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            SaveFile sf = new SaveFile(DoSaveFile);
            sf(1);
        }
        /// <summary>
        /// 現在のchar2を保存。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            SaveFile sf = new SaveFile(DoSaveFile);
            sf(2);
        }
        
        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void chart1_Click(object sender, EventArgs e)
        {

        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="no">chart No.{1 or 2}</param>
        #region 画像の保存
        /// <summary>
        /// グラフの二画面表示を行う。
        /// 初期では左:時間領域、右:周波数領域で表示され、
        /// 第二引数noによってグラフ位置の選択、グラフの更新を行う。
        /// 
        /// </summary>
        /// <param name="y">1列のデータを用いて、グラフ表示します。</param>
        /// <param name="no">2つのchartを分別して、描写対象を決定できる</param>
        private void Plot(Chart str, IEnumerable<double> y)
        {
            if (str == chart1 && !chart1_state) return;
            if (str == chart2 && !chart2_state) return;

            function.File f = new function.File();
            function.Axis plot_axis = f.Plot(str, y.ToArray(), "Area1", "時間 [s]");
            label1.Text = "DTime : " + plot_axis.time.ToString();
            label2.Text = "DFreq : " + plot_axis.frequency.ToString();
        }
        delegate void SaveFile(int no);
        public void DoSaveFile(int no)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PNG Image|*.png|JPeg Image|*.jpg";
            saveFileDialog.InitialDirectory = this.root;
            saveFileDialog.Title = "Save Chart As Image File";
            saveFileDialog.FileName = "Sample.png";

            DialogResult result = saveFileDialog.ShowDialog();
            saveFileDialog.RestoreDirectory = true;

            if (result == DialogResult.OK && saveFileDialog.FileName != "")
            {
                try
                {
                    if (saveFileDialog.CheckPathExists)
                    {
                        switch (no)
                        {
                            case 1:
                                if (saveFileDialog.FilterIndex == 2)
                                {
                                    chart1.SaveImage(saveFileDialog.FileName, ChartImageFormat.Jpeg);
                                }
                                else if (saveFileDialog.FilterIndex == 1)
                                {
                                    chart1.SaveImage(saveFileDialog.FileName, ChartImageFormat.Png);
                                }
                                break;
                            case 2:
                                if (saveFileDialog.FilterIndex == 2)
                                {
                                    chart2.SaveImage(saveFileDialog.FileName, ChartImageFormat.Jpeg);
                                }
                                else if (saveFileDialog.FilterIndex == 1)
                                {
                                    chart2.SaveImage(saveFileDialog.FileName, ChartImageFormat.Png);
                                }
                                break;
                        }

                        saveFileDialog.Dispose();

                    }
                    else
                    {
                        MessageBox.Show("Given Path does not exist");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        #endregion
        private void button5_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。");
            testForHertz();
           // Plot(chart2, DSP.TimeDomain.effector.ACF(lDataList.ToArray()));
            Console.WriteLine("アクションが終了しました。");
        }      
        private void testForHertz()
        {
            string filename = root + @"\testVer1.txt";

            // 短時間フーリエ変換するための格納・実行クラスの生成
            function.ComplexStaff ex
                = new function.ComplexStaff(divnum, lDataList.ToArray());

            ex.fs = 44100;
            ex.mergin = 50;
            //ex.setTimeDistance(20 / 100);

            double[][] heldz = ex.GetHertz(rank);

            //heldz = Pass(heldz);

            using (System.IO.FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                using (System.IO.StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (double[] str in heldz)
                    {
                        int length = str.Length - 1;
                        #region changed 201610111559
                        foreach (double data in str)
                        {
                            //sw.Write(data + ",");
                            sw.Write(data);
                            if (length-- > 0)
                                sw.Write(",");
                            else
                                sw.WriteLine();
                            Console.Write("{0},", data);
                        }
                        //Console.WriteLine("");
                        #endregion

                    }
                }
            }
            Console.WriteLine("配列を保存しました。");
        }
        /// <summary>
        /// 同じ音階を隣り合うdouble[]があった時、後方の値をゼロにする
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private double[][] Pass(IEnumerable<double[]> x)
        {
            double[] tmp = rank.Select(c => (double)c).ToArray();

            var obj = x.Select(c =>
            {
                return c.Select(item =>
                {
                    var reobj = item;
                    foreach (double tmpt in tmp)
                        if (item == tmpt)
                        {
                            reobj = 0; break;
                        }

                    tmp = c.ToArray();
                    return reobj;
                }).ToArray();
            });

            return obj.ToArray();
        }
        private void button4_Click(object sender, EventArgs e)
        {
            double A = lDataList.Select(c => Math.Abs(c)).Max();

        }
        static int cursol = 0;
        static double[] data;
        private function.File f = new function.File();
        void init()
        {
            int len = 1000;
            if (len * (cursol + 1) > lDataList.Count()) cursol = 0;
            data = lDataList.Skip(len * cursol++).Take(len).ToArray();
        }
        private double[] tourWave(double[] x)
        {
            double[] differ = Enumerable.Range(0, x.Length - 1)
                .Select(c => x[c] - x[c + 1]).ToArray();

            return differ;
        }
        private double[] initFFT()
        {
            ActiveComplex ac = new ActiveComplex(data, function.Fourier.WindowFunc.Hanning);
            ac.FTransform(function.Fourier.ComplexFunc.FFT);

            var mag = ac.GetMagnitude();
            double max = mag.Max();

            double[] ans =  mag.Take(mag.Count() / 2)
                .Skip(1)
                .Select(c => c/max)
                .TakeWhile(c => c>0.01)
                .ToArray();
            f.APlot(
                chart2,ans,
                "周波数", "fft");

            return ans;
        }
        private void initCepstrum()
        {
            ActiveComplex ac = new ActiveComplex(data, function.Fourier.WindowFunc.Hamming);
            ac.FTransform(function.Fourier.ComplexFunc.FFT);

            var mag = ac.GetMagnitude();
            var mag2 = ac.GetMagnitude().Select(c => Math.Sqrt(c));
            double max = mag.Max();

            double[] ans = mag.Take(mag.Count() / 2)
                .Select(c => c / max)
                .ToArray();

            ac = new ActiveComplex(ans, function.Fourier.WindowFunc.Hamming);
            ac.FTransform(function.Fourier.ComplexFunc.FFT);

            mag = ac.GetMagnitude();
            max = mag.Max();

            ans = mag.Take(mag.Count() / 2)
                .Skip(1)
                .Select(c => c / max)
                .TakeWhile(c => c > 0.01)
                .ToArray();
            f.APlot(
                chart1, ans,
                "周波数", "AmpCep");


            double[] ans2 = mag2.Take(mag2.Count() / 2)
                .Select(c => c / max)
                .ToArray();

            ac = new ActiveComplex(ans2, function.Fourier.WindowFunc.Hamming);
            ac.FTransform(function.Fourier.ComplexFunc.FFT);

            mag2 = ac.GetMagnitude();
            max = mag2.Max();

            ans = mag.Take(mag.Count() / 2)
                .Skip(1)
                .Select(c => c / max)
                .ToArray();
            f.APlot(
                chart2, ans,
                "周波数", "PowCep");
        }
        private void ACF_button_Click(object sender, EventArgs e)
        {
            init();
            /*
            initFFT();
            f.APlot(
                chart1,
                DSP.TimeDomain.effector.M_ACF(1, data).ToArray(),
                "時間", "acf");
                */

            double[] mag = initFFT();
            double[] dif = tourWave(mag);
            f.APlot(
                chart1, mag,
                "周波数", "PowCep");
            f.APlot(
                chart2, dif,
                "周波数", "PowCep");

            label2.Text = "sampleNo :" + cursol.ToString();
        }

        private void SDF_button_Click(object sender, EventArgs e)
        {
            init();
            initCepstrum();
            label2.Text = "sampleNo :" + cursol.ToString();

        }

        private void NSDF_button_Click(object sender, EventArgs e)
        {
            init(); initFFT();
            f.APlot(
                chart1,
                DSP.TimeDomain.effector.M_NSDF(1, data).ToArray(),
                "時間","nsdf");
            label2.Text = "sampleNo :" + cursol.ToString();

        }
        private void button1_Click(object sender, EventArgs e)
        {
        }
        private void test_button_Click(object sender, EventArgs e)
        {
        }
        private void BinaryConvert(double[] x, string argc)
        {
            int count = x.Length;
            double myu_T = x.Sum() / count;
            double max_val = 0; double max_threshold = 1;

            var classA = Enumerable.Range(0, count)
                .Select(c => x.Take(c).ToArray());
            var classB = Enumerable.Range(0, count)
                .Select(c => x.Skip(c).ToArray());

            foreach(int i in Enumerable.Range(0, count))
            {
                double[] c1 = classA.ElementAt(i);
                double[] c2 = classB.ElementAt(i);
                int n1 = c1.Count();
                int n2 = c2.Count();
                double myu1 = c1.Sum() / n1;
                double myu2 = c2.Sum() / n2;
                double sigma1 = c1.Select(c => c * c).Sum() / n1;
                double sigma2 = c2.Select(c => c * c).Sum() / n2;
                double sigma_w = (n1 * sigma1 + n2 + sigma2) / (n1 + n2);
                double sigma_B = (n1 * Math.Pow(myu1 - myu_T,2) + n2 * Math.Pow(myu2 - myu_T,2)) / (n1 + n2);

                if(max_val < (sigma_B / sigma_w))
                {
                    max_val = sigma_B / sigma_w;
                    max_threshold = i;
                }
            }
            Console.WriteLine("{0}:閾値:{1}より周波数軸を2分できる", argc, max_threshold);
        }

        private void FFT_button_Click(object sender, EventArgs e)
        {

            init();
            
            ActiveComplex ac = new ActiveComplex(data, function.Fourier.WindowFunc.Hanning);
            ac.FTransform(function.Fourier.ComplexFunc.FFT);

            var mag = ac.GetMagnitude();
            double max = mag.Max();

            double[] ans = mag.Select(c => c / max).ToArray();
            f.APlot(
                chart2, ans,
                "周波数", "fft");

            ac = new ActiveComplex(data, function.Fourier.WindowFunc.Hamming);
            ac.MusucalTransform();

            mag = ac.GetMagnitude();
            max = mag.Max();

            ans = mag.Select(c => c / max).ToArray();
            f.APlot(
                chart1, ans,
                "周波数", "Mfft");



            label2.Text = "sampleNo :" + cursol.ToString();
        }
    }
}