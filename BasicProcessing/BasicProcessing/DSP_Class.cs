using function;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DSP
{
    namespace TimeDomain
    {
        public static class effector
        {
            public static double[] ACF(double[] x)
            {
                int length = x.Length / 10;
                double[] y = new double[length];
                foreach(int tau in Enumerable.Range(0, length))
                {
                    y[tau] = 0;
                    if (tau == 0) continue;
                    double[] tmp = x.SkipWhile((val, index) => index % tau == 0).ToArray();
                    int innerLength = length > tmp.Length ? tmp.Length : length;

                    foreach (int j in Enumerable.Range(0, innerLength))
                        y[tau] += x[j] * tmp[j];
                }

                return y;
            }
            public static IEnumerable<double> ACF(int divided, IEnumerable<double> x)
            {
                double[] ans = new double[x.Count()];
                int winLength = x.Count() / divided;
                // tau mean time renge
                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    foreach(int j in Enumerable.Range(0, x.Count()))
                    {
                        ans[tau] = x.ElementAtOrDefault(j) * x.ElementAtOrDefault(j + tau);
                    }
                }
                return ans;
            }
            /// <summary>
            /// Autocorrelation function
            /// </summary>
            public static double[] M_ACF(int divided, IEnumerable<double> x)
            {
                double[] ans = new double[x.Count()];
                int winLength = x.Count() / divided;
                // tau mean time renge
                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    int cursol = 0;
                    while (cursol + tau < x.Count())
                    {
                        ans[tau] += x.ElementAt(cursol) * x.ElementAt(cursol+tau);
                        if (++cursol > x.Count()) break;
                    }
                }
                return ans;
            }
            public static IEnumerable<double> Enum_M_ACF(int divided, IEnumerable<double> x)
            {
                double[] ans = new double[x.Count()];
                int winLength = x.Count() / divided;
                // tau mean time renge
                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    int cursol = 0;
                    while (cursol + tau < x.Count())
                    {
                        ans[tau] += x.ElementAt(cursol) * x.ElementAt(cursol + tau);
                        if (++cursol > x.Count()) break;
                    }
                }
                int num = divided;
                var cursor = from start in Enumerable.Range(0, num) select winLength * start;
                var query = from inner in Enumerable.Range(1, x.Count())
                            from outer in Enumerable.Range(1, x.Count())
                            select (inner * outer);
                return Enumerable.Range(0, x.Count()).
                    Select(c =>
                    {
                        return x.ElementAt(c);
                    });

            }
            public static double[] M_M(int divided, IEnumerable<double> x)
            {
                double[] ans = new double[x.Count()];
                int winLength = x.Count() / divided;
                // tau mean time renge
                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    foreach (int j in Enumerable.Range(0, x.Count()))
                    {
                        if (j + tau < x.Count())
                        {
                            ans[tau] += Math.Pow(x.ElementAt(j),2) + Math.Pow(x.ElementAt(j+tau),2);
                        }
                        else
                        {
                            ans[tau] += 0.0;
                        }
                    }
                }
                return ans;
            }
            public static double[] M_NSDF(int divided, IEnumerable<double> x)
            {
                double[] ans = new double[x.Count()];
                int winLength = x.Count() / divided;
                // tau mean time renge
                double[] ACF = M_ACF(divided, x);double[] M = M_M(divided, x);

                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    ans[tau] = M[tau] - 2 * ACF[tau];
                }
                return ans;
            }
            internal static double[] M_SDF(int divided, IEnumerable<double> x)
            {
                double[] ans = new double[x.Count()];
                int winLength = x.Count() / divided;
                // tau mean time renge
                double[] ACF = M_ACF(divided, x); double[] M = M_M(divided, x);

                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    ans[tau] = 2 * ACF[tau] / M[tau];
                }
                return ans;
            }
            private static IEnumerable<int> IndexOfChangepoint(double[] timeSample)
            {
                for (int i = 0; i < timeSample.Length; i++)
                {
                    // first step is skipped
                    if (i == 0) continue;
                    // change flag with comparing
                    if (timeSample[i] > 0 && timeSample[i - 1] < 0
                        || timeSample[i] < 0 && timeSample[i - 1] > 0)
                    {
                        yield return (timeSample[i] * timeSample[i]) > (timeSample[i - 1] * timeSample[i - 1])
                            ? (i - 1) : i;
                    }
                }
            }
            /// <summary>
            /// 同じ配列引数が隣り合った時に削除する。
            /// また、間にある要素がstr個未満の場合にも削除する。
            /// </summary>
            /// <param name="timeSample"></param>
            /// <returns></returns>
            public static IEnumerable<int> fIndexOfChangepoint(double[] timeSample, int str)
            {
                int i = -1;
                int count = 0;
                foreach (int item in IndexOfChangepoint(timeSample))
                {
                    if (i != item && count > str)
                    {
                        count = 0;
                        yield return item;
                    }
                    i = item;
                    count++;
                }
            }
            public static List<List<double>> assignTimewave(double[] rawSign, int Nmax, int select)
            {
                List<List<double>> storage = new List<List<double>>();
                switch (select)
                {
                    case 1:
                        // 短時間をNmaxで等間隔に分けた場合の割り当て
                        double[] tmp = new double[rawSign.Length / Nmax];
                        int count = 0;
                        foreach (double sign in rawSign)
                        {
                            if (count >= tmp.Length)
                            {
                                count = 0;
                                storage.Add(new List<double>(tmp));
                            }
                            tmp[count++] = sign;
                        }
                        break;
                    case 2:
                        // 零で交差する点で切り取る。
                        // 但し、Nmaxの10分の一に満たない場合は
                        // 次の期間に追加。
                        int[] changepoint = TimeDomain.effector.
                            fIndexOfChangepoint(rawSign, rawSign.Length / Nmax / 10).ToArray();
                        int pointcount = 0;
                        List<double> tmp2 = new List<double>();
                        for (int j = 0; j < rawSign.Length; j++)
                        {
                            if (changepoint[pointcount] <= 2)
                            {
                                pointcount++;
                                continue;
                            }
                            if (j == changepoint[pointcount] && tmp2.Count > 2)
                            {
                                storage.Add(tmp2);
                                tmp2 = new List<double>();
                                pointcount++;
                            }
                            tmp2.Add(rawSign[j]);
                        }
                        if (tmp2.Count > 2)
                            storage.Add(tmp2);
                        break;
                }
                return storage;
            }
        }
    }
    namespace FrequencyDomein
    {
        /// <summary>
        /// 全長の波形データ rawSignをフィールドに持ち、
        /// 周波数検知を短時間毎に実行するためのテストクラス
        /// </summary>
        public class TestPitchDetect
        {
            int Nmax;
            //double[] rawSign;
            List<List<double>> storage;
            public TestPitchDetect(int Nmax, double[] rawSign)
            {
                this.Nmax = Nmax;
                storage = new List<List<double>>();
                //assignTimewave(rawSign, 1);
                storage = TimeDomain.effector.assignTimewave(rawSign, Nmax, 1);
            }
            public void Execute()
            {
                double hertz = 0.0;double scale = 0.0;
                foreach(List<double> shortTime in storage)
                {
                    hertz = PitchDetect.AnalyzeSound(shortTime.ToArray());
                    scale = PitchDetect.ConvertHertzToScale(hertz);
                    // console out
                    Console.WriteLine("{0}Hz : {1},{2}",
                        hertz, scale,
                        PitchDetect.ConvertScaleToString(scale));
                }
            }
        }
        public static class PitchDetect
        {
            public static double AnalyzeSound(double[] timeSample)
            {
                // define here
                double threshold = 0.04f;    // ピッチとして検出する最小の分布
                int qSamples = 0;            // 配列のサイズ

                // define in costractor
                double[] spectrum;           // FFTされたデータ
                double fSample = 44100;      // サンプリング周波数

                // undefine : 分析結果の格納先
                double pitchValue;           // ピッチの周波数
                
                ActiveComplex ac = new ActiveComplex(timeSample, Fourier.WindowFunc.Blackman);
                ac.FTransform(Fourier.ComplexFunc.FFT); // array size is available
                spectrum = ac.GetMagnitude().ToArray();
                if (spectrum.Length > 0) qSamples = spectrum.Length;
                Tuple<int, double> max = new Tuple<int, double>(0, 0.0);
                
                int count = 0;
                // 最大値（ピッチ）を見つける。ただし、閾値は超えている必要がある
                // maxには2つの値の組み合わせを格納する。
                foreach (double sample in spectrum)
                {
                    if (sample > max.Item1 && sample > threshold)
                    {
                        max = new Tuple<int, double>(
                            count,
                            sample
                            );
                    }
                    count++;
                }
                double freqN = max.Item1;
                if (max.Item1 > 0 && max.Item1 < qSamples - 1)
                {
                    //隣のスペクトルも考慮する
                    double dL = spectrum[max.Item1 - 1] / spectrum[max.Item1];
                    double dR = spectrum[max.Item1 + 1] / spectrum[max.Item1];
                    freqN += 0.5f * (dR * dR - dL * dL);
                }
                pitchValue = freqN * (fSample / 2) / qSamples;// 何番目のスペクトル列か？
                return pitchValue;
            }
            public static double ConvertHertzToScale(double hertz)
            {
                if (hertz == 0) return 0.0f;
                else return (12.0f * Math.Log(hertz / 110.0f) / Math.Log(2.0f));
            }
            // 数値音階から文字音階への変換
            public static string ConvertScaleToString(double scale)
            {
                // 12音階の何倍の精度で音階を見るか
                int precision = 2;

                // 今の場合だと、mod24が0ならA、1ならAとA#の間、2ならA#…
                int s = (int)scale;
                if (scale - s >= 0.5) s += 1; // 四捨五入
                s *= precision;

                int smod = s % (12 * precision); // 音階
                int soct = s / (12 * precision); // オクターブ

                string value; // 返す値

                if (smod == 0) value = "A";
                else if (smod == 1) value = "A+";
                else if (smod == 2) value = "A#";
                else if (smod == 3) value = "A#+";
                else if (smod == 4) value = "B";
                else if (smod == 5) value = "B+";
                else if (smod == 6) value = "C";
                else if (smod == 7) value = "C+";
                else if (smod == 8) value = "C#";
                else if (smod == 9) value = "C#+";
                else if (smod == 10) value = "D";
                else if (smod == 11) value = "D+";
                else if (smod == 12) value = "D#";
                else if (smod == 13) value = "D#+";
                else if (smod == 14) value = "E";
                else if (smod == 15) value = "E+";
                else if (smod == 16) value = "F";
                else if (smod == 17) value = "F+";
                else if (smod == 18) value = "F#";
                else if (smod == 19) value = "F#+";
                else if (smod == 20) value = "G";
                else if (smod == 21) value = "G+";
                else if (smod == 22) value = "G#";
                else value = "G#+";
                value += soct + 1;

                return value;
            }
        }
    }
    
}
