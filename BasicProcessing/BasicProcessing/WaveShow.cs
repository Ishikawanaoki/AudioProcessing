using myfuntion;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BasicProcessing
{
    /// <summary>
    /// 第２番目にあたるウィンドウ、および処理記述
    /// </summary>
    public partial class WaveShow : Form
    {
        double[] lDataList;
        double[] rDataList;
        WaveReAndWr.WavHeader header;
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
            for (int i=0; i<ltmp.Length; i++)
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

            string[] xValues = new string[y.Length/2];

            myfuntion.Axis plot_axis = new myfuntion.Axis(y.Length, 44100);
            plot_axis.strighAxie(ref xValues);

            switch (no) {
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
            y = myfunction.seikika(y);

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

        /// <summary>
        /// クラス変数の追加
        /// playerにより再生状況を記憶している
        /// </summary>
        private System.Media.SoundPlayer player = null;
        /// <summary>
        /// 参照先 waveFile を再生する
        /// 不適切なファイルとして、現在無効
        /// </summary>
        /// <param name="waveFile"></param>
        private void PlaySound(string waveFile)
        {
            //再生されているときは止める
            if (player != null)
                StopSound();

            //読み込む
            player = new System.Media.SoundPlayer(waveFile);
            
            //非同期再生する
            //player.Play();

            //次のようにすると、ループ再生される
            //player.PlayLooping();

            //次のようにすると、最後まで再生し終えるまで待機する
            player.PlaySync();
        }

        //再生されている音を止める
        private void StopSound()
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                player = null;
            }
        }
        private void WaveShow_Load_1(object sender, EventArgs e)
        {
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。");
            //initsample();
            Console.WriteLine("アクションが終了しました。");
        }
        /// <summary>
        /// テストを行う対象を自動的に作るプログラム
        /// </summary>
        public void initsample()
        {
            // データ列リストの宣言、初期化
            List<short> ldata = new List<short>();
            List<short> rdata = new List<short>();
            // テスト対象ファイル名
            string longbinarysample = root + @"\bsample";
            // 空の文字列
            string tfilename = "";
            string[] InFile = new string[]
            {
                @"..\..\音ファイル\a1.wav",//@"..\..\音ファイル\a1s.wav",
                @"..\..\音ファイル\b1.wav",
                @"..\..\音ファイル\c1.wav",//@"..\..\音ファイル\c1s.wav",@"..\..\音ファイル\c2.wav",
                @"..\..\音ファイル\d1.wav",//@"..\..\音ファイル\d1s.wav",
                @"..\..\音ファイル\e1.wav",
                @"..\..\音ファイル\f1.wav",//@"..\..\音ファイル\f1s.wav",
                @"..\..\音ファイル\g1.wav"//,@"..\..\音ファイル\g1s.wav"
            };
            WaveReAndWr.DataList tmp;
            for (int i = 0; i < InFile.Length; i++)
            {
                tmp = WaveReAndWr.WavReader(InFile[i], "", false);
                ldata.AddRange(tmp.lDataList);
                rdata.AddRange(tmp.rDataList);
                if (i == 0) this.header = tmp.WavHeader;
            }
            tfilename = longbinarysample + "01.wav";
            tmp = new WaveReAndWr.DataList(ldata, rdata, header);
            WaveReAndWr.WavWriter(tfilename, tmp);

            double[] ldata2 = new double[ldata.Count];
            double[] rdata2 = new double[rdata.Count];
            for (int ii = 0; ii < ldata.Count; ii++)
            {
                ldata2[ii] = ldata[ii];
                rdata2[ii] = rdata[ii];
            }
            ldata2 = myfunction.seikika(ldata2);
            rdata2 = myfunction.seikika(rdata2);

            //PlaySound(tfilename);
            for (int iii = 0; iii < ldata2.Length; iii++)
            {
                ldata[iii] = (short)ldata2[iii];
                rdata[iii] = (short)rdata2[iii];
            }
            tfilename = longbinarysample + "02.wav";
            tmp = new WaveReAndWr.DataList(ldata, rdata, header);
            WaveReAndWr.WavWriter(tfilename, tmp);
        }
        private void test_button_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。");
            //test_idft();
            int divnum = 100;
            int[] LFru = new int[lDataList.Length/divnum];
            int[] RFru = new int[rDataList.Length/divnum];
            myfunction2.DSP_Class ex = new myfunction2.DSP_Class();
            // 参照渡しにより、フィールドの時系列データは正弦波で再生されている。
            // 変数 LFru, RFru は divnum 個のサンプルで共通する周波数[Hz}で再生する
            LFru = ex.complexSearchF(ref lDataList, divnum);
            RFru = ex.complexSearchF(ref rDataList, divnum);

            string filename = root + @"\mypractice.wave";
            Write(filename, lDataList, rDataList);

            

            Console.WriteLine("アクションが終了しました。");
        }
        /// <summary>
        /// 任意の時系列データdataを、
        /// 任意の出力先filenameへと保存する。
        ///  + 左右の主利を追加
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="data"></param>
        private void Write(string filename, double[] Lindata, double[] Rindata)
        {
            short[] ans = new short[Lindata.Length * 4];
            int count = 0;
            short tmp = 0;

            for (int i = 0; i < ans.Length; i++)
            {
                if (i % 4 == 0)
                {
                    ans[i] = (short)Lindata[count++];
                    tmp = ans[i];
                }
                ans[i] = tmp;
            }

            List<short> Ldata = new List<short>();
                Ldata.AddRange(ans);

            count = 0; // *
            for (int i = 0; i < ans.Length; i++)
            {
                if (i % 4 == 0)
                {
                    ans[i] = (short)Rindata[count++];
                    tmp = ans[i];
                }
                ans[i] = tmp;
            }

            List<short> Rdata = new List<short>();
            Rdata.AddRange(ans);
            // フィールド変数から、ヘッダーを参照しています。
            WaveReAndWr.DataList datalist = new WaveReAndWr.DataList(Ldata, Rdata, this.header);
            WaveReAndWr.WavWriter(filename, datalist);
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            string safeFileName;
            OpenFileDialog ofp = new OpenFileDialog();
            DialogResult dr;        // OpenfileDialog の結果を dr に格納
            dr = ofp.ShowDialog(this);

            safeFileName = ofp.SafeFileName;

            if (dr == DialogResult.OK)
            {
                label3.Text = safeFileName;
            }

            safeFileName = root + "\\" + safeFileName;
            WaveReAndWr.DataList data = WaveReAndWr.WavReader(safeFileName, "", false);
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }
}