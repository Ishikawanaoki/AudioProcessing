using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 前のプロジェクトからの名残を転々と映していたりしたため、
/// 動的処理と静的処理がごちゃごちゃになり
/// メソッド呼出しの煩雑さや、
/// 処理速度の超重化が観られたため
/// 新たな名前空間へと静的処理を移行します。
/// 
/// 動的処理とは！？
/// 多くがフォーム（ウィンドウ）に関わるパーツの制御です。
/// そのため、静的処理関数群、普遍的な表示部、内部処理決定部（本プロジェクトの主体）
/// の3つに細分化することで完成されます。
/// 
/// ここまで読んでいただいた方へ
/// 疑問点や意見があれば、ぜひ教えて頂きたいです。
/// プロジェクト展開の助けになればと誠心誠意の対応をさせていただきます。
/// 
/// 目次
///  - 複素数クラス
///         -- 明示的に複素数を扱うためにもちいる、格納庫です。
///         -- 扱うプリメティブ変数はdoubleであるため、そこそこの演算精度が期待できます。
///         -- 用いる変数について調査願います。
///  - 複素数クラスを用いた演算
///     - DFT（離散フーリエ変換）
///     - IDFT（逆フーリエ変換）
///     - FFT（高速フーリエ変換）
///     - IFFT（逆高速フーリエ変換）
///         -- 複素数クラスを扱う1つの数学クラスとして作りました。
///         -- フーリエ解析では次のことを期待して実装していきます。
///             時間 -> 周波数
///                による暗示的に有限時間（周期関数）と限定した元での
///                スペクトル表示、また一部更新をします。
///             周波数 -> 時間
///                 更新されたスぺクトルをもとに（短い）任意時間での音波の高さへと戻すことが期待できます。
///                 また、任意のパワースペクトルの大きさを指定することで任意の音の高さへの表現が期待できます。
///                 考察として、
///                 ・ディジタルフィルタを扱うならすべて時間領域の処理となり、フーリエ変換が不必要となります。
///                 ・データ数が増えることでのｺﾝﾃｷｽﾄﾃﾞｯﾄﾞﾛｯｸというエラーが起こり得ますが、もっと効率の良い処理方法が必要です。
///                 ・MEMやデータのモデル化は将来的には欲しいです。
///         -- 一部まだ実装されていません。
///         -- FFT関連は要素数への制限があり、呼び出す前に考慮してください。
///         -- 上に加えて、転居による不具合があるかもしれません。
///  - 
/// </summary>
namespace myfuntion
{
    /// <summary>
    /// 内部処理決定部
    /// (主要)
    /// </summary>
    class myfunction
    {
    }

    /// <summary>
    /// なんか難しい構造体です。
    /// フーリエ変換後の値を格納します。
    /// </summary>
    class Complex
    {
        public double real = 0.0;
        public double imag = 0.0;


        /// <summary>
        /// コンストラクタです。
        /// 引数なしの場合には初期化しません。
        /// 普通呼びません。
        /// </summary>
        public Complex()
        {
        }

        /// <summary>
        /// フィールドへの初期化でず。
        /// </summary>
        /// <param name="real">実部です。</param>
        /// <param name="img">虚部です。</param>
        public Complex(double real, double img)
        {
            this.real = real;
            this.imag = img;
        }

        /// <summary>
        /// 文字出力のオーバライド
        /// Ex: 3+5i = "3+5i"
        /// </summary>
        /// <returns></returns>
        override public string ToString()
        {
            string data = real.ToString() + "+" + imag.ToString() + "i";
            return data;
        }


        /// <summary>
        /// 極座標からx-y座標への変換
        /// </summary>
        /// <param name="r"></param>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static Complex from_polar(double r, double radians)
        {
            Complex data = new Complex(r * Math.Cos(radians), r * Math.Sin(radians));
            return data;
        }


        /// <summary>
        /// 複素数同士の和
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator +(Complex a, Complex b)
        {
            Complex data = new Complex(a.real + b.real, a.imag + b.imag);
            return data;
        }


        /// <summary>
        /// 複素数同士の差
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator -(Complex a, Complex b)
        {
            Complex data = new Complex(a.real - b.real, a.imag - b.imag);
            return data;
        }


        /// <summary>
        /// 複素数同士の積
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Complex operator *(Complex a, Complex b)
        {
            Complex data = new Complex((a.real * b.real) - (a.imag * b.imag),
           (a.real * b.imag + (a.imag * b.real)));
            return data;
        }


        /// <summary>
        /// 複素数同士の商
        /// </summary>
        public double magnitude
        {
            get
            {
                return Math.Sqrt(Math.Pow(real, 2) + Math.Pow(imag, 2));
            }
        }

        /// <summary>
        /// Re-Im 平面上の単位円における偏角
        /// </summary>
        public double phase
        {
            get
            {
                return Math.Atan(imag / real); // アークタンジェントを返し、-n/2<=theta<=n/2となる値を返す
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    class Fourier
    {

        /// <summary>
        /// 窓関数の選択
        /// Windowing()の呼び出しでもこの列挙帯への参照を必要とします。
        /// </summary>
        public enum WindowFunc
        {
            Hamming,
            Hanning,
            Blackman,
            Rectangular
        }

        /// <summary>
        /// 列挙帯により異なる方法で窓関数を働きかけます。
        /// 現段階では、実行部から直接呼ぶこと、
        /// つまり、窓関数を働かせ、フーリエ変換を働かせることは一連ではないので注意が必要です。
        /// </summary>
        /// <param name="data"></param>
        /// <param name="windowFunc"></param>
        /// <returns></returns>
        public static double[] Windowing(double[] data, WindowFunc windowFunc)
        {
            int size = data.Length;
            double[] windata = new double[size];

            for (int i = 0; i < size; i++)
            {
                double winValue = 0;
                // 各々の窓関数
                if (WindowFunc.Hamming == windowFunc)
                {
                    winValue = 0.54 - 0.46 * Math.Cos(2 * Math.PI * i / (size - 1));
                }
                else if (WindowFunc.Hanning == windowFunc)
                {
                    winValue = 0.5 - 0.5 * Math.Cos(2 * Math.PI * i / (size - 1));
                }
                else if (WindowFunc.Blackman == windowFunc)
                {
                    winValue = 0.42 - 0.5 * Math.Cos(2 * Math.PI * i / (size - 1))
                                    + 0.08 * Math.Cos(4 * Math.PI * i / (size - 1));
                }
                else if (WindowFunc.Rectangular == windowFunc)
                {
                    winValue = 1.0;
                }
                else
                {
                    winValue = 1.0;
                }
                // 窓関数を掛け算
                windata[i] = data[i] * winValue;
            }
            return windata;
        }


        /// <summary>
        /// 離散フーリエ変換
        /// 回転子 double型 d_theta
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Complex[] DFT(Complex[] x)
        {
            int N = x.Length;
            Complex[] X = new Complex[N];
            double d_theta = (-2) * Math.PI / N;
            for (int k = 0; k < N; k++)
            {
                X[k] = new Complex(0, 0);
                for (int n = 0; n < N; n++)
                {
                    // Complex temp = Complex.from_polar(1, -2 * Math.PI * n * k / N);
                    Complex temp = Complex.from_polar(1, d_theta * n * k);
                    temp *= x[n]; //演算子 * はオーバーライドしたもの
                    X[k] += temp; //演算子 + はオーバーライドしたもの
                }
            }
            return X;
        }

        /// <summary>
        /// 高速フーリエ変換
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Complex[] FFT(Complex[] x)
        {
            int N = x.Length;
            Complex[] X = new Complex[N];
            Complex[] d, D, e, E;
            if (N == 1)
            {
                X[0] = x[0];
                return X;
            }
            int k;
            e = new Complex[N / 2];
            d = new Complex[N / 2];
            for (k = 0; k < N / 2; k++)
            {
                e[k] = x[2 * k];
                d[k] = x[2 * k + 1];
            }
            D = FFT(d);
            E = FFT(e);
            for (k = 0; k < N / 2; k++)
            {
                Complex temp = Complex.from_polar(1, -2 * Math.PI * k / N);
                D[k] *= temp;
            }
            for (k = 0; k < N / 2; k++)
            {
                X[k] = E[k] + D[k];
                X[k + N / 2] = E[k] - D[k];
            }
            return X;
        }
    }
    class WaveReAndWr
    {
        /// <summary>
        /// The header is used to provide specifications on the file type,
        /// sample rate,
        /// sample size
        /// and bit size of the file,
        /// as well as its overall length.
        /// The header of a WAV(RIFF) file is 44 bytes long.
        /// （約）ヘッダーの大きさは、ファイルタイプや標本化周波数、標本数、ファイルサイズによって変わる。
        /// ＊基準として 44byte を取るとすると、
        /// 2番目 size := filesize    -  8
        /// 13番目 data := filesize   - 14
        /// また、
        /// bytePerSec, blockSizeは相関値である。
        /// その為ファイルを交信したい場合には次のメンバのみが独立で、変更可能である。（推定、要確認）
        ///     dimBit
        /// 
        /// *filesize = size+8 = dataSize+8
        /// 
        /// </summary>
        public struct WavHeader
        {
            public byte[] riffID;       // (固定)"riff"
            public uint size;           // ファイルサイズ-8, Typically, you'd fill this in after creation.
            public byte[] wavID;        // (固定)"WAVE"
            public byte[] fmtID;        // (固定)"fmt "
            public uint fmtSize;        // (固定)fmtチャンクのバイト数, Length of format data.  Always 16
            public ushort format;       // (固定)フォーマット, Wave type PCM mustbe 1
            public ushort channels;     // (固定)チャンネル数 = 2(stereo)
            public uint sampleRate;     // (固定)サンプリングレート = 44100(CD基準)
            public uint bytePerSec;     //（相関）データ速度 = 176400 = SampleRate * 4
            public ushort blockSize;    //（相関）ブロックサイズ = 4
            public ushort dimBit;       // 量子化ビット数(Bits per sample) = 16
            public byte[] dataID;       // (固定)"data"
            public uint dataSize;       // 波形データのバイト数, ファイルサイズ-44

            // bytePerSec := (sampleRate * dimBit * channels) / 8
            // blockSize  := (BitsPerSample * Channels) / 8
        }
        public struct DataList
        {
            public List<short> lDataList;
            public List<short> rDataList;
            public WavHeader WavHeader;

            public DataList(List<short> lDataList, List<short> rDataList, WavHeader WavHeader)
            {
                this.lDataList = lDataList;
                this.rDataList = rDataList;
                this.WavHeader = WavHeader;
            }
        }

        /// <summary>
        /// このメソッドでは外部からでも呼び出せるように、静的とする
        /// 処理の中断を明確にするために、状態を保存、のちにラベル表示できる。（未）
        /// readerとwriterは分ける。（未）
        /// 静的なメソッドでは、メソッド外とのオブジェクト参照は禁止される（要出典）
        /// 
        /// このrederとwriterに求めることは、新たなwavファイルの生成
        /// </summary>
        /// <param name="args">
        /// 入力側のWaveファイル
        /// ここでの処理ではリニアPCMであるWaveファイルしか処理されません。
        /// </param>
        /// <param name="fileout">
        /// デフォルトのヘッダー情報の保存先
        /// if分岐が無効であれば適当な文字列でもよいです。
        /// </param>
        public static DataList WavReader(string args, string fileout)
        {
            WavHeader Header = new WavHeader();
            List<short> lDataList = new List<short>();
            List<short> rDataList = new List<short>();

            using (FileStream fs = new FileStream(args, FileMode.Open, FileAccess.Read))
            using (System.IO.BinaryReader br = new BinaryReader(fs))
            {
                try
                {
                    Header.riffID = br.ReadBytes(4);
                    Header.size = br.ReadUInt32();
                    Header.wavID = br.ReadBytes(4);
                    Header.fmtID = br.ReadBytes(4);
                    Header.fmtSize = br.ReadUInt32();
                    Header.format = br.ReadUInt16();
                    if (Header.format != 1) // 入力がリニアPCM出ない時のダミー操作
                    {
                        WavHeader dm = new WavHeader();
                        DataList dmd = new DataList(lDataList, rDataList, dm);
                        return dmd;
                    }
                    Header.channels = br.ReadUInt16();
                    Header.sampleRate = br.ReadUInt32();
                    Header.bytePerSec = br.ReadUInt32();
                    Header.blockSize = br.ReadUInt16();
                    Header.dimBit = br.ReadUInt16();
                    Header.dataID = br.ReadBytes(4);
                    Header.dataSize = br.ReadUInt32();

                    for (int i = 0; i < Header.dataSize / Header.blockSize; i++)
                    {
                        lDataList.Add((short)br.ReadUInt16());
                        rDataList.Add((short)br.ReadUInt16());
                    }
                }
                finally
                {
                    if (br != null) br.Close();
                    if (fs != null) fs.Close();
                }
            }


            // trueなら、header情報の出力
            if (true)
            {
                string tmp;
                StreamWriter kekkaout = new StreamWriter(fileout);
                tmp = System.Text.Encoding.GetEncoding("shift_jis").GetString(Header.riffID);
                kekkaout.WriteLine("riffID : " + tmp);
                kekkaout.WriteLine(Header.size);
                tmp = System.Text.Encoding.GetEncoding("shift_jis").GetString(Header.wavID);
                kekkaout.WriteLine("wavID : " + tmp);
                tmp = Encoding.GetEncoding("shift_jis").GetString(Header.fmtID);
                kekkaout.WriteLine("fmtID : " + tmp);
                kekkaout.WriteLine(Header.fmtSize);
                kekkaout.WriteLine(Header.format);
                kekkaout.WriteLine(Header.channels);
                kekkaout.WriteLine(Header.sampleRate);
                kekkaout.WriteLine(Header.bytePerSec);
                kekkaout.WriteLine(Header.blockSize);
                kekkaout.WriteLine(Header.dimBit);
                tmp = Encoding.GetEncoding("shift_jis").GetString(Header.dataID);
                kekkaout.WriteLine("dID : " + tmp);
                kekkaout.WriteLine(Header.dataSize);
                kekkaout.Close();
            }

            DataList datalist = new DataList(lDataList, rDataList, Header);
            return datalist;

        }
        public static void WavWriter(string args, DataList datalist)
        {

            List<short> lNewDataList = datalist.lDataList;
            List<short> rNewDataList = datalist.rDataList;
            WavHeader Header = datalist.WavHeader;

            Header.dataSize = (uint)Math.Max(lNewDataList.Count, rNewDataList.Count) * 4;

            using (System.IO.FileStream fs = new FileStream(args, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                try
                {
                    bw.Write(Header.riffID);
                    bw.Write(Header.size);
                    bw.Write(Header.wavID);
                    bw.Write(Header.fmtID);
                    bw.Write(Header.fmtSize);
                    bw.Write(Header.format);
                    bw.Write(Header.channels);
                    bw.Write(Header.sampleRate);
                    bw.Write(Header.bytePerSec);
                    bw.Write(Header.blockSize);
                    bw.Write(Header.dimBit);
                    bw.Write(Header.dataID);
                    bw.Write(Header.dataSize);

                    for (int i = 0; i < Header.dataSize / Header.blockSize; i++)
                    {
                        // 1st IF turning point
                        if (i < lNewDataList.Count)
                        {
                            bw.Write((ushort)lNewDataList[i]);
                        }
                        else
                        {
                            bw.Write(0);
                        }

                        // 2st IF turning point
                        if (i < rNewDataList.Count)
                        {
                            bw.Write((ushort)rNewDataList[i]);
                        }
                        else
                        {
                            bw.Write(0);
                        }
                    }
                }
                finally
                {
                    if (bw != null) bw.Close();
                    if (fs != null) fs.Close();
                }
            }
        }
    }
}
