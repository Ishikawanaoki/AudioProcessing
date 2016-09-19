using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
namespace function
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
        public Axis(double sample_value, double sampling_frequency)
        {
            time = 1 / sampling_frequency;
            frequency = sampling_frequency / sample_value;
        }
        public double[] get_div()
        {
            double[] axis = new double[2];
            axis[0] = time; axis[1] = frequency;
            return axis;
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
    public class myfunction
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static IEnumerable<double> seikika(double[] y)
        {
            double seikika = y.Max() - y.Min();
            foreach (double item in y)
               yield return (item / seikika * 100);
        }
        public static double[] DoDFT(double[] y) //本体
        {
            ActiveComplex acmp = new ActiveComplex(y);
            acmp.FTransform(Fourier.ComplexFunc.DFT);
            return acmp.GetMagnitude().ToArray();
        }
        public static Complex[] Manual_DoDFT(double[] y) //本体
        {
            ActiveComplex acmp = new ActiveComplex(y);
            return acmp.FTransform(Fourier.ComplexFunc.DFT);
        }
        public static IEnumerable<double> DoIDFT(Complex[] sign) //本体
        {
            ActiveComplex acmp = new ActiveComplex(sign);
            Complex[] items = acmp.FTransform(Fourier.ComplexFunc.IDFT);
            foreach (Complex item in items)
                yield return item.real;
        }
        public IEnumerable<short> Do_s_IDFT(Complex[] sign) //本体
        {
            ActiveComplex acmp = new ActiveComplex(sign);
            Complex[] items = acmp.FTransform(Fourier.ComplexFunc.IDFT);
            foreach (Complex item in items)
                yield return (short)item.real;
        }
        public static double[] DoFFT(double[] y) //本体
        {
            ActiveComplex acmp = new ActiveComplex(y, Fourier.WindowFunc.Hamming);
            acmp.FTransform(Fourier.ComplexFunc.FFT);
            return acmp.GetMagnitude().ToArray();
        }
        public static Complex[] Manual_DoFFT(double[] y) //本体
        {
            ActiveComplex acmp = new ActiveComplex(y, Fourier.WindowFunc.Hamming);
            return acmp.FTransform(Fourier.ComplexFunc.FFT);
        }
    }
    public class ActiveComplex
    {
        private List<Complex> complex;
        public ActiveComplex(double[] items)
        {
            complex = new List<Complex>();
            Add(items);
        }
        public ActiveComplex(Complex[] items)
        {
            complex = new List<Complex>();
            complex.AddRange(items);
        }
        public ActiveComplex(double[] items, Fourier.WindowFunc wfunc)
        {
            complex = new List<Complex>();
            AddwithWindow(items, wfunc);
        }
        private void Add(double[] items)
        {
            foreach (double item in items)
            {
                Complex tmp = new Complex(item, 0);
                complex.Add(tmp);
            }
        }
        private void AddwithWindow(double[] items, Fourier.WindowFunc wfunc)
        {
            double[] items2 = Fourier.Windowing(items, wfunc);
            foreach (double item in items2)
            {
                Complex tmp = new Complex(item, 0);
                complex.Add(tmp);
            }
        }
        /// <summary>
        /// 振幅スペクトル列を返す
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> GetMagnitude()
        {
            foreach (Complex item in complex)
                yield return item.magnitude;
        }
        /// <summary>
        /// 1つの切り取り時間において、振幅スペクトルをランク付けする
        /// </summary>
        /// <param name="rank">上位第rank位までのスペクトルを採用する</param>
        /// <returns>タップルには 
        /// item1 : ans : 
        /// item2 : 
        /// が取り出せるようにしている。
        /// </returns>
        public Tuple<double[], List<LinkedList<int>>> Ranked(int rank)
        {
            List<double> ans = new List<double>(); // スペクトルの大きさを上位rankまでrank個だけ格納
            List<LinkedList<int>> ansIndex = new List<LinkedList<int>>(); // List.Count=rank. LinkedList.Count<=2
            IEnumerable<double> str = GetMagnitude();

            #region 配列ansの決定
            for (int i=0; i<rank; i++)
            {
                double[] Imax = new double[1];  //要素数1の配列を用意
                Imax[0] = str.Max();            //最大値
                ans.AddRange(Imax);             //最大の振幅スペクトルを格納
                str.Except(Imax);               //最大の振幅スペクトルを除外
            }
            #endregion

            double[] str2 = GetMagnitude().ToArray();
            for(int ii=0; ii<str2.Length; ii++)
            {
                LinkedList<int> tmp = new LinkedList<int>();
                foreach(double RankedValue in ans)
                {
                    if (str2[ii] == RankedValue)
                        tmp.AddLast(ii); // 周波数軸上の位置に該当する、スペクトル位置を格納
                }
                ansIndex.Add(tmp);       // 短時間単位に、複数の周波数軸データを格納
            }
            return Tuple.Create(ans.ToArray(), ansIndex);
        }
        /// <summary>
        /// complexObj : 上位rank位までのスペクトルの大きさ、その位置
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        public IEnumerable<double> RankedMagnitude(int rank)
        {
            Tuple<double[], List<LinkedList<int>>> complexObj = Ranked(rank);
            double maxThrethold = complexObj.Item1[complexObj.Item1.Length - 1];
            foreach (double magnitude in GetMagnitude())
            {
                if (magnitude > maxThrethold)
                    yield return magnitude;
            }
        }
        /// <summary>
        /// 正方向、逆方向でのフーリエ解析呼び出し。
        /// 呼出しと共に、フィールド outbox は生成し直す。
        /// </summary>
        /// <param name="cfunc"></param>
        /// <returns></returns>
        public Complex[] FTransform(Fourier.ComplexFunc cfunc)
        {
            complex = new List<Complex>(Fourier.FTransform(complex.ToArray(), cfunc));
            return complex.ToArray();
        }
        public double[] FunctionTie()
        {
            FTransform(Fourier.ComplexFunc.FFT);
            FTransform(Fourier.ComplexFunc.IFFT);
            return GetMagnitude().ToArray();
        }
    }
    /// <summary>
    /// 複素数を実部虚部に分けて格納する構造体です。
    /// フーリエ変換後の値を扱います。
    /// </summary>
    public class Complex
    {
        public double real = 0.0;
        public double img = 0.0;

        /// <summary>
        /// フィールドへの初期化でず。
        /// </summary>
        /// <param name="real">実部です。</param>
        /// <param name="img">虚部です。</param>
        public Complex(double real, double img)
        {
            this.real = real;
            this.img = img;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        override public string ToString()
        {
            string data = real.ToString() + "+" + img.ToString() + "i";
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
            Complex data = new Complex(a.real + b.real, a.img + b.img);
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
            Complex data = new Complex(a.real - b.real, a.img - b.img);
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
            Complex data = new Complex((a.real * b.real) - (a.img * b.img),
           (a.real * b.img + (a.img * b.real)));
            return data;
        }
        /// <summary>
        /// 振幅スペクトル
        /// </summary>
        public double magnitude
        {
            get
            {
                return Math.Sqrt(Math.Pow(real, 2) + Math.Pow(img, 2));
            }
        }
        /// <summary>
        /// 位相スペクトル
        /// </summary>
        public double phase
        {
            get
            {
                return Math.Atan(img / real); // アークタンジェントを返し、-n/2<=theta<=n/2となる値を返す
            }
        }
        public Complex ChangeToConjugate()
        {
            return new Complex(
                this.real, this.img * (-1));
        }
    }
    /// <summary>
    ///  + enum WindowFunc : 窓関数オプション
    ///  + double[] Windowing(double[] data, WindowFunc windowFunc)
    ///  + Complex[] DFT(Complex[] x)
    /// </summary>
    public static class Fourier
    {
        public enum ComplexFunc
        {
            DFT,
            IDFT,
            FFT,
            IFFT,
            STDFT
        }
        public enum WindowFunc
        {
            Hamming,
            Hanning,
            Blackman,
            Rectangular
        }
        /// <summary>
        /// 列挙帯により異なる方法で窓関数を働きかけます。
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
        public static Complex[] FTransform(Complex[] x, ComplexFunc func)
        {
            switch (func)
            {
                case ComplexFunc.DFT:
                    return DFT(x);
                case ComplexFunc.IDFT:
                    return IDFT(x);
                case ComplexFunc.FFT:
                    return FFT(x);
                case ComplexFunc.IFFT:
                    return IFFT(x);
            }
            throw new ArgumentNullException();
        }
        /// <summary>
        /// 離散フーリエ変換
        /// 回転子 double型 d_theta
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static Complex[] DFT(Complex[] x)
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
        private static Complex[] IDFT(Complex[] x)
        {
            int N = x.Length;
            Complex[] X = new Complex[N];
            double d_theta = //(-2) * Math.PI / N;
                2 * Math.PI / N;

            // 以下、配列計算
            for (int k = 0; k < N; k++)
            {
                X[k] = new Complex(0, 0);
                for (int n = 0; n < N; n++)
                {
                    Complex temp = Complex.from_polar(1, d_theta * n * k);
                    temp *= x[n]; //演算子 * はオーバーライドしたもの
                    X[k] += temp; //演算子 + はオーバーライドしたもの
                }
                X[k].real /= N;
                X[k].img /= N;
            }
            return X;
        }
        private static int EnableLines(int length)
        {
            int LineValidCount = 1;
            while (length >= LineValidCount) LineValidCount *= 2;
            LineValidCount /= 2;
            return LineValidCount;
        }
        /// <summary>
        /// 高速フーリエ変換
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        private static Complex[] FFT(Complex[] x)
        {
            //初期宣言
            int N = EnableLines(x.Length);
            Complex[] X = new Complex[N];
            Complex[] d, D, e, E;
            //例外処理
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
            double d_theta = (-2) * Math.PI / N;
            for (k = 0; k < N / 2; k++)
            {
                //Complex temp = Complex.from_polar(1, -2 * Math.PI * k / N);
		        // k means -2*pi*( k / N ); k = 0 - N/2
		        // Exp(jm)*Exp(jn) 
		        // = Exp(j(m+n))

		        // 複素数を複素単位円上にあると考えると、偏角の任意自然数倍*nの意味
		        // ここでは回転子の意味
		        // fft の再帰呼び出しの式及び他者コードを比較する上では
		        // 引数の偶数と奇数に分けたD,E及びそれ以下の再帰における挙動を
		        // 確認する
                Complex temp = Complex.from_polar(1, d_theta * k);
                D[k] *= temp;
            }
            for (k = 0; k < N / 2; k++)
            {
		        // 偶数
                X[k] = E[k] + D[k];
		
                X[k + N / 2] = E[k] - D[k];
            }
            return X;
        }
        private static Complex[] IFFT(Complex[] x)
        {
            List<Complex> y = new List<Complex>();

            // Complex Conjugat to y
            foreach (Complex item in x)
            {
                y.Add(item.ChangeToConjugate());
            }
            // FFT to x
            x = FFT(y.ToArray());
            int Nmax = x.Length;

            // To get Complex Conjugat and to get magnitude : to y
            y.Clear();
            foreach (Complex item in x)
            {
                y.Add(
                    new Complex(
                        item.real / Nmax,
                        item.img  / Nmax
                        )
                );
            }
            return y.ToArray();
        }
    }
    public class WaveReAndWr
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
        /// その為ファイルを更新したい場合には次のメンバのみが独立で、変更可能である。（推定、要確認）
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
        /// <summary>
        /// 
        /// </summary>
        public struct DataList<T>
        {
            public List<T> lDataList;
            public List<T> rDataList;
            public WavHeader WavHeader;

            public DataList(List<T> lDataList, List<T> rDataList, WavHeader WavHeader)
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
        /// このrederとwriterに求めることは、新たなwavファイルの生成
        /// </summary>
        /// <param name="args"></param>
        /// 入力側のWaveファイル
        /// ここでの処理ではリニアPCMであるWaveファイルしか処理されません。
        /// <param name="fileout"></param>
        /// デフォルトのヘッダー情報の保存先
        /// if分岐が無効であれば適当な文字列でもよいです。
        /// <param name="flag"></param>
        /// <returns></returns>
        public static DataList<short> WavReader(string args, string fileout, Boolean flag)
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
                        DataList<short> dmd = new DataList<short>(lDataList, rDataList, dm);
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
            if (flag)
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

            DataList<short> datalist = new DataList<short>(lDataList, rDataList, Header);
            return datalist;

        }
        /// <summary>
        /// ﾊﾞｲﾅﾘ書き込み先 args、DataListを引数
        /// </summary>
        /// <param name="args"></param>
        /// <param name="datalist"></param>
        public static void WavWriter(string args, DataList<short> datalist)
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
        /// <summary>
        /// バイナリファイルの行数を取得し、
        /// その値以下の最大の2の乗数を返します。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
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
            return LineValidCount;
        }   
    }
    public class File
    {
        /// <summary>
        /// バイナリファイル fileName を読み込み、1行ずつ読み込み
        /// その値をdouble配列で返却します。
        /// データ列は最大値と比べたパーセント率を示します。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static double[] includeFile(string fileName)
        {
            double amin, amax;

            //signal
            List<double> y = new List<double>();

            using (System.IO.StreamReader koeFile = new System.IO.StreamReader(fileName))
            {
                while (koeFile.Peek() != -1)
                {
                    y.Add(Convert.ToDouble(koeFile.ReadLine()));
                }
            }
            amax = y.Max();
            amin = y.Min();
            if (amin < 0) amax += amin * (-1);
            for (int i = 0; i < y.Count; i++)
            {
                y[i] = y[i] / amax * 100;
            }
            return y.ToArray();
        }
    }
}