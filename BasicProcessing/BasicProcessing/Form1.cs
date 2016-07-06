using System;
using System.Windows.Forms;
using static myfuntion.WaveReAndWr;

namespace BasicProcessing
{
    /// <summary>
    /// このクラスは初期状態であり何も表示しない。
    /// <para name = data>
    /// data とは、wavファイルの全情報を持つインスタンスであり、また、
    /// 動的メソッドを呼ぶための一次格納庫、
    /// クラスコンストラクタでは初期化されません。
    /// 必ず新たなメソッドを作り、
    /// 2つの静的なメソッドWavReader, WavWriterを呼び出してください。
    /// button2はデフォルト操作</para>
    /// </summary>
    public partial class Form1 : Form
    {
        private DataList data;
        private string root = @".\";
        string[] IOFile;
        string fileout; // ヘッダー情報
        public Form1()
        {
            InitializeComponent();
            data = new DataList();
            IOFile = new string[2] {
                @".\音ファイル\g1.wav",
                @".\kekka_wav.txt" };
            fileout = @".\kekka_content_wav.txt";

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void button1_Click_1(object sender, EventArgs e)
        {
            ShowGlaph Glaph = new ShowGlaph(Show);
            Glaph(data);
        }

        /// <summary>
        /// 別ウィンドウにグラフを表示するだけのデリゲート
        /// 
        /// デリゲート（委譲）により、
        /// ボタン2の処理の最後にボタン1のハンドラーを追加することにて
        /// waveファイルを読み込み、そして次のように遷移させたい
        /// 　-> グラフ表示
        /// 　-> 時間／周波数の処理を行う -> グラフ表示
        /// 　-> etc
        /// 色々考えたが、クラス変数としてデータDataListを持つしかないと考える。
        /// やはり、データをフィールドとして持つには初期化が必要であり、
        /// またそのためにはフィールドに入力パスと出力パスを持ってしかるべきだと考える。
        /// </summary>
        delegate void ShowGlaph(DataList datalist);

        private void button2_Click(object sender, EventArgs e)
        {
            this.data = WavReader(IOFile[0], fileout);
            label1.Text = IOFile[0];
            Console.WriteLine("読み出しが成功しました。");
                               WavWriter(IOFile[1], data);
            Console.WriteLine("書き出しが成功しました。");

            ShowGlaph Glaph = new ShowGlaph(Show);
            Glaph(data);
        }
        static void Show(DataList datalist)
        {
            WaveShow show = new WaveShow(datalist.lDataList, datalist.rDataList);
            show.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string safeFileName;
            OpenFileDialog ofp = new OpenFileDialog();
            DialogResult dr;        // OpenfileDialog の結果を dr に格納
            dr = ofp.ShowDialog(this);

            IOFile[0] = ofp.FileName;

            safeFileName = ofp.SafeFileName;

            if (dr == DialogResult.OK)
            {
                char[] chArray1 = safeFileName.ToCharArray();
                char[] chArray2 = IOFile[0].ToCharArray();
                char[] chArray3 = new char[chArray2.Length - chArray1.Length];
                for (int i = 0; i < chArray3.Length; i++)
                    chArray3[i] = chArray2[i];
                label1.Text = safeFileName;
                root = new string(chArray3);
                IOFile[1] = root + @"kekka\kekka_wav.txt";
            }
        }
    }
}

