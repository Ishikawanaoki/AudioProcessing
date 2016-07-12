using myfuntion;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BasicProcessing
{
    /// <summary>
    /// to show glaphical audio wave from .wav format, 
    /// i use reading some tags for .wav.
    /// than next to do is to compare over some signal and generate some.
    /// than complete i-dft to be able to convert power spectol, is called f-domein, to limited  time-domein
    /// </summary>
    public partial class WaveShow : Form
    {
        double[] lDataList;
        double[] rDataList;
        private string root = @".\";

        public WaveShow()
        {
            InitializeComponent();
        }

        public WaveShow(List<short> lDataList1, List<short> rDataList1)
        {
            InitializeComponent();
            short[] ltmp = lDataList1.ToArray();
            short[] rtmp = rDataList1.ToArray();
            lDataList = new double[ltmp.Length];
            rDataList = new double[rtmp.Length];
            for(int i=0; i<ltmp.Length; i++)
            {
                lDataList[i] = (double)ltmp[i];
                rDataList[i] = (double)rtmp[i];
            }

            // 左側を処理します
            
            Plot(lDataList, 1);
            //double[] tmp = myfunction.DoDFT(lDataList);
            double[] tmp = myfunction.DoFFT(lDataList);
            Plot(tmp, 2);
        }
        private void WaveShow_Load(object sender, EventArgs e)
        {

        }
        /// <summary>
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
        /// LPF
        /// </summary>
        /// <param name="y"></param>
        private void test2(double[] y)
        {
            int dim = 2; //記憶幅

            double[] time = new double[y.Length];
            for(int i=0; i<dim; i++)
            {
                time[i] = y[i];
            }
            for(int i=dim; i<y.Length; i++)
            {
                time[i] = 0.5*y[i] + 0.5*y[i - 2];
            }
            double[] spectrum = myfunction.DoFFT(time);

            //描写開始
            Console.WriteLine("描写を開始します");
            Plot(time,1);
            Plot(spectrum, 2);
        }
        private void test3()
        {
            string arg = @"C:\Users\N.Ishikawa\Desktop\data\au\a1.wav";
            string arg2 = @"C:\Users\N.Ishikawa\Desktop\data\sample\MAN01.KOE";
            string fileout = @"C:\Users\N.Ishikawa\Desktop\data\sample\MAN01.KOE.wav";
            WaveReAndWr.DataList dlist = WaveReAndWr.WavReader(arg, "");
            List<short> sample = dlist.lDataList;

            double[] data = WaveReAndWr.includeFile(arg2);

            if (data.Length > sample.Count) return;

            for(int i=0; i<sample.Count; i++)
            {
                    sample[i] = (short)data[i%data.Length];
             }
            foreach(short str in sample){
                
            }


            dlist.lDataList = sample;
            dlist.rDataList = sample;

            WaveReAndWr.WavWriter(fileout, dlist);


        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。");
            //test2(lDataList);
            test3();
            Console.WriteLine("アクションが終了しました。");
        }
        /// <summary>
        /// 現在のchar1を保存します。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
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
                        if (saveFileDialog.FilterIndex == 2)
                        {
                            chart1.SaveImage(saveFileDialog.FileName, ChartImageFormat.Jpeg);
                        }
                        else if (saveFileDialog.FilterIndex == 1)
                        {
                            chart1.SaveImage(saveFileDialog.FileName, ChartImageFormat.Png);
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

            // SaveFileDialog の新しいインスタンスを生成する (デザイナから追加している場合は必要ない)
            //SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            // ダイアログのタイトルを設定する
            //saveFileDialog1.Title = "ここにダイアログのタイトルを書いてください";

            // 初期表示するディレクトリを設定する
            //saveFileDialog1.InitialDirectory = @"..\..\";

            // 初期表示するファイル名を設定する
            //saveFileDialog1.FileName = "ここに初期表示するファイル名を書いてください";

            // ファイルのフィルタを設定する
            //saveFileDialog1.Filter = "テキスト ファイル|*.txt;*.log|すべてのファイル|*.*";

            // ファイルの種類 の初期設定を 2 番目に設定する (初期値 1)
            //saveFileDialog1.FilterIndex = 2;

            // ダイアログボックスを閉じる前に現在のディレクトリを復元する (初期値 false)
            //saveFileDialog1.RestoreDirectory = true;

            // [ヘルプ] ボタンを表示する (初期値 false)
            //saveFileDialog1.ShowHelp = true;

            // 存在しないファイルを指定した場合は、
            // 新しく作成するかどうかの問い合わせを表示する (初期値 false)
            //saveFileDialog1.CreatePrompt = true;

            // 存在しているファイルを指定した場合は、
            // 上書きするかどうかの問い合わせを表示する (初期値 true)
            //saveFileDialog1.OverwritePrompt = true;

            // 存在しないファイル名を指定した場合は警告を表示する (初期値 false)
            //saveFileDialog1.CheckFileExists = true;

            // 存在しないパスを指定した場合は警告を表示する (初期値 true)
            //saveFileDialog1.CheckPathExists = true;

            // 拡張子を指定しない場合は自動的に拡張子を付加する (初期値 true)
            //saveFileDialog1.AddExtension = true;

            // 有効な Win32 ファイル名だけを受け入れるようにする (初期値 true)
            //saveFileDialog1.ValidateNames = true;

            // ダイアログを表示し、戻り値が [OK] の場合は、選択したファイルを表示する
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            //    MessageBox.Show(saveFileDialog1.FileName);
            //}

            // 不要になった時点で破棄する (正しくは オブジェクトの破棄を保証する を参照)
            //saveFileDialog1.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
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
                        if (saveFileDialog.FilterIndex == 2)
                        {
                            chart2.SaveImage(saveFileDialog.FileName, ChartImageFormat.Jpeg);
                        }
                        else if (saveFileDialog.FilterIndex == 1)
                        {
                            chart2.SaveImage(saveFileDialog.FileName, ChartImageFormat.Png);
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
    }
}