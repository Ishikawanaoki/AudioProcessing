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
        private string root = @"..\..\音ファイル";

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
        private void test_try_ifft(double[] y, Boolean flag)
        {
            WaveReAndWr.DataList dlist = new WaveReAndWr.DataList();
            List<short> sample = new List<short>();
            string fileout2 = root + @"\out\MAN01.KOE.wav";
            double[] y_out = new double[y.Length];
            //int dim = 2; //記憶幅
            if (flag)
            {
                string arg = root + @"\a1.wav";
                string fileout = root + @"\out\MAN01.KOE.txt";

                dlist = WaveReAndWr.WavReader(arg, fileout, false);
                sample = dlist.lDataList;
                y_out = new double[sample.Count];
                
                for (int i = 0; i < sample.Count; i++)
                {
                    y_out[i] = sample[i];
                } 
            }

            double[] time = y_out;
            double[] retime= new double[y_out.Length];
            //for(int i=0; i<dim; i++)
            //{
            //    time[i] = y[i];
            //}
            //for(int i=dim; i<y.Length; i++)
            //{
            //    time[i] = 0.5*y[i] + 0.5*y[i - 2];
            //}
            myfuntion.Complex[] tmp = new Complex[y_out.Length];
            for (int i = 0; i < y_out.Length; i++)
            {
                tmp[i] = new Complex(y_out[i], 0);
            }

            myfuntion.Complex[]  tmp2 = Fourier.FFT(tmp);

            tmp = Fourier.IFFT(tmp2);

            for (int ii = 0; ii < tmp.Length; ii++)
            {
                retime[ii] = tmp[ii].magnitude;
                retime[ii] = Math.Log10(retime[ii]) * 10;
            }
            //描写開始
            Console.WriteLine("描写を開始します");
            Plot(time,1);
            Plot(retime, 2);

            if (flag)
            {
                for (int i = 0; i < sample.Count; i++)
                {
                    sample[i] = (short)(retime[i]);
                }
                dlist.lDataList = sample;
                dlist.rDataList = sample;

                WaveReAndWr.WavWriter(fileout2, dlist);
            }
        }
        private void test3()
        {
            string arg =        root + @"\a1.wav";
            string arg2 =       root + @"\data\MAN01.KOE";
            string fileout =    root + @"\out\MAN01.KOE.txt";
            string fileout2 =   root + @"\out\MAN01.KOE.wav";
            WaveReAndWr.DataList dlist = WaveReAndWr.WavReader(arg, fileout, false);
            List<short> sample = dlist.lDataList;

            double[] data = WaveReAndWr.includeFile(arg2);

            if (data.Length > sample.Count) return;

            double tmp;
            int i;
            double max = 0;
            for (i = 0; i < sample.Count; i++)
            {
                if (max < sample[i]) max = sample[i];
            }
            Console.WriteLine("max = {0}", max);
            for (i = 0; i < sample.Count; i++)
            {
                // 読み込んだwavのデータ数いっぱいに
                // KOEファイルを繰り返し代入している。
                // data列は100に正規化されているため、最大を
                // wavファイルでの最大へ持っていく。
                if (i % 70 == 0)
                {
                    tmp = data[i % data.Length] / 100 * max;
                }
                else tmp = 0;
                // waveの一つのデータブロックが
                // 16bit固定のため、やはり（データ入出）short、（演算）doubleが好ましい。
                sample[i] = (short)tmp;
            }

            dlist.lDataList = sample;
            dlist.rDataList = sample;

            WaveReAndWr.WavWriter(fileout2, dlist);
        }
        private void wave_generate_test()
        {
            // samplerate
            // "100Hz-2KAD.txt"      : 2000
            // "MAN01.KOE"           : 10000
            // "WOMAN01.KOE"         : 10000
            // defoult Wave format   : 44100
            
            // Nmaxは、データ数（標本の数）であり、ヘッダーに依存しないことを確認
            // すなわち、読み込んだwaveファイルの整数倍に時間を延ばすことも可能である。

            int Nmax;
            int rate = 44100;


            rate /= 2; // waverate = 22050 Hz
            string arg = root + @"\a1.wav";
            string fileout = root + @"\out\wavgene.wav";
            WaveReAndWr.LittleDataList dlist = WaveReAndWr.LittleWavReader(arg);

            Nmax = dlist.Nmax*10;
            short[] data = new short[Nmax];


            double[] func_out = myfunction2.DSP_Class.SquareWave(Nmax, rate);
            Console.WriteLine("Nmax = {0}", Nmax);
            
            double div = Math.PI * 2 / Nmax * rate;
            for (int i=0; i<Nmax; i++)
            {
                data[i] = (short)(func_out[i] * 16215);
            }

            // short => List<short>
            List<short> sample = new List<short>();
            sample.AddRange(data);
            // Do Writing
            WaveReAndWr.DataList datalist = new WaveReAndWr.DataList(sample, sample, dlist.WavHeader);
            WaveReAndWr.WavWriter(fileout, datalist);



        }

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("ボタンが押されました。");
            //test2(lDataList);
        
            wave_generate_test();
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

        private void test_button_Click(object sender, EventArgs e)
        {
            string arg2 = root + @"\data\MAN01.KOE";
            double[] data = WaveReAndWr.includeFile(arg2);
            test_try_ifft(data,true);
        }
    }
}