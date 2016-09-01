using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
//チャートコントロールを使うために必要
using System.Windows.Forms.DataVisualization.Charting;

namespace BasicProcessing
{
    /// <summary>
    /// http://support2.dundas.com/OnlineDocumentation/WinChart2003/ChartAreas_3D.html
    /// 
    /// http://www.oborodukiyo.info/MSChart/VS2010/MSC-TwoSeriesOf3D.aspx
    /// </summary>
    public partial class Chart3D : Form
    {
        SqlDataAdapter sda = new SqlDataAdapter();
        DataTable dt = new DataTable();
        //Chart chart1 = new Chart();

        public Chart3D()
        {
            InitializeComponent();
        }

        private void Chart3D_Load(object sender, EventArgs e)
        {
            //ADO.NETでデータを取得する。
            SqlConnectionStringBuilder bldr = new SqlConnectionStringBuilder();
            bldr.DataSource = ".";
            bldr.InitialCatalog = "Northwind";
            bldr.IntegratedSecurity = true;

            using (SqlConnection conn = new SqlConnection(bldr.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT OrderID,SUM(Quantity) AS Quantity,SUM(UnitPrice * Quantity) AS Proceeds FROM [Order Details] WHERE OrderID <= 10250 GROUP BY OrderID ORDER BY OrderID";
                    this.sda.SelectCommand = cmd;
                    this.sda.Fill(this.dt);
                }
            }

            //チャートエリアの追加の仕方
            ChartArea a1 = new ChartArea("main");
            //3Dに設定する
            a1.Area3DStyle.Enable3D = true;
            chart1.ChartAreas.Add(a1);
            //凡例のインスタンスを追加する
            chart1.Legends.Add(new Legend());
            //左側のY軸の設定
            Axis y1 = new Axis();
            //目盛の間隔を5に設定
            y1.Interval = 5.0d;
            //目盛の最小値を20に設定
            y1.Minimum = 20.0d;
            //目盛の最大値を70に設定
            y1.Maximum = 70.0d;
            //左のY軸になるように設定
            a1.AxisY = y1;

            //右側のY軸の設定
            Axis y2 = new Axis();
            //目盛の間隔を200に設定
            y2.Interval = 200.0d;
            //右のY軸になるように設定
            a1.AxisY2 = y2;

            //一つ目のグラフのインスタンス
            Series s1 = new Series();
            //凡例での名前を設定
            s1.LegendText = "売上数量";
            //グラフの種類を折れ線グラフに設定
            s1.ChartType = SeriesChartType.Column;
            //グラフの色を半透明の赤に指定
            s1.Color = Color.FromArgb(100, Color.Red);
            //X軸のデータ名を設定
            s1.XValueMember = "OrderID";
            //下側のX軸の目盛を使用する
            s1.XAxisType = AxisType.Primary;
            //Y軸のデータ名を設定
            s1.YValueMembers = "Quantity";
            //左側のY軸の目盛を使用する
            s1.YAxisType = AxisType.Primary;
            //使用するチャートエリアを指定する
            s1.ChartArea = "main";



            //二つ目のグラフのインスタンス
            Series s2 = new Series();
            //凡例での名前を設定
            s2.LegendText = "売上";
            //グラフの種類を棒グラフに設定
            s2.ChartType = SeriesChartType.Column;
            //グラフの色を指定
            s2.Color = Color.Blue;
            //X軸のデータ名を設定
            s2.XValueMember = "OrderID";
            //下側のX軸の目盛を使用する
            s2.XAxisType = AxisType.Primary;
            //Y軸のデータ名を設定
            s2.YValueMembers = "Proceeds";
            //右側のY軸の目盛を使用する
            s2.YAxisType = AxisType.Secondary;
            //使用するチャートエリアを指定する
            s2.ChartArea = "main";

            //グラフを追加するときに、後の方が上になる。
            this.chart1.Series.Add(s2);
            this.chart1.Series.Add(s1);
            //フォームにチャートコントロールを追加する
            this.Controls.Add(this.chart1);
            this.chart1.Dock = DockStyle.Fill;
            //データをセットする。
            this.chart1.DataSource = dt;
        }


    }
}
