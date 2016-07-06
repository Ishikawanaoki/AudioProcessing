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
        short[] lDataList;
        short[] rDataList;
        // private short lDataList1;
        // private short rDataList1;

        public WaveShow()
        {
            InitializeComponent();
        }

        public WaveShow(List<short> lDataList1, List<short> rDataList1)
        {
            InitializeComponent();
            this.lDataList = lDataList1.ToArray();
            this.rDataList = rDataList1.ToArray();
            // 実行

            double[] tmp = new double[lDataList.Length];
            int j = 0;
            foreach (short i in lDataList) tmp[j++] = (double)i;
            Plot(tmp, 1);
            test(lDataList);
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
            String gname = "sample";
            int[] xValues = new int[y.Length];
            switch (no) {
                case 1:
                    chart1.Series.Clear();
                    chart1.Series.Add(gname);
                    chart1.Series[gname].ChartType = SeriesChartType.Line;

                    break;
                case 2:
                    chart2.Series.Clear();
                    chart2.Series.Add(gname);
                    chart2.Series[gname].ChartType = SeriesChartType.Line;
                    break;
            }
            for (int i = 0; i < xValues.Length; i++)
            {
                //グラフに追加するデータクラスを生成
                DataPoint dp = new DataPoint();
                dp.SetValueXY(xValues[i], y[i]);  //XとYの値を設定
                dp.IsValueShownAsLabel = false;  //グラフに値を表示するように指定
                switch (no)
                {
                    case 1:
                        chart1.Series[gname].Points.Add(dp);   //グラフにデータ追加
                        break;
                    case 2:
                        chart2.Series[gname].Points.Add(dp);   //グラフにデータ追加
                        break;
                }
             }
        }
        /// <summary>
        /// DFTを実行し、そしてグラフ描写をするテストクラスです
        /// <para name = "sign">時間信号の複素数列</para>
        /// <para name = "do_dft">結果である複素数列</para>
        /// </summary>
        private void test(short[] y) //本体
        {
            Complex[] sign = new Complex[y.Length];
            Complex[] do_dft = new Complex[y.Length];

            double seikika = 0;
            double[] y_out = new double[y.Length];


            for (int i = 0; i < y.Length; i++)
            {
                sign[i] = new Complex((double)y[i], 0);
            }
            
            do_dft = Fourier.DFT(sign);

            for (int ii = 0; ii < y.Length; ii++)
            {
                y_out[ii] = do_dft[ii].magnitude;
                y_out[ii] = Math.Log10(y_out[ii]) * 10;
                if (seikika < y_out[ii]) seikika = y_out[ii];
            }


            for (int iii = 0; iii < y.Length; iii++)
                y_out[iii] = y_out[iii] / seikika * 100;

            Plot(y_out, 2);
        }
    }
}