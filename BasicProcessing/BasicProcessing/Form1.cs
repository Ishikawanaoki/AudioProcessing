using System;
using System.Windows.Forms;
using static function.WaveReAndWr;

namespace BasicProcessing
{
    /// <summary>
    /// このクラスは初期状態であり、
    /// 次のオプションがある。
    ///  -(1)- 初期指定のﾊﾞｲﾅﾘﾌｧｲﾙ(Wave)を読み込み、書き出す。また、コンソールに状態表示
    ///  ----- 読み込み先および、書き込み先はIOFile[0],[1]にあたる
    ///  -(2)- (1)における読込先IOFile[0] を変更する
    ///  -(3)- (1)によるデータを用いて、時系列データに変更を加えるため、Waveshowへデータを渡す
    ///  ----- (3)は(1)の副次的動作として記述。なにより、フィールドを無下に増やさないため。
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
        //private DataList data;
        private static string root = @"..\..\音ファイル";
        string[] IOFile;
        string fileout; // ヘッダー情報
        public Form1()
        {
            TestFunction tf = new TestFunction();
            tf.Show();

            InitializeComponent();
            IOFile = new string[2] {
                root + @"\a1.wav",              // 初期指定のバイナリ読み込み先
                    // button3により更新、 button2により読み出し
                root + @"\out\kekka_wav.txt"    // 初期指摘のバイナリ書き込み先
                    // button2（無効）
            };

            fileout = root + @"\out\wav_header.txt";　// ヘッダー情報
                    // button2 WavReaderメソッドの引数をtrueなら有効（無効）
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void button1_Click_1(object sender, EventArgs e)
        {
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
        /// クラス変数としてデータDataListを持つ。
        /// データをフィールドとして持つには初期化が必要、
        /// フィールドに入力パスと出力パスを持つ。
        /// </summary>
        delegate void ShowGlaph(DataList<short> datalist);

        /// <summary>
        /// "実行"ボタン挿入による処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            DataList<short> data = WavReader(IOFile[0], fileout, false);
            label1.Text = IOFile[0];
            Console.WriteLine("読み出しが成功しました。");
            

            // 同じﾊﾞｲﾅﾘﾌｧｲﾙを書き出す操作
            //WavWriter(IOFile[1], data);
            //Console.WriteLine("書き出しが成功しました。");

            // 以下(3)の動作
            // 委譲により、内部でのWaveShowウィンドウを作成、表示する。
            ShowGlaph Glaph = new ShowGlaph(Show);
            Glaph(data);
        }
        static void Show(DataList<short> datalist)
        {
            WaveShow show = new WaveShow(datalist.lDataList, datalist.rDataList, datalist.WavHeader);
            show.Show();
        }

        /// <summary>
        /// IOFileの書換えと、ファイル名のみのラベル表示を行う。
        /// ＊[ファイルを開く]ダイアログをキャンセルすると、フィールド変数がnull参照となるため、動作不安定。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                IOFile[1] = root + @"\" + safeFileName + ".txt";
            }
        }
    }
}

