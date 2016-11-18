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
        public static double sec;
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
            sec = fs / divnum;
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

            sec = lDataList.Count() / fs / divnum;

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
        private void testForHertzOnlyNote()
        {
            string filename = root + @"\testVer1.txt";

            // 短時間フーリエ変換するための格納・実行クラスの生成
            function.ComplexStaff ex
                = new function.ComplexStaff(divnum, lDataList.ToArray());

            ComplexStaff.fs = 44100;
            ComplexStaff.mergin = 50;
            ex.setTimeDistance(0.02);
            //ex.setTimeDistance(20 / 100);

            double[][] heldz = ex.GetMusicalNote(rank);

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
        private void testForHertz()
        {
            string filename = root + @"\testVer1.txt";

            // 短時間フーリエ変換するための格納・実行クラスの生成
            function.ComplexStaff ex
                = new function.ComplexStaff(divnum, lDataList.ToArray());

            ComplexStaff.fs = 44100;
            ComplexStaff.mergin = 50;
            ex.setTimeDistance(0.02);
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
        private IEnumerable<double> TracePath(IEnumerable<double> x, double percent)
        {
            int startI = 0; int tmpI = 0;
            double tmpVal = 0.0;
            double ans = 0.0;
            var aQuery = x.Select((c, i) => new { Index = i, Val = c - x.Max() * percent });
            foreach (var cursol in aQuery)
            {
                ans = 0.0;                                          // ans 初期化
                if (cursol.Index == 0)
                {
                    tmpVal = cursol.Val;
                    continue;
                }


                if(tmpVal * cursol.Val < 0)
                {
                    tmpI = Math.Abs(tmpVal) > Math.Abs(cursol.Val)
                        ? cursol.Index : cursol.Index - 1;          //x[c.i-1]==tmpVal
                    if(startI == 0)
                    {
                        startI = tmpI;                              //初期化
                    }
                    else
                    {
                        var trace = x.Skip(startI).Take(tmpI);
                        if (trace.Count() > 0)
                            ans = trace.Max();                      // ans 更新
                        startI = cursol.Index;                      // s 進める
                    }
                }
                tmpVal = cursol.Val;
                if(ans!=0)
                    Console.WriteLine("{0}:[1] => {2}",cursol.Index,　x.ElementAt(cursol.Index), ans);
                yield return ans;
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            int count = 20;
            int percent = 95; // percent
            int decreace = 5; //percent
            var data = TracePath(outPut, percent);
            do
            {
                percent -= decreace;
                data = TracePath(outPut, percent);
                Console.WriteLine("Continue!?");
            } while (data.Count() > count);
            Console.WriteLine("End");
        }
        private IEnumerable<double> outPut = new double[0];
        static int cursol = 0;
        static double[] data;
        private function.File f = new function.File();
        private const int len = 1000;
        void init()
        {
            if (len * (cursol + 1) > lDataList.Count()) cursol = 0;
            data = lDataList.Skip(len * cursol).Take(len).ToArray();

            button4.Enabled = (outPut.Count() > 0);
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
        private double[] initCepstrum()
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

            return ans.ToArray();
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

            double[] mag = initFFT(); outPut = mag;
            double[] dif = tourWave(mag);
            f.APlot(
                chart1, mag,
                "周波数", "FFT");
            f.APlot(
                chart2, dif,
                "周波数", "S-FFT");

            label2.Text = "sampleNo :" + cursol.ToString();
        }

        private void SDF_button_Click(object sender, EventArgs e)
        {
            init();
            outPut = initCepstrum();
            label2.Text = "sampleNo :" + cursol.ToString();
        }

        private void NSDF_button_Click(object sender, EventArgs e)
        {
            init(); initFFT();
            var y = DSP.TimeDomain.effector.M_NSDF(1, data);
            outPut = y;
            f.APlot(
                chart1,
                y.ToArray(),
                "時間","nsdf");
            label2.Text = "sampleNo :" + cursol.ToString();

        }
        private void button1_Click(object sender, EventArgs e)
        {
        }
        private void test_button_Click(object sender, EventArgs e)
        {
            testForHertzOnlyNote();
        }
        private void FFT_button_Click(object sender, EventArgs e)
        {

            init();

            ActiveComplex ac = new ActiveComplex(data, function.Fourier.WindowFunc.Hamming);
            ac.MusucalTransform();

            var mag = ac.GetMagnitude();
            double max = mag.Max();
            double maxFrequency = 27.5 * Math.Pow(2, mag.Count()/12);
            
            // mag[0] => A0 = 27.5 * 2^(0/12)

            double[] ans = mag.Select(c => c / max).ToArray();
            outPut = ans;
            f.APlot(
                chart1, ans,
                "周波数", "Mfft");


            ac = new ActiveComplex(data, function.Fourier.WindowFunc.Hamming);
            ac.FTransform(function.Fourier.ComplexFunc.FFT);

            mag = ac.GetMagnitude();
            max = mag.Max();

            int thretholdIndex = Enumerable.Range(0, int.MaxValue)
                .Select((_, i) => new { Index = i, Val = i * fs / mag.Count() })
                .Where(c => c.Val > maxFrequency)
                .Select(c => c.Index)
                .First();

            ans = mag.Select(c => c / max)
                .Select((val,i)=> new { Index = i, Val = val })
                .TakeWhile(c => c.Index < thretholdIndex)
                .Select(c => c.Val)
                .ToArray();
            f.APlot(
                chart2, ans,
                "周波数", "fft");

            label2.Text = "sampleNo :" + cursol.ToString();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            int tmp = decimal.ToInt32(numericUpDown1.Value);
            if (len * (cursol + 1) > lDataList.Count())
            {
                numericUpDown1.Value = 0;
                cursol = 0;
            }
            else
            {
                cursol = tmp;
            }
        }
    }
}