using BasicProcessing.Fourier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicProcessing
{
    namespace ShortTimes
    {
        public class ActiveComplex
        {
            private List<Complex> complex;
            //private int _length;
            /// <summary>
            /// double配列から呼び出される。
            /// double[] => Complex[]にするためには、必ず内部でAdd()が呼び出される
            /// </summary>
            /// <param name="items"></param>
            public int Getlength()
            {
                return complex.Count;
            }
            public ActiveComplex(double[] items)
            {
                complex = new List<Complex>();
                Add(items);
            }
            public ActiveComplex(double[] items, Fourier.WindowFunc wfunc)
            {
                complex = new List<Complex>();
                Add(Fourier.Fourier.Windowing(items, wfunc));
            }
            public ActiveComplex(Complex[] items)
            {
                complex = items.ToList();
            }
            private void Add(double[] items)
            {
                foreach (double item in items)
                    complex.Add(new Complex(item, 0));
            }
            public IEnumerable<Complex> GetEnumerable()
            {
                foreach (var str in complex)
                    yield return str;
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
            public IEnumerable<double> GetReality()
            {
                foreach (Complex item in complex)
                    yield return item.real;
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
                foreach (var tmp in rank.OrderByDescending(t => t))
                {
                    while (countup++ <= tmp)
                    {
                        max = GetMagnitude().Where(c => c < max).Max();
                    }
                    yield return max;
                }
            }
            /// <summary>
            /// 一つの時間窓での任意の順位に対する、インデックスを先頭から検索
            /// </summary>
            /// <param name="rank"></param>
            /// <returns></returns>
            /// 

            public static int _tmp_counter = 0;
            public static int realcount = 0;
            public IEnumerable<int> GetRanked_Index(int[] rank)
            {
                foreach (var str in GetRanked_Muximums(rank))
                {
                    yield return GetMagnitude()
                        .Select((val, index) => // 振幅スペクトルから値 nameと、indexを順次に取り出す
                    {
                        realcount++;
                        if (val == str) return index;             // 任意順位 str に該当
                        else return -1;               // 該当なし
                    })
                        .Where(c => c > 0)                // 該当なしを弾く
                        .Select(c =>
                        {
                            return c;
                        })
                        .FirstOrDefault();                 // 最初の対象を
                                                           // シーケンスで返す

                }
            }
            public IEnumerable<Complex> RankedComplex(int[] rank)
            {
                return GetEnumerable().Select((name, index) =>
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
                    else return new Complex(0, 0);
                });
            }
            /// <summary>
            /// 配列rankに対する周波数のみを透過させるフィルタ
            /// </summary>
            /// <param name="rank"></param>
            /// <returns></returns>
            public IEnumerable<double> RankedMagnitude(int[] rank)
            {
                IEnumerable<double> tmp;
                IEnumerable<double> ans = Enumerable.Range(0, complex.Count).Select(c => 1.0); // 全て1

                foreach (var str in GetRanked_Muximums(rank))
                {
                    tmp = GetMagnitude()
                        .Select((num, index) => // 振幅スペクトルから値 nameと、indexを順次に取り出す
                    {
                        if (num == str) return num; // 透過
                        else return 0.0;              // 遮断
                    });
                    ans = ans.SelectMany((t) => tmp, (t, a) => t * a);
                }
                return ans;
            }
            public IEnumerable<double> GetHeldz(int[] rank)
            {
                double length = this.complex.Count; // 時間窓の大きさ

                double axis_fru = 44100 / length; // グラフ : 周波数-振幅スペクトルでの周波数の一目盛り

                foreach (var item in GetRanked_Index(rank))
                {
                    double tmp = item;
                    if (item < length / 2) { tmp *= axis_fru; }
                    else { tmp *= axis_fru * (-1); }
                    yield return function.otherUser.Music.OneMusicalScale(item);
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
                complex = new List<Complex>(Fourier.Fourier.FTransform(complex.ToArray(), cfunc));
                return complex.ToArray();
            }
            public double[] FunctionTie()
            {
                FTransform(ComplexFunc.FFT);
                FTransform(ComplexFunc.IFFT);
                return GetMagnitude().ToArray();
            }
            // new added
            // Pass <- 
            private IEnumerable<double> OnlyReal(IEnumerable<Complex> cmp)
            {
                return cmp.Select(c => c.real);
            }
            public IEnumerable<double> HighPassDSP(int fr)
            {
                var converted = Fourier.IEnumerableFourier.FFT(complex);

                int target = Enumerable.Range(0, int.MaxValue)
                    .Where(c =>
                        fr <= 44100 / converted.Count() * c
                    ).FirstOrDefault();

                var passed = converted.Select((val, index) =>
                {
                    if (index <= target) return new Complex(0, 0);
                    else return val;
                });

                //return OnlyReal(Fourier.IFFT(passed));
                return OnlyReal(passed);
            }
            public IEnumerable<double> LowPassDSP(int fr)
            {
                var converted = Fourier.IEnumerableFourier.FFT(complex);

                int target = Enumerable.Range(0, int.MaxValue)
                    .Where(c =>
                        fr >= 44100 / converted.Count() * c
                    ).FirstOrDefault();

                var passed = converted.Select((val, index) =>
                {
                    if (index <= target) return val;
                    else return new Complex(0, 0);
                });

                //return OnlyReal(Fourier.IFFT(passed));
                return OnlyReal(passed);
            }
        }

        /// <summary>
        /// for stdft(fft)
        /// </summary>
        public class ComplexStaff
        {
            private int dividedNum;     // 分割数
            private double[] rawSign;   // 変換前の波形データ（全体）
            private int shortLength;    // 短時間に対応するデータ数
            public ComplexStaff(int dividedNum, double[] rawSign)
            {
                this.dividedNum = dividedNum;

                // ハイパスに通す
                //this.rawSign = TimeDomain.Filter.HighPass(rawSign, 2000).ToArray();
                //this.rawSign = rawSign;

                if (dividedNum > 0)
                    shortLength = rawSign.Length / dividedNum;
            }
            /// <summary>
            /// shortLength個のデータを、配列shortSignへ割り当てる。
            /// </summary>
            /// <param name="groupIndex"></param>
            /// <param name="sign"></param>
            private double[] AssignSignal(int groupIndex)
            {
                // 短時間に分割
                double[] shortSign = new double[shortLength];
                Array.Copy(rawSign, groupIndex * shortLength, shortSign, 0, shortLength);

                // ハイパスに通す
                //shortSign = TimeDomain.Filter.HighPass(shortSign, 2000).ToArray();

                return shortSign;
            }
            private double[] ShortTimeRankedHeldz(int[] rank, int groupIndex)
            {
                //ActiveComplex.realcount = 0; ActiveComplex._tmp_counter = 0;
                ActiveComplex ac = new ActiveComplex(AssignSignal(groupIndex), Fourier.WindowFunc.Hamming);
                ac.FTransform(Fourier.ComplexFunc.FFT);
                //Console.WriteLine("group:{0},pass:{1},allcount:{2}", groupIndex,ActiveComplex._tmp_counter, ActiveComplex.realcount);

                double[] tmp = ac.GetHeldz(rank).ToArray();
                //Console.Write("{");
                //foreach (var str in tmp)
                //Console.Write("{0} ,", str);
                //Console.WriteLine();
                return tmp;
            }
            private double[] ShortTimeRankedMagnitude(int[] rank, int groupIndex)
            {
                ActiveComplex ac = new ActiveComplex(AssignSignal(groupIndex), Fourier.WindowFunc.Hamming);
                ac.FTransform(Fourier.ComplexFunc.FFT);

                return ac.RankedMagnitude(rank).ToArray();
            }
            private double[] InverseSTDFT(double[] mdata)
            {
                ActiveComplex ac = new ActiveComplex(mdata);
                ac.FTransform(Fourier.ComplexFunc.IFFT);
                return ac.GetReality().ToArray();
            }
            /// <summary>
            /// 2016/10/04 拡張
            /// 有効な周波数値の配列を返す
            /// サンプルの時間間隔は、計算可能
            /// </summary>
            /// <param name="filename"></param>
            /// <returns></returns>
            public double[][] GetHeldz(int[] rank)
            {
                List<double[]> heldz = new List<double[]>();

                for (int timeWindow = 0; timeWindow < dividedNum; timeWindow++)
                {
                    heldz.Add(ShortTimeRankedHeldz(rank, timeWindow));
                }
                return heldz.ToArray();
            }
            public double[] DoSTDFT(int[] rank)
            {
                List<double> waveform = new List<double>();

                for (int timeWindow = 0; timeWindow < dividedNum; timeWindow++)
                {
                    waveform.AddRange(InverseSTDFT(ShortTimeRankedValue(rank, timeWindow)));
                }

                return waveform.ToArray();
            }

            private double[] ShortTimeRankedValue(int[] rank, int groupIndex)
            {
                ActiveComplex ac = new ActiveComplex(AssignSignal(groupIndex), Fourier.WindowFunc.Hamming);
                ac.FTransform(Fourier.ComplexFunc.FFT);

                ac = new ActiveComplex(ac.RankedComplex(rank).ToArray());
                ac.FTransform(Fourier.ComplexFunc.IFFT);

                return ac.GetReality().ToArray();
            }
        }
    }
}
