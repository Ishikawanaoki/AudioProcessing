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
        public double[] lDataList;
        public double[] rDataList;
        public WaveReAndWr.WavHeader header;
        private string root = @"..\..\音ファイル";
        /// <summary>
        /// 引数なしのコンストラクタは無効
        /// （フィールドは空）
        /// </summary>
        public WaveShow()
        {
            InitializeComponent();
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
        public WaveShow(List<short> lDataList1, List<short> rDataList1, WaveReAndWr.WavHeader header)
        {
            InitializeComponent();
            // コレクションの取り出し（配列化）
            short[] ltmp = lDataList1.ToArray();
            short[] rtmp = rDataList1.ToArray();

            // フィールドの初期化
            lDataList = new double[ltmp.Length];
            rDataList = new double[rtmp.Length];

            // 浮動小数点への変換
            for (int i = 0; i < ltmp.Length; i++)
            {
                lDataList[i] = (double)ltmp[i];
                rDataList[i] = (double)rtmp[i];
            }

            // ヘッダーエラーは保留とする
            //これによると、不適切なファイルとして再生できないソフトもある。
            // リニアPCMによる要件を纏める必要がある（保留）
            this.header = header;

            // 以下、左側のデータ列を扱う
            //左グラフには時系列、右グラフには高速フーリエ変換結果を出す
            Plot(chart1, lDataList);
            ActiveComplex acomp = new ActiveComplex(lDataList, Fourier.WindowFunc.Blackman);
            acomp.FTransform(Fourier.ComplexFunc.FFT);
            Plot(chart2, acomp.GetMagnitude().ToArray());
        }
        #region conPane
        private void WaveShow_Load(object sender, EventArgs e)
        {

        }
        private void WaveShow_Load_1(object sender, EventArgs e) { }
        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。");
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
            foreach (Fourier.WindowFunc fu in func)
            {
                ac = new ActiveComplex(lDataList, fu);
                lData = ac.FunctionTie();
                ac = new ActiveComplex(rDataList, fu);
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
            Console.WriteLine("アクションが終了しました。");
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
        private void Plot(Chart str, double[] y)
        {
            function.File f = new function.File();
            function.Axis plot_axis = f.Plot(str, y, "Area1", "時間 [s]");
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="divnum">波形の等分する分割数</param>
        private void testMyAnalys(int divnum)
        {
            double[] RspeAna;
            double[] LspeAna;

            // 短時間フーリエ変換するための格納・実行クラスの生成
            DSP.ComplexStaff ex
                = new DSP.ComplexStaff(divnum, lDataList);

            Tuple<double[], double[]> tuple = ex.DoSTDFT(root + @"\LeftSide.txt");
            LspeAna = tuple.Item2;
            Console.WriteLine("LFの実行");

            // 結果のグラフ表示
            // 左側の波形について
            // chart1 : 実行前の波形
            // chart2 : 実行後の波形
            Plot(chart1, lDataList);
            Plot(chart2, LspeAna);

            ex = new DSP.ComplexStaff(divnum, rDataList);
            RspeAna = ex.DoSTDFT(root + @"\RightSide.txt").Item2;
            Console.WriteLine("RFの実行");

            string filename = root + @"\mypractice.wav";
            WaveReAndWr.DataList<double> dlist
                = new WaveReAndWr.DataList<double>(
                    new List<double>(LspeAna),
                    new List<double>(RspeAna), 
                    header);

            //function.File.Write(filename, dlist, 5);
            WaveReAndWr.WavWriter(filename, function.File.ConvertDoubletoShort(dlist));
            Console.WriteLine("{0}を保存しました", filename);
        }
        private void test_button_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。");
            testMyAnalys(2000);
            DSP.FrequencyDomein.TestPitchDetect test = new DSP.FrequencyDomein.TestPitchDetect(100, lDataList);
            
            Console.WriteLine("アクションが終了しました。");
        }
        private void button5_Click(object sender, EventArgs e)
        {
            string filename = root + @"\targetarray.txt";

            // 短時間フーリエ変換するための格納・実行クラスの生成
            DSP.ComplexStaff ex
                = new DSP.ComplexStaff(2000, lDataList);

            double[] heldz = ex.GetHeldz();
            using (System.IO.FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                using (System.IO.StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (double str in heldz)
                    {
                        sw.WriteLine(str);
                        Console.Write("{0},", str);
                    }
                }
            }
            Console.WriteLine("配列を保存しました。");
        }
        private double[] getSampleWave2()
        {
            double[] ans = new double[200];
            for (int i = 0; i < ans.Length; i++)
                ans[i] = 0;
            return ans;
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
    }
}