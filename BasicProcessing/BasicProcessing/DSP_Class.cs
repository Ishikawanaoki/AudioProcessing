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
        public static class Filter
        {
            const int samplerate = 44100;
            public static IEnumerable<double> HighPass(double[] str, float fs)
            {
                float q = (float)Math.Sqrt(0.5);
                float omega = 2.0f * (float)Math.PI * fs / samplerate;
                float alpha = (float)Math.Sin(omega) / (2.0f * q);

                float a0 = 1.0f + alpha;
                float a1 = -2.0f * (float)Math.Cos(omega);
                float a2 = 1.0f - alpha;
                float b0 = (1.0f + (float)Math.Cos(omega)) / 2.0f;
                float b1 = -(1.0f + (float)Math.Cos(omega));
                float b2 = (1.0f + (float)Math.Cos(omega)) / 2.0f;

                float in1 = 0, in2 = 0, out1 = 0, out2 = 0;
                var tmp = effector.GetEnumerableWave(str);
                var tmp2 =  Enumerable.Range(0, tmp.Count()).Select(i =>{
                    in1 = (float)tmp.ElementAtOrDefault(i - 2);
                    in2 = (float)tmp.ElementAtOrDefault(i - 1);
                    return b0 / a0 * tmp.ElementAtOrDefault(i) + b1 / a0 * in1 + b2 / a0 * in2;
                });

                return Enumerable.Range(0, tmp.Count()).Select(i => {
                    out1 = (float)tmp2.ElementAtOrDefault(i + 2);
                    out2 = (float)tmp2.ElementAtOrDefault(i + 1);
                    return tmp2.ElementAtOrDefault(i) - a1 / a0 * out1 - a2 / a0 * out2;
                });
                }
        }
        public static class effector
        {
            public static IEnumerable<double> GetEnumerableWave(double[] x)
            {
                foreach (var str in x)
                    yield return str;
            }
            public static IEnumerable<double> classedWaveForm(IEnumerable<double> x)
            {
                double unsafeValue = 0.10f;
                bool flag = true;
                foreach (var target in x) {
                    if (target < unsafeValue && flag) break;
                    else if (target < unsafeValue) flag = false;
                    else flag = true;
                    yield return target;
                }
            }

            /// <summary>
            /// Autocorrelation function
            /// </summary>
            public static IEnumerable<double> ACF(int divided, double[] x)
            {
                double[] ans = new double[x.Length];
                int winLength = x.Length / divided;
                // tau mean time renge
                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    foreach(int j in Enumerable.Range(0, x.Length))
                    {
                        if (j + tau < x.Length)
                        {
                            ans[tau] += x[j] * x[j + tau];
                        }
                        else
                        {
                            ans[tau] += 0.0;
                        }
                    }
                }
                return ans;
            }
            /// <summary>
            /// Autocorrelation function
            /// </summary>
            public static double[] M_ACF(int divided, double[] x)
            {
                double[] ans = new double[x.Length];
                int winLength = x.Length / divided;
                // tau mean time renge
                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    int cursol = 0;
                    while (cursol + tau < x.Length)
                    {
                        ans[tau] += x[cursol] * x[cursol + tau];
                        if (++cursol > x.Length) break;
                    }
                }
                return ans;
            }
            public static IEnumerable<double> Enum_M_ACF(int divided, double[] x)
            {
                double[] ans = new double[x.Length];
                int winLength = x.Length / divided;
                // tau mean time renge
                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    int cursol = 0;
                    while (cursol + tau < x.Length)
                    {
                        ans[tau] += x[cursol] * x[cursol + tau];
                        if (++cursol > x.Length) break;
                    }
                }
                int num = divided;
                var cursor = from start in Enumerable.Range(0, num) select winLength * start;
                var query = from inner in Enumerable.Range(1, x.Length)
                            from outer in Enumerable.Range(1, x.Length)
                            select (inner * outer);
                return Enumerable.Range(0, x.Length).
                    Select(c =>
                    {
                        return x[c];
                    });

            }
            public static double[] M_M(int divided, double[] x)
            {
                double[] ans = new double[x.Length];
                int winLength = x.Length / divided;
                // tau mean time renge
                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    foreach (int j in Enumerable.Range(0, x.Length))
                    {
                        if (j + tau < x.Length)
                        {
                            ans[tau] += Math.Pow(x[j],2) + Math.Pow(x[j + tau],2);
                        }
                        else
                        {
                            ans[tau] += 0.0;
                        }
                    }
                }
                return ans;
            }
            public static double[] M_NSDF(int divided, double[] x)
            {
                double[] ans = new double[x.Length];
                int winLength = x.Length / divided;
                // tau mean time renge
                double[] ACF = M_ACF(divided, x);double[] M = M_M(divided, x);

                foreach (int tau in Enumerable.Range(1, winLength - 1))
                {
                    ans[tau] = M[tau] - 2 * ACF[tau];
                }
                return ans;
            }
            internal static double[] M_SDF(int divided, double[] x)
            {
                double[] ans = new double[x.Length];
                int winLength = x.Length / divided;
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
            this.rawSign = TimeDomain.Filter.HighPass(rawSign, 2000).ToArray();
            this.rawSign = rawSign;

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

            return shortSign;
        }
        private double[] ShortTimeRankedHeldz(int[] rank, int groupIndex)
        {
            ActiveComplex ac = new ActiveComplex(AssignSignal(groupIndex), Fourier.WindowFunc.Hamming);
            ac.FTransform(Fourier.ComplexFunc.FFT); double[] tmp = ac.GetHeldz(rank).ToArray();
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
