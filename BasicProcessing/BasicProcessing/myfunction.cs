using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms.DataVisualization.Charting;

/// <summary>
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
        public double time;        // 時間軸領域の1目盛り
        public double frequency;   // 周波数軸領域の1目盛り
        public Axis(double sample_value, double sampling_frequency)
        {
            time = 1 / sampling_frequency;
            frequency = sampling_frequency / sample_value;
        }
        public static double GetFre(double sample_value)
        {
            int fs = 44100;
            return fs / sample_value;
        }
        /// <summary>
        /// axis[0] = time
        /// axis[1] = frequency;
        /// </summary>
        /// <returns>
        /// </returns>
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
    /// スペクトル解析の第一段階
    /// 短時間に分割する。
    /// </summary>
    public class ComplexStaff
    {
        private readonly double[] rawSign;   // 変換前の波形データ（全体）
        public int shortLength;    // 短時間に対応するデータ数
        public int fs = 44100;
        public int mergin = 50; // 短時間窓の重なり(%)
        #region public
        public ComplexStaff(int dividedNum, double[] rawSign)
        {
            this.rawSign = rawSign;

            if (dividedNum > 0)
                shortLength = rawSign.Length / dividedNum;
        }
        /// <summary>
        /// 常に任意時間の時間窓を切り出す
        /// </summary>
        /// <param name="sec"></param>
        public void setTimeDistance(double sec)
        {
            shortLength = (int)(fs * sec);
            if (shortLength <= 0) shortLength = rawSign.Length;
        }
        public double[][] GetHertz(int[] rank)
        {
            // double[] をクエリして、IEnumerable<double[]>を生成
            return Enumerable
                .Range(0, rawSign.Length % shortLength)
                .Select((_, gindex) =>
                    ShortTimeRankedHeldz(rank, gindex))
                .ToArray();
        }
        public double[][] GetMusicalNote(int[] rank)
        {
            return Enumerable
                .Range(0, rawSign.Length % shortLength)
                .Select((_, gindex) =>
                    ShortTimeRankedNote(rank, gindex))
                .ToArray();
        }
        #endregion
        /// <summary>
        /// shortLength個のデータを、配列shortSignへ割り当てる。
        ///  + マージンを考慮し、時間的重なりを追加。
        ///  0 < mergin
        ///  
        /// </summary>
        /// <param name="groupIndex"></param>
        /// <param name="sign"></param>
        private double[] AssignSignalInSame(int groupIndex)
        {
            if(groupIndex < 1) return new double[0];

            if (groupIndex == 1)
            {
                return rawSign.Take(shortLength).ToArray();
            }
            else
            {
                return rawSign.Skip(shortLength * (groupIndex - 1) + shortLength / 100 * mergin)
                    .Take(shortLength).ToArray();
            }
        }
        private double[] ShortTimeRankedHeldz(int[] rank, int groupIndex)
        {
            ActiveComplex ac = new ActiveComplex(AssignSignalInSame(groupIndex), Fourier.WindowFunc.Hamming);

            if (ac.isContain())
            {
                ac.FTransform(Fourier.ComplexFunc.FFT);
                return ac.GetHertz(rank).ToArray();

            }
            else
            {
                return new double[rank.Length];
            }
        }
        private IEnumerable<Tuple<int, double>> GetTuple(double[] x)
        {
            foreach(var item in x.Select((val, i) => new {Val =val, Index = i }))
            {
                yield return Tuple.Create(item.Index, item.Val);
            }
        }
        private IEnumerable<int> GetOrderedIndex(double[] x)
        {
            var tuple = GetTuple(x);
            var ordered = tuple.OrderByDescending(c => c.Item2);
            return ordered.Select(c => c.Item1);
        }
        private double[] ShortTimeRankedNote(int[] rank, int groupIndex)
        {
            ActiveComplex ac = new ActiveComplex(AssignSignalInSame(groupIndex), Fourier.WindowFunc.Hamming);

            if (ac.isContain())
            {
                ac.MusucalTransform();
                double[] mag = ac.GetMagnitude().ToArray();

                int[] orderdIndex = GetOrderedIndex(mag)
                    .Take(rank.Max())
                    .Skip(rank.Min())
                    .ToArray();

                return orderdIndex.Select(c => 27.5 * Math.Pow(2, c / 12))
                    .Select(c => otherUser.Music.OneMusicalScale(c))
                    .ToArray();

            }
            else
            {
                return new double[rank.Length];
            }
        }
        
        static class TimeDomain
        {
            private static IEnumerable<int> IndexOfChangepoint(double[] rawSign)
            {
                // 全体の5/100以下の微弱な変化は無視する
                int c_counter = 0;
                for (int i = 0; i < rawSign.Length; i++)
                {
                    // first step is skipped
                    if (i == 0) { c_counter++; continue; }
                    // change flag with comparing
                    if (rawSign[i] * rawSign[i - 1] <= 0 && c_counter > rawSign.Length/20)
                    {
                        c_counter = 0;
                        yield return Math.Abs(rawSign[i]) > Math.Abs(rawSign[i - 1]) ? (i - 1) : i;
                    }
                }
            }
            public static IEnumerable<double> GetFixWave(double[] rawSign, int groupIndex)
            {
                if (IndexOfChangepoint(rawSign).Count() < groupIndex) return new double[0];

                int start = groupIndex == 0 ? 0 :IndexOfChangepoint(rawSign).ElementAt(groupIndex-1);
                int end = groupIndex == 0 ? IndexOfChangepoint(rawSign).ElementAt(groupIndex) 
                    : IndexOfChangepoint(rawSign).ElementAt(groupIndex);
                return rawSign.Where((_, index) => index >= start && index <= end);
            }
            public static int GetEfficientCount(double[] rawSign)
            {
                return IndexOfChangepoint(rawSign).Count();
            }
        }
    }

    /// <summary>
    /// Copmlex, Fourier クラスの呼び出しを統括する意図で定義
    /// フィールドにComplexリストを唯一保持して、なおかつその変更はメソッド呼出しにのみ有効
    /// 
    /// (前処理)帯域制限、可変的短時間 の実装
    /// (1)GetMagnitude 振幅スペクトル取得
    ///    GetReality 余弦成分(double)取得
    /// (2)GetRanked_Maximums 任意の順位に相当する最大の「振幅スペクトル」を単数、または複数取得
    /// (3)GetRanked_Index 「最大の振幅スペクトル」を示すスペクトルの内、正で最も低い周波数へのインデックス
    ///     => 短時間での、「任意の順位」にある「最大の振幅スペクトル」の最も低い周波数を活用
    /// (4)GetHertz データ数とサンプリングレート(規定値44100Hz)より、「(3)のインデックス」から周波数を求める
    ///             また、求めた周波数に対して、最も近い音階(12音律)に直す
    ///             
    /// 但し、(3)での「最も低い（低い段階で出現する）周波数」は作為的な選択であり、
    /// 「振幅スペクトルの内n番目に大きいもの」は一つの短時間において2つ以上必ず存在するため、
    /// '(3)GetRanked_indexies, '(4)GetHertzsを定義
    /// 
    /// </summary>
    public class ActiveComplex
    {
        private Complex[] complex;
        public int Getlength()
        {
            return complex.Length;
        }
        public ActiveComplex(double[] items)
        {
            complex = items.Select(c => {return new Complex(c, 0);}).ToArray();
        }
        public ActiveComplex(double[] items, Fourier.WindowFunc wfunc)
        {
            complex = Fourier.Windowing(items, wfunc).Select(c => { return new Complex(c, 0); }).ToArray();
        }
        public ActiveComplex(Complex[] items)
        {
            complex = items;
        }
        /// <summary>
        /// 振幅スペクトル列を返す
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> GetMagnitude()
        {
            return complex.Select(c => c.magnitude);
        }
        /// <summary>
        /// 余弦成分の取得
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> GetReality()
        {
            return complex.Select(c => c.real);
        }
        public bool isContain()
        {
            return complex.Length != 0;
        }
        /// <summary>
        /// return IEnumerable contains a num of rank lines
        /// 入力されたランクと、等しい個数の最大値を求めるシーケンスを返す。
        /// </summary>
        /// <param name="rank">順位を意味するint配列</param>
        /// <returns></returns>
        public IEnumerable<double> GetRanked_Muximums(int[] rank)
        {
            int countup = 1;
            double max = double.MaxValue;
            var query = GetMagnitude();
            if (!(query.Count() > 0))
                yield break;
            else
            {
                foreach (var tmp in rank.OrderBy(t => t)) //昇順に並び替える
                {
                    while (countup <= tmp)
                    {
                        if (max == 0) max = double.MaxValue;
                        max = query.Where(c => c < max).Max();
                        countup++;
                    }
                    yield return max;
                }
            }
        }
        /// <summary>
        /// 一つの時間窓での任意の順位に対する、インデックスを先頭から検索
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        /// 
        public IEnumerable<int> GetRanked_Index(int[] rank)
        {
            foreach(var value in GetRanked_Muximums(rank))
            {
                yield return GetMagnitude()
                    .Select((val, index) => // 振幅スペクトルから値 nameと、indexを順次に取り出す
                    {
                        if (val == value) return index;             // 任意順位 str に該当
                        else            return -1;               // 該当なし
                    })
                    .Where(c => c > 0)                // 該当なしを弾く
                    .FirstOrDefault();                 // 最初の対象を
                                                       // シーケンスで返す
                
            }
        }
        /*public IEnumerable<int> GetRanked_Indexies(int rank)
        {
            int[] tmp = new int[1];tmp[0] = rank;
            double value = GetRanked_Muximums(tmp).FirstOrDefault();
            return GetMagnitude().Select((val, index) =>
            {
                if (value == val) return index;
                else return -1;
            }).Where(c => c > 0);

        }*/
        public IEnumerable<Complex> RankedComplex(int[] rank)
        {
            return complex.Select((name, index) => 
            {
                bool flag = false;
                foreach(var str in GetRanked_Index(rank))
                {
                    if (index == str)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag) return name;
                else return new Complex(0,0);
            });
        }
        /*public IEnumerable<Complex> RankedComplex(int rank)
        {
            return complex.Select((name, index) =>
            {
                bool flag = false;
                foreach (var str in GetRanked_Indexies(rank))
                {
                    if (index == str)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag) return name;
                else return new Complex(0, 0);
            });
        }
        */
        /// <summary>
        /// 配列rankに対する周波数のみを透過させるフィルタ
        /// </summary>
        /// <param name="rank"></param>
        /// <returns></returns>
        /*public IEnumerable<double> RankedMagnitude(int[] rank)
        {
            return GetMagnitude().Select((name, index) =>
            {
                bool flag = false;
                foreach (var str in GetRanked_Index(rank))
                {
                    if (index == str)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag) return name;
                else return 0.0;
            });
        }*/
        public IEnumerable<double> GetHertz(int[] rank)
        {
            double length = complex.Length; // 時間窓の大きさ
            Axis axis = new Axis(length, 44100); // 軸計算

            double axis_fru = axis.frequency; // グラフ : 周波数-振幅スペクトルでの周波数の一目盛り

            foreach (var item in GetRanked_Index(rank))
            {
                double tmp = item;
                if (item < length/2){ tmp *= axis_fru; }
                else{ tmp *= axis_fru * (-1); }
                yield return function.otherUser.Music.OneMusicalScale(tmp);
            }
        }
        /*public IEnumerable<double> GetHertzs(int rank)
        {
            double length = complex.Length; // 時間窓の大きさ
            Axis axis = new Axis(length, 44100); // 軸計算

            double axis_fru = axis.frequency; // グラフ : 周波数-振幅スペクトルでの周波数の一目盛り

            foreach (var item in GetRanked_Indexies(rank))
            {
                double tmp = item;
                if (item < length / 2) { tmp *= axis_fru; }
                else { tmp *= axis_fru * (-1); }
                yield return function.otherUser.Music.OneMusicalScale(tmp);
            }
        }
        */
        /// <summary>
        /// 正方向、逆方向でのフーリエ解析呼び出し。
        /// 呼出しと共に、フィールド outbox は生成し直す。
        /// </summary>
        /// <param name="cfunc"></param>
        /// <returns></returns>
        public Complex[] FTransform(Fourier.ComplexFunc cfunc)
        {
            complex = Fourier.FTransform(complex.ToArray(), cfunc);
            return complex;
        }
        public void MusucalTransform()
        {
            complex = Fourier.MusicalDFT(complex.ToArray());
        }
        public double[] FunctionTie()
        {
            FTransform(Fourier.ComplexFunc.FFT);
            FTransform(Fourier.ComplexFunc.IFFT);
            return GetMagnitude().ToArray();
        }
        private IEnumerable<double> OnlyReal(IEnumerable<Complex> cmp)
        {
            return cmp.Select(c => c.real);
        }
        public Tuple<double[],double[]>  HighPassDSP(int fr)
        {
            Complex[] converted =Fourier.FTransform(complex.ToArray(), Fourier.ComplexFunc.FFT);

            int target = Enumerable.Range(0, int.MaxValue).Where(c => fr <= Axis.GetFre(converted.Count()) * c).FirstOrDefault();
            
            Complex[] cutoff =  converted.Select((val, index) => {
                if (index <= target) return new Complex(0,0);
                else                return val;
            }).ToArray();

            //return OnlyReal(Fourier.IFFT(passed));
            return new Tuple<double[], double[]>(OnlyReal(converted).ToArray(), OnlyReal(cutoff).ToArray());
        }
        public Tuple<double[], double[]> LowPassDSP(int fr)
        {
            Complex[] converted = Fourier.FTransform(complex, Fourier.ComplexFunc.FFT);

            int target = Enumerable.Range(0, int.MaxValue).Where(c => fr <= Axis.GetFre(converted.Count()) * c).FirstOrDefault();

            Complex[] cutoff = converted.Select((val, index) => {
                if (index <= target) return val;
                else return new Complex(0, 0);
            }).ToArray();

            //return OnlyReal(Fourier.IFFT(passed));
            return new Tuple<double[], double[]>(OnlyReal(converted).ToArray(), OnlyReal(cutoff).ToArray());
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
            return new Complex(r * Math.Cos(radians), r * Math.Sin(radians));
        }
        public static Complex from_polar_times(double radians)
        {
            return new Complex(Math.Cos(radians), Math.Sin(radians));
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
        /// <summary>
        /// 複素共役を返す。
        /// </summary>
        /// <returns></returns>
        public Complex ChangeToConjugate()
        {
            return new Complex(real, img * (-1));
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
            IFFT
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
        public static IEnumerable<double> Windowing(IEnumerable<double> data, WindowFunc windowFunc)
        {
            int size = data.Count();
            return data.Select((val, index) =>
            {
                double winValu = 0.0;
                switch (windowFunc)
                {
                    case WindowFunc.Hamming:
                        winValu = 0.54 - 0.46 * Math.Cos(2 * Math.PI * index / (size - 1));
                        break;
                    case WindowFunc.Hanning:
                        winValu = 0.5 - 0.5 * Math.Cos(2 * Math.PI * index / (size - 1)); ;
                        break;
                    case WindowFunc.Blackman:
                        winValu = 0.42 - 0.5 * Math.Cos(2 * Math.PI * index / (size - 1))
                                    + 0.08 * Math.Cos(4 * Math.PI * index / (size - 1));
                        break;
                    case WindowFunc.Rectangular:
                        winValu = 1.0;
                        break;
                    default:
                        winValu = 1.0;
                        break;
                }
                return val * winValu;
            });
        }
        public static Complex[] MusicalDFT(Complex[] x)
        {
            int N = x.Length;
            double fA0 = 27.5; int num = 11;
            Complex[] X = new Complex[12 * num];
            double theta = (-2) * Math.PI;
            
            for (int k = 0; k < num * 12; k++)
            {
                X[k] = new Complex(0, 0);
                for (int n = 0; n < N; n++)
                {
                    Complex temp = Complex.from_polar(1, theta * n * Math.Pow(fA0, k / 12));
                    X[k] += temp * x[n];
                }
            }
            return X;
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
        public static IEnumerable<Complex> FTransForm(IEnumerable<Complex> x)
        {
            return IFFT(x);
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
            double d_theta = 2 * Math.PI / N;

            // 以下、配列計算
            for (int k = 0; k < N; k++)
            {
                X[k] = new Complex(0, 0);
                for (int n = 0; n < N; n++)
                {
                    Complex temp = Complex.from_polar(1, d_theta * n * k);
                    X[k] += temp * x[n]; //演算子 + はオーバーライドしたもの
                }
                X[k].real /= N;
                X[k].img /= N;
            }
            return X;
        }
        private static IEnumerable<Complex> AcIDFT(double start, bool upDown, Complex[] x)
        {
            int N = x.Length;
            double d_theta = 2 * Math.PI / N;

            return Enumerable.Range(0, N).Select((_,k) => {
                var query = x.Select((val, n) =>
                    Complex.from_polar(1, d_theta * n * k) * val);

                double real = query.Select(c => c.real).Sum();
                double img = query.Select(c => c.img).Sum();

                return new Complex(real, img);
            });
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
        public static IEnumerable<Complex> FFT(IEnumerable<Complex> x)
        {
            var start = DateTime.Now;


            int N = EnableLines(x.Count());

            var X = Enumerable.Range(0, N).Select((val, index) => {
                if (index == 0) return x.ElementAtOrDefault(0);
                else return new Complex(0, 0);
            });

            var e = Enumerable.Range(0, N / 2).Select((val, index) => x.ElementAtOrDefault(2 * index));
            var d = Enumerable.Range(0, N / 2).Select((val, index) => x.ElementAtOrDefault(2 * index + 1));

            var D = FFT(d);
            var E = FFT(e);

            double d_theta = (-2) * Math.PI / N;
            D = Enumerable.Range(0, N /2)
                .Select((val, index) => D.ElementAtOrDefault(index) * Complex.from_polar_times(d_theta * index));


            Console.WriteLine("N={0} : {1}", N, DateTime.Now - start);


            return Enumerable.Range(0, N / 2).Select((val, index) => {
                if (index < N / 2) return E.ElementAtOrDefault(index) + D.ElementAtOrDefault(index);
                else               return E.ElementAtOrDefault(index) - D.ElementAtOrDefault(index);
            });

        }
        public static IEnumerable<Complex> IFFT(IEnumerable<Complex> x)
        {
           var y =  x.Select(c =>{
                return c.ChangeToConjugate();
            });
            x = FFT(y);
            return x.Select(c => {
                return new Complex(c.real / x.Count(), c.img / x.Count());
            });
        }
        private static Complex[] IFFT(Complex[] x)
        {
            List<Complex> y = new List<Complex>();

            // Complex Conjugat to y
            foreach (Complex item in x)
            {
                // 複素共役
                y.Add(item.ChangeToConjugate());
            }
            // FFT to x
            x = FFT(y.ToArray());
            // 配列の要素数を有効にする
            int Nmax = x.Length;

            // To get Complex Conjugat and to get magnitude : to y
            y.Clear();
            foreach (Complex item in x)
            {
                y.Add(
                    new Complex(item.real / Nmax,　item.img  / Nmax)
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
                #region 読み込み作業
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
                #endregion
            }


            // trueなら、header情報の出力
            if (flag)
            {
                #region flag == true
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
                #endregion
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
                #region 書き込み作業
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
                #endregion
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
            //input Signal
            List<double> y = new List<double>();
            #region read data from file
            using (System.IO.StreamReader koeFile = new System.IO.StreamReader(fileName))
            {
                while (koeFile.Peek() != -1)
                {
                    y.Add(Convert.ToDouble(koeFile.ReadLine()));
                }
            }
            #endregion

            #region 正規化
            double amax = y.Max(); double amin = y.Min();
            if (amin < 0) amax += amin * (-1);
            for (int i = 0; i < y.Count; i++)
            {
                y[i] = y[i] / amax * 100;
            }
            #endregion

            return y.ToArray();
        }
        public Axis Plot(Chart str, double[] y, string area, string title)
        {
            string[] xValues = new string[y.Length / 2];

            function.Axis plot_axis = new function.Axis(y.Length, 44100);
            plot_axis.strighAxie(ref xValues);

            str.Titles.Clear();
            str.Series.Clear();
            str.ChartAreas.Clear();

            str.Series.Add(area);
            str.ChartAreas.Add(new ChartArea(area));            // ChartArea作成
            str.ChartAreas[area].AxisX.Title = "title";  // X軸タイトル設定
            str.ChartAreas[area].AxisY.Title = "[/]";  // Y軸タイトル設定

            str.Series[area].ChartType = SeriesChartType.Line;

            for (int i = 0; i < xValues.Length; i++)
            {
                DataPoint dp = new DataPoint();
                dp.SetValueXY(xValues[i], y[i]);  //XとYの値を設定
                dp.IsValueShownAsLabel = false;  //グラフに値を表示しないように指定
                str.Series[area].Points.Add(dp);   //グラフにデータ追加
            }

            return plot_axis;
        }
        public void APlot(Chart str, double[] y, string area, string title)
        {

            str.Titles.Clear();
            str.Series.Clear();
            str.ChartAreas.Clear();

            str.Series.Add(area);
            str.ChartAreas.Add(new ChartArea(area));            // ChartArea作成
            str.ChartAreas[area].AxisX.Title = "title";  // X軸タイトル設定
            str.ChartAreas[area].AxisY.Title = "[/]";  // Y軸タイトル設定

            str.Series[area].ChartType = SeriesChartType.Line;

            for (int i = 0; i < y.Length; i++)
            {
                DataPoint dp = new DataPoint();
                dp.SetValueXY(i.ToString(), y[i]);  //XとYの値を設定
                dp.IsValueShownAsLabel = false;  //グラフに値を表示しないように指定
                str.Series[area].Points.Add(dp);   //グラフにデータ追加
            }
        }
        public void FrequencyPlot(Chart str, double[] y, string area, string title, double fs)
        {

            str.Titles.Clear();
            str.Series.Clear();
            str.ChartAreas.Clear();

            str.Series.Add(area);
            str.ChartAreas.Add(new ChartArea(area));            // ChartArea作成
            str.ChartAreas[area].AxisX.Title = "title";  // X軸タイトル設定
            str.ChartAreas[area].AxisY.Title = "[f]";  // Y軸タイトル設定

            str.Series[area].ChartType = SeriesChartType.Line;

            int count = y.Length;
            for (int i = 0; i < count; i++)
            {
                DataPoint dp = new DataPoint(i*fs/count, y[i]);
                dp.IsValueShownAsLabel = false;  //グラフに値を表示しないように指定
                str.Series[area].Points.Add(dp);   //グラフにデータ追加
            }
        }
        /// <summary>
        /// 任意の時系列データdataを、
        /// 任意の出力先filenameへと保存する。
        ///  + データを任意整数倍に間引きすることで矩形波になると推測
        /// </summary>
        /// <param name="filename">保存ファイル名</param>
        /// <param name="Lindata">左</param>
        /// <param name="Rindata">右</param>
        /// <param name="times">間引きするデータ数</param>
        public static void Write(string filename, WaveReAndWr.DataList<double> dlist, int times)
        {
            if (times <= 0) return; // 中止
            int count = 0; // カウンタ変数
            short Ltmp = 0; short Rtmp = 0; // 間引きの時に書き出す、値を格納
            int size = dlist.rDataList.Count * times;
            List<short> Ldata = new List<short>();
            List<short> Rdata = new List<short>();
            for (int i = 0; i < size; i++)
            {
                if (i % times == 0)
                {
                    Ltmp = (short)dlist.lDataList[count];
                    Rtmp = (short)dlist.rDataList[count];
                }
                Ldata.Add(Ltmp); // キャスト代入
                Rdata.Add(Rtmp); // キャスト代入
            }
            // フィールド変数から、ヘッダーを参照しています。
            WaveReAndWr.DataList<short> datalist = new WaveReAndWr.DataList<short>(Ldata, Rdata, dlist.WavHeader);
            WaveReAndWr.WavWriter(filename, datalist);
        }
        public static WaveReAndWr.DataList<short> ConvertDoubletoShort(WaveReAndWr.DataList<double> dlist)
        {
            List<short> Ldata = new List<short>();
            List<short> Rdata = new List<short>();

            foreach (double str in dlist.lDataList) Ldata.Add((short)str);

            foreach (double str in dlist.rDataList) Ldata.Add((short)str);

            return new WaveReAndWr.DataList<short>(
                Ldata,
                Rdata,
                dlist.WavHeader
                );
        }
        public static WaveReAndWr.DataList<short> ConvertDoubletoShort
            (WaveReAndWr.DataList<double> dlist, int times)
        {
            int length =
                dlist.lDataList.Count > dlist.rDataList.Count
                ? dlist.rDataList.Count
                : dlist.lDataList.Count;
            length *= times;
                      
            List<short> Ldata = new List<short>();
            List<short> Rdata = new List<short>();

            short ltmp=0, rtmp=0;
            int count = 0;

            for(int i=0; i<length; i++)
            {
                if(i % times == 0)
                {
                    ltmp = (short)dlist.lDataList[count];
                    rtmp = (short)dlist.rDataList[count++];
                }

                Ldata.Add(ltmp); Rdata.Add(rtmp);
            }
            return new WaveReAndWr.DataList<short>(
                Ldata,
                Rdata,
                dlist.WavHeader
                );
        }
    }
}