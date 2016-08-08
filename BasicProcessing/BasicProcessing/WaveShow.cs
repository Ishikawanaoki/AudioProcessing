using myfuntion;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BasicProcessing
{
    /// <summary>
    /// 第２番目にあたるウィンドウ、および処理をここに記述する。
    /// to show glaphical audio wave from .wav format, 
    /// i use reading some tags for .wav.
    /// than next to do is to compare over some signal and generate some.
    /// than complete i-dft to be able to convert power spectol, is called f-domein, to limited  time-domein
    /// </summary>
    public partial class WaveShow : Form
    {
        double[] lDataList;
        double[] rDataList;
        WaveReAndWr.WavHeader header;
        private string root = @"..\..\音ファイル";

        public WaveShow()
        {
            InitializeComponent();
        }

        public WaveShow(List<short> lDataList1, List<short> rDataList1, WaveReAndWr.WavHeader header)
        {
            InitializeComponent();
            
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
            this.header = header;
            
            // 以下、左側を処理します
            Plot(lDataList, 1);
            //double[] tmp = myfunction.DoDFT(lDataList);
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

        private System.Media.SoundPlayer player = null;
        //WAVEファイルを再生する
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
            initsample();
            Console.WriteLine("アクションが終了しました。");
        }
        private void test_button_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。");
            test_idft();
            Console.WriteLine("アクションが終了しました。");
        }
        /// <summary>
        /// 秘匿なメソッド
        /// (i) : 初期化時に描写された時系列デートの右に、dft->idftを通して得た結果を描写する
        /// (ii) : 得られた時系列データを、4倍の時間に伸ばしてWaveへ書き込む
        /// </summary>
        private void test_idft()
        {
        //(i)
            Complex[] tmp_f = myfunction.Manual_DoFFT(lDataList);
            double[] tmp_t = myfunction.DoIDFT(tmp_f);
            Plot(tmp_t, 2);

            string fileout = root + @"\madesound.wav";
            short[] ans = new short[tmp_t.Length * 4];
            int count = 0;
            short tmp = 0;

            for(int i=0; i<ans.Length; i++)
            {
                if(i%4 == 0)
                {
                    ans[i] = (short)tmp_t[count++];
                    tmp = ans[i];
                }
                ans[i] = tmp;
            }

            List<short> data = new List<short>();
            data.AddRange(ans);
            WaveReAndWr.DataList datalist = new WaveReAndWr.DataList(data, data, header);
            WaveReAndWr.WavWriter(fileout, datalist);

            //PlaySound(fileout);
        }
        /// <summary>
        /// テストを行う対象を自動的に作るプログラム
        /// </summary>
        public void initsample()
        {
            List<short> ldata = new List<short>();
            List<short> rdata = new List<short>();
            string longbinarysample = root + @"\bsample";
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
            for(int i=0; i<InFile.Length; i++)
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
            for(int iii=0; iii<ldata2.Length; iii++)
            {
                ldata[iii] = (short)ldata2[iii];
                rdata[iii] = (short)rdata2[iii];
            }
            tfilename = longbinarysample + "02.wav";
            tmp = new WaveReAndWr.DataList(ldata, rdata, header);
            WaveReAndWr.WavWriter(tfilename, tmp);


            //Plot(ldata2, 1);
            //double[] tmp = myfunction.DoDFT(lDataList);
            //double[] spec = myfunction.DoFFT(ldata2);

            //double[] ana = complexAnalysc(ldata2, 100);
            lDataList = complexAnalysc(ldata2, 100);
            Plot(lDataList, 2);
        }
        /// <summary>
        /// 短時間高速離散フーリエ変換を行う
        /// 
        /// Nmax = x.Length
        /// Nmax = k * j
        /// x[Nmax] => xx[k][j];
        /// short time = 0 ~ k
        /// j : 分割数
        /// </summary>
        /// <param name="x"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        double[] complexAnalysc(double[] x, int j)
        {
            int k = x.Length / j;
            double[] xx = new double[k];
            int count = 0;
            List<double> ans = new List<double>();
            Complex[] cmptmp;
            List<short> sign = new List<short>();
            List<double> sign2 = new List<double>();
            double[] ans_spec;
            
            for (int n = 0; n<j; n++)
            {
                for(int m = 0; m<k; m++)
                {
                    xx[m] = x[count++];
                }
                if (count > x.Length) break;
                xx = Fourier.Windowing(xx, Fourier.WindowFunc.Hamming);
                ans.AddRange(myfunction.DoFFT(xx));

                cmptmp = myfunction.Manual_DoDFT(xx);

                sign2.AddRange(myfunction.DoIDFT(cmptmp));
                sign.AddRange(myfunction.Do_s_IDFT(cmptmp));
            }

            ans_spec = ans.ToArray();

            Plot(sign2.ToArray(), 1);

            return ans_spec;
        }
        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}