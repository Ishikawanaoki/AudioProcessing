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
        public const int divnum = 2000;
        /// <summary>
        /// 引数なしのコンストラクタは無効
        /// （フィールドは空）
        /// </summary>
        public WaveShow()
        {
            InitializeComponent();
            rank = new int[3]{ 2,3,4};
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
            Plot(chart1, lDataList);

            if (chart2_state)
            {
                ActiveComplex acomp = new ActiveComplex(lDataList.ToArray(), function.Fourier.WindowFunc.Blackman);
                acomp.FTransform(function.Fourier.ComplexFunc.FFT);
                Plot(chart2, acomp.GetMagnitude().ToArray());
             }

            //ActiveComplex acomp = new ActiveComplex(lDataList, function.Fourier.WindowFunc.Hamming);
            //Tuple<double[], double[]> tmp = acomp.HighPassDSP(2000);
            //Plot(chart1, tmp.Item1);
            //Plot(chart2, tmp.Item2);
            //ActiveComplex ac = new ActiveComplex(lDataList, function.Fourier.WindowFunc.Hamming);

            //lDataList = DSP.TimeDomain.Filter.HighPass(lDataList, 2000).ToArray();
            //ActiveComplex ac2 = new ActiveComplex(lDataList, function.Fourier.WindowFunc.Hamming);

            //ac.FTransform(function.Fourier.ComplexFunc.FFT);
            //ac2.FTransform(function.Fourier.ComplexFunc.FFT);

            //Plot(chart1, ac.GetReality().ToArray());
            //Plot(chart2, ac2.GetReality().ToArray());


        }
        #region conPane
        private void WaveShow_Load(object sender, EventArgs e)
        {

        }
        private void WaveShow_Load_1(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。");

            test_Filter();

            Console.WriteLine("アクションが終了しました。");
        }
        private void test_Filter()
        {
            chart1_state = true; chart2_state = true;

            //Ldata
            function.ActiveComplex ac = new function.ActiveComplex(lDataList.ToArray(), function.Fourier.WindowFunc.Hamming);

            //Plot(chart1, ac.HighPassDSP(2000).ToArray());
            //Plot(chart2, ac.LowPassDSP(2000).ToArray());
        }
        private void CompareWindows()
        {
            //testMyAnalys(2000);
            //DSP.FrequencyDomein.TestPitchDetect test = new DSP.FrequencyDomein.TestPitchDetect(100, lDataList);
            //test.Execute();
            List<Fourier.WindowFunc> func = new List<Fourier.WindowFunc>();
            func.Add(Fourier.WindowFunc.Blackman);
            func.Add(Fourier.WindowFunc.Hamming);
            func.Add(Fourier.WindowFunc.Hanning);
            func.Add(Fourier.WindowFunc.Rectangular);
            ActiveComplex ac;
            double[] lData; double[] rData; string filename;
            WaveReAndWr.DataList<double> dlist;
            foreach (function.Fourier.WindowFunc fu in func)
            {
                ac = new ActiveComplex(lDataList.ToArray(), fu);
                lData = ac.FunctionTie();
                ac = new ActiveComplex(rDataList.ToArray(), fu);
                rData = ac.FunctionTie();
                filename = root + @"\sound_" + fu.ToString() + ".wav";
                dlist = new WaveReAndWr.DataList<double>(
                        new List<double>(lData),
                        new List<double>(rData),
                        header);

                WaveReAndWr.WavWriter(filename,
                    function.File.ConvertDoubletoShort(dlist, 10));
                Console.WriteLine("{0}が保存されました。", filename);
            }
        }
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
        private void test_button_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。\n lData size : {0}",lDataList.Count());
            testForHertz2();
            Console.WriteLine("アクションが終了しました。");
        }
        private void button5_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。\n lData size : {0}", lDataList.Count());
            testForHertz();
            Console.WriteLine("アクションが終了しました。");
        }
        private void testForHertz()
        {
            string filename = root + @"\testVer1.txt";

            // 短時間フーリエ変換するための格納・実行クラスの生成
            function.ComplexStaff ex
                = new function.ComplexStaff(divnum, lDataList.ToArray());

            double[][] heldz = ex.GetHertz(rank);
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
        private void testForHertz2()
        {
            string filename = root + @"\testVer2.txt";

            // 短時間フーリエ変換するための格納・実行クラスの生成
            function.ComplexStaff ex
                = new function.ComplexStaff(divnum, lDataList.ToArray());

            double[][] heldz = ex.GetHertz2(rank);
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
        private void button4_Click(object sender, EventArgs e)
        {
            List<double> wave = getSampleWave(lDataList.Max(), 1000, 44100, 200);

            Plot(chart1, wave.ToArray());
            Console.WriteLine("Process End");

            string filename = root + @"\newwave.wav";

            //function.File.Write(filename, dlist, 5);
            WaveReAndWr.WavWriter(filename, function.File.ConvertDoubletoShort(new WaveReAndWr.DataList<double>(wave, wave, header)));
            Console.WriteLine("{0}を保存しました", filename);
        }
        private List<double> getSampleWave(double A, double f0, double fs, double length)
        {
            List<double> list = new List<double>();
            //for (int i = 0; i < length; i++)
            for (int n = 0; n < (length * fs); n++)
            {
                list.Add(
                    //A * Math.Sin(2 * Math.PI * f0 * i / length)
                    A * Math.Sin(2 * Math.PI * f0 * n / fs)
                    );
            }
            return list;
        }

        private void ACF_button_Click(object sender, EventArgs e)
        {
            Plot(chart2, 
                DSP.TimeDomain.effector.classedWaveForm(
                    DSP.TimeDomain.effector.M_ACF(100, lDataList)).ToArray());
        }

        private void SDF_button_Click(object sender, EventArgs e)
        {
            Plot(chart2,
                DSP.TimeDomain.effector.classedWaveForm(
                    DSP.TimeDomain.effector.M_SDF(100, lDataList)).ToArray());
        }

        private void NSDF_button_Click(object sender, EventArgs e)
        {
            Plot(chart2,
                DSP.TimeDomain.effector.classedWaveForm(
                    DSP.TimeDomain.effector.M_NSDF(100, lDataList)).ToArray());
        }
    }
}