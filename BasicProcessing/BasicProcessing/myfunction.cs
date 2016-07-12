﻿using System;
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
    /// グラフ表示する際の軸データを内部的に演算しています。
    /// 全て、double型です。
    /// はじめに、コンストラクタにより、標本数と標本化周波数を引数にして、
    /// フィールドにはそれぞれの軸の一目盛り分の値が格納されてます。
    /// </summary>
    public class Axis
    {
        public double time;        // 時間軸領域の目盛り
        public double frequency;   // 周波数軸領域の目盛り
        public Axis(int sample_value, int sampling_frequency)
        {
            time = 1 / sampling_frequency;
            frequency = sampling_frequency / sample_value;
        }
        public void doubleAxie(ref double[] x)
        {
            x[0] = frequency;
            for (int i = 1; i < x.Length; i++)
                x[i] = x[i - 1] + frequency;
        }
        public void strighAxie(ref string[] x)
        {
            int dimF = (int)frequency;
            int[] x2 = new int[x.Length];
            x2[0] = dimF;
            x[0] = dimF.ToString();
            for (int i = 1; i < x.Length; i++)
            {
                x2[i] = x2[i - 1] + dimF;
                x[i] = x2[i].ToString();
            }
        }
        public void intAxis(ref int[] x)
        {
            int dimF = (int)frequency;
            x[0] = dimF;
            for (int i = 1; i < x.Length; i++)
                x[i] = x[i - 1] + dimF;
        }
    }
    /// <summary>
    /// 内部処理決定部
    /// (主要)
    /// </summary>
    class myfunction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double[] seikika(double[] y)
        {
            double min = 0;
            double max = 0;
            foreach (double x in y)
            {
                if (min > x) min = x;
                if (max < x) max = x;
            }
            max = max - min;
            int i = 0;
            foreach (double x2 in y)
            {
                //x2 = x2 / max * 100;]
                y[i++] = x2 / max * 100;
            }
            return y;
        }
        /// <summary>
        /// DFTを実行し、そしてグラフ描写をするテストクラスです
        /// <para name = "sign">時間信号の複素数列</para>
        /// <para name = "do_dft">結果である複素数列</para>
        /// </summary>
        public static double[] DoDFT(double[] y) //本体
        {
            Console.WriteLine("離散フーリエ変換を開始します");
            Complex[] sign = new Complex[y.Length];
            Complex[] do_dft = new Complex[y.Length];

            double[] y_out = new double[y.Length];

            for (int i = 0; i < y.Length; i++)
            {
                sign[i] = new Complex(y[i], 0);
            }

            do_dft = Fourier.DFT(sign);

            for (int ii = 0; ii < y.Length; ii++)
            {
                y_out[ii] = do_dft[ii].magnitude;
                y_out[ii] = Math.Log10(y_out[ii]) * 10;
            }
            Console.WriteLine("離散フーリエ変換を修了します");
            return y_out;
        }
        public static double[] DoFFT(double[] y) //本体
        {
            // 要素数をチェックします。
            int Lines = EnableLines(y.Length);
            Console.WriteLine("要素数をチェックしました。\ndT = {0}\ndF = {1}",44100.0 / y.Length, 1 / 44100.0);

            Console.WriteLine("高速フーリエ変換を開始します");
            Complex[] sign = new Complex[Lines];
            Complex[] do_dft = new Complex[Lines];

            double[] y_out = new double[Lines];

            for (int i = 0; i < Lines; i++)
                sign[i] = new Complex(y[i], 0);


            do_dft = Fourier.FFT(sign);

            for (int ii = 0; ii < Lines; ii++)
            {
                y_out[ii] = do_dft[ii].magnitude;
                y_out[ii] = Math.Log10(y_out[ii]) * 10;
            }
            Console.WriteLine("高速フーリエ変換を修了します");

            return y_out;
        }
        private static int EnableLines(int length)
        {
            int LineValidCount = 1;
            while (length >= LineValidCount) LineValidCount *= 2;
            LineValidCount /= 2;
            Console.WriteLine("ﾌｧｲﾙ内行数 = {0}\n有効行数 = {1}", length, LineValidCount);
            return LineValidCount;
        }
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
            if (false)
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
        public static double[] includeFile(string fileName)
        {
            string buf;
            double aver, amax, sum, aby, seikika;
            aver = 0; amax = 0; sum = 0; aby = 0; seikika = 0;
            int Nmax = GetLinesOfTextFile(fileName);

            //signal
            double[] y = new double[Nmax];


            System.IO.StreamReader koeFile = new System.IO.StreamReader(fileName);

            for (int i = 0; i < Nmax; i++)
            {
                if (koeFile.Peek() == -1)    // ファイルの最後で有れば -1 を返す
                    break;

                buf = koeFile.ReadLine();

                y[i] = Convert.ToDouble(buf);

                sum += y[i];
                if (amax < y[i]) amax = y[i];
            }
            koeFile.Close();
            aver = sum / Nmax; sum = 0;
            aby = amax;
            for (int i = 0; i < Nmax; i++)
            {
                y[i] -= aver;
                sum += y[i]; //　この最大値は、平均値を除去した後のもの【使わない】
                if (aby > y[i]) aby = y[i];
            }
            seikika = aby * (-1);
            if (seikika < amax) seikika = amax;
            // seikika は正規化をするために、信号値の絶対値の最大値を格納
            for (int i = 0; i < Nmax; i++)
            {
                y[i] = y[i] / seikika * 100;
            }
            return y;
        }
        public static int GetLinesOfTextFile(string fileName)
        {
            StreamReader StReader = new StreamReader(fileName);
            int LineCount = 0; int LineValidCount = 2;
            while (StReader.Peek() >= 0)
            {
                StReader.ReadLine();
                LineCount++;
            }
            StReader.Close();
            while (LineCount >= LineValidCount) LineValidCount *= 2;
            LineValidCount /= 2;
            Console.WriteLine("ﾌｧｲﾙ内行数 = {0}\n有効行数 = {1}", LineCount, LineValidCount);
            return LineValidCount;
        }
    }
}
