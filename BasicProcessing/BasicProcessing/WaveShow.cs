using function;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Plot(lDataList, 1);
            double[] tmp = myfunction.DoFFT(lDataList);
            Plot(tmp, 2);
        }
        private void WaveShow_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// グラフの二画面表示を行う。
        /// 初期では左:時間領域、右:周波数領域で表示され、
        /// 第二引数noによってグラフ位置の選択、グラフの更新を行う。
        /// 
        /// </summary>
        /// <param name="y">1列のデータを用いて、グラフ表示します。</param>
        /// <param name="no">2つのchartを分別して、描写対象を決定できる</param>
        private void Plot(double[] y, int no)
        {
            Series seriesLine = new Series();
            seriesLine.ChartType = SeriesChartType.Line; // 折れ線グラフ
            seriesLine.LegendText = "Legend:Line";       // 凡例

            string[] xValues = new string[y.Length / 2];

            function.Axis plot_axis = new function.Axis(y.Length, 44100);
            plot_axis.strighAxie(ref xValues);

            switch (no)
            {
                case 1:
                    chart1.Titles.Clear();
                    chart1.Series.Clear();
                    chart1.ChartAreas.Clear();

                    chart1.Series.Add("Area1");
                    chart1.ChartAreas.Add(new ChartArea("Area1"));            // ChartArea作成
                    chart1.ChartAreas["Area1"].AxisX.Title = "時間 t [s]";  // X軸タイトル設定
                    chart1.ChartAreas["Area1"].AxisY.Title = "[/]";  // Y軸タイトル設定

                    chart1.Series["Area1"].ChartType = SeriesChartType.Line;

                    break;
                case 2:
                    chart2.Titles.Clear();
                    chart2.Series.Clear();
                    chart2.ChartAreas.Clear();

                    chart2.Series.Add("Area2");
                    chart2.ChartAreas.Add(new ChartArea("Area2"));            // ChartArea作成
                    chart2.ChartAreas["Area2"].AxisX.Title = "周波数 f [Hz]";  // X軸タイトル設定
                    chart2.ChartAreas["Area2"].AxisY.Title = "[/]";  // Y軸タイトル設定

                    chart2.Series["Area2"].ChartType = SeriesChartType.Line;
                    break;
            }

            // 正規化を行います
            y = myfunction.seikika(y).ToArray();

            for (int i = 0; i < xValues.Length; i++)
            {
                DataPoint dp = new DataPoint();
                //グラフに追加するデータクラスを生成
                switch (no)
                {
                    case 1:
                        dp.SetValueXY(xValues[i], y[i]);  //XとYの値を設定
                        dp.IsValueShownAsLabel = false;  //グラフに値を表示しないように指定
                        chart1.Series["Area1"].Points.Add(dp);   //グラフにデータ追加
                        break;
                    case 2:
                        dp.SetValueXY(xValues[i], y[i]);  //XとYの値を設定
                        dp.IsValueShownAsLabel = false;  //グラフに値を表示するように指定
                        chart2.Series["Area2"].Points.Add(dp);   //グラフにデータ追加
                        break;
                }
            }
            label1.Text = "DTime : " + plot_axis.time.ToString();
            label2.Text = "DFreq : " + plot_axis.frequency.ToString();

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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="no">chart No.{1 or 2}</param>
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
        private void WaveShow_Load_1(object sender, EventArgs e)
        {
        }
        private void testmyanalys(int divnum)
        {
            myfunction2.ComplexStaff ex 
                = new myfunction2.ComplexStaff(divnum, lDataList);

            double[] RspeAna;
            double[] LspeAna;


            LspeAna = ex.DoSTDFT();
            Console.WriteLine("LFの実行");

            Plot(lDataList, 1);
            Plot(LspeAna, 2);

            ex = new myfunction2.ComplexStaff(divnum, rDataList);
            RspeAna = ex.DoSTDFT();
            Console.WriteLine("RFの実行");

            string filename = root + @"\mypractice.wave";
            WaveReAndWr.DataList<double> dlist
                = new WaveReAndWr.DataList<double>(new List<double>(LspeAna), new List<double>(RspeAna), header);

            Write(filename, dlist, 5);
            Console.WriteLine("{0}を保存しました", filename);
        }
        private void userdefined()
        {
            myfunction2.FrequencyDomein.PichDetect pitch
                = new myfunction2.FrequencyDomein.PichDetect(lDataList);
            pitch.AnalyzeSound();
        }
        private void test_button_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。");

            testmyanalys(200);


            Console.WriteLine("アクションが終了しました。");
        }
        /// <summary>
        /// 任意の時系列データdataを、
        /// 任意の出力先filenameへと保存する。
        ///  + データを任意整数倍に間引きすることで矩形波になると推測
        /// </summary>
        /// <param name="filename">保存ファイル名</param>
        /// <param name="Lindata">左</param>
        /// <param name="Rindata">右</param>
        /// <param name="times">間引きするデータ数</param>
        private void Write(string filename, WaveReAndWr.DataList<double> dlist, int times)
        {
            if (times <= 0) return; // 中止
            int count = 0; // カウンタ変数
            short Ltmp = 0; short Rtmp = 0; // 間引きの時に書き出す、値を格納
            int size = dlist.rDataList.Count * times;
            List<short> Ldata = new List<short>();
            List<short> Rdata = new List<short>();
            for (int i = 0; i < size; i++)
            {
                if (i % times == 0)
                {
                    Ltmp = (short)dlist.lDataList[count];
                    Rtmp = (short)dlist.rDataList[count];
                }
                Ldata.Add(Ltmp); // キャスト代入
                Rdata.Add(Rtmp); // キャスト代入
            }
            // フィールド変数から、ヘッダーを参照しています。
            WaveReAndWr.DataList<short> datalist = new WaveReAndWr.DataList<short>(Ldata, Rdata, dlist.WavHeader);
            WaveReAndWr.WavWriter(filename, datalist);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            string safeFileName;
            OpenFileDialog ofp = new OpenFileDialog();
            DialogResult dr;        // OpenfileDialog の結果を dr に格納
            dr = ofp.ShowDialog(this);

            safeFileName = ofp.SafeFileName;

            safeFileName = root + "\\" + safeFileName;
        }
        private void label3_Click(object sender, EventArgs e)
        {

        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void label2_Click(object sender, EventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {
            testMathematicalWave();
        }
        private void testMathematicalWave()
        {
            function.otherUser.MathematicalWave func = new function.otherUser.MathematicalWave();
            double[] tw = func.createTriangleWave(1, 1, 1, 200);
            double[] sw = func.createSawtoothWave(1, 1, 1, 200);
            double[] sample = getSampleWave(1, 1, 1, 200).ToArray();
            Plot2(tw,getSampleWave2(),1);
            Plot2(sw,getSampleWave2(), 2);
            Console.WriteLine("tw.Length  : {0}", tw.Length);
            Console.WriteLine("sw.Length  : {0}", sw.Length);
            Console.WriteLine("sample.Length  : {0}", sample.Length);
            for(int i=0; i<tw.Length; i++)
                Console.WriteLine("{0}  : {1},{2},{3}", i, tw[i], sw[i], sample[i]);

        }
        private void test()
        {
            function.otherUser.MathematicalWave func = new function.otherUser.MathematicalWave();
            func.exMain();
        }
        private double[] getSampleWave2()
        {
            double[] ans = new double[200];
            for (int i = 0; i < ans.Length; i++)
                ans[i] = 0;
            return ans;
        }
        private List<double> getSampleWave(double A,double f0, double fs, double length)
        {
            List<double> list = new List<double>();
            for (int i = 0; i < length; i++)
                // for (int n = 0; n < (length * fs); n++)
                list.Add(
                    Math.Sin(2 * Math.PI * f0 * i / length)
                    // Math.Sin(m * 2 * Math.PI * f0 * n / fs);
                    );
            return list;
        }
        private void Plot2(double[] y, double[] y2, int no)
        {
            string[] xValues = new string[y.Length];

            function.Axis plot_axis = new function.Axis(y.Length, 100);
            plot_axis.strighAxie(ref xValues);

            switch (no)
            {
                case 1:
                    chart1.Titles.Clear();
                    chart1.Series.Clear();
                    chart1.ChartAreas.Clear();

                    chart1.Series.Add("Area1");
                    chart1.ChartAreas.Add(new ChartArea("Area1"));            // ChartArea作成
                    chart1.ChartAreas["Area1"].AxisX.Title = "時間 t [s]";  // X軸タイトル設定
                    chart1.ChartAreas["Area1"].AxisY.Title = "[/]";  // Y軸タイトル設定

                    chart1.Series.Add("SampleWave");
                    chart1.ChartAreas.Add(new ChartArea("SampleWave"));            // ChartArea作成
                    chart1.ChartAreas["SampleWave"].AxisX.Title = "時間 t [s]";  // X軸タイトル設定
                    chart1.ChartAreas["SampleWave"].AxisY.Title = "[/]";  // Y軸タイトル設定

                    chart1.Series["Area1"].ChartType = SeriesChartType.Line;
                    chart1.Series["SampleWave"].ChartType = SeriesChartType.Line;

                    break;
                case 2:
                    chart2.Titles.Clear();
                    chart2.Series.Clear();
                    chart2.ChartAreas.Clear();

                    chart2.Series.Add("Area1");
                    chart2.ChartAreas.Add(new ChartArea("Area1"));            // ChartArea作成
                    chart2.ChartAreas["Area1"].AxisX.Title = "時間 t [s]";  // X軸タイトル設定
                    chart2.ChartAreas["Area1"].AxisY.Title = "[/]";  // Y軸タイトル設定

                    chart2.Series.Add("SampleWave");
                    chart2.ChartAreas.Add(new ChartArea("SampleWave"));            // ChartArea作成
                    chart2.ChartAreas["SampleWave"].AxisX.Title = "時間 t [s]";  // X軸タイトル設定
                    chart2.ChartAreas["SampleWave"].AxisY.Title = "[/]";  // Y軸タイトル設定

                    chart2.Series["Area1"].ChartType = SeriesChartType.Line;
                    chart2.Series["SampleWave"].ChartType = SeriesChartType.Line;
                    break;
            }

            for (int i = 0; i < xValues.Length; i++)
            {
                switch (no)
                {
                    case 1:
                        DataPoint dp = new DataPoint();
                        DataPoint dp1 = new DataPoint();
                        dp.SetValueXY(xValues[i], y[i]);
                        dp.IsValueShownAsLabel = false;
                        chart1.Series["Area1"].Points.Add(dp);

                        dp1.SetValueXY(xValues[i], y2[i]);
                        dp1.IsValueShownAsLabel = false;
                        chart1.Series["SampleWave"].Points.Add(dp1);
                        break;
                    case 2:
                        DataPoint dp2 = new DataPoint();
                        DataPoint dp3 = new DataPoint();
                        dp2.SetValueXY(xValues[i], y[i]);
                        dp2.IsValueShownAsLabel = false;
                        chart2.Series["Area1"].Points.Add(dp2);

                        dp3.SetValueXY(xValues[i], y2[i]);
                        dp3.IsValueShownAsLabel = false;
                        chart2.Series["SampleWave"].Points.Add(dp3);
                        break;
                }
                

            }
            
            label1.Text = "DTime : " + plot_axis.time.ToString();
            label2.Text = "DFreq : " + plot_axis.frequency.ToString();
        }
    }
}