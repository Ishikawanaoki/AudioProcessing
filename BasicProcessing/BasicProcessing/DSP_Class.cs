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
            public static IEnumerable<double> classedWaveForm(double[] x)
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
            public static double[] ACF(int divided, double[] x)
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
        //private double[] shortSign; // 任意の単位時間内の波形データ
        public ComplexStaff(int dividedNum, double[] rawSign)
        {
            this.dividedNum = dividedNum;
            this.rawSign = rawSign;

            if (dividedNum > 0)
                shortLength = rawSign.Length / dividedNum;
            //shortSign = new double[shortLength];
            //長さが shortLength を超える場合は起こり得ない
        }
        /// <summary>
        /// shortLength個のデータを、配列shortSignへ割り当てる。
        /// </summary>
        /// <param name="groupIndex"></param>
        /// <param name="sign"></param>
        private double[] AssignSignal(int groupIndex)
        {
            double[] shortSign = new double[shortLength];
            Array.Copy(rawSign, groupIndex * shortLength, shortSign, 0, shortLength);
            return shortSign; 
        }
        private Tuple<double[],double[],List<double>> RankedMagnitudeConvert(int rank, int groupIndex, StreamWriter sw)
        {
            // 必ずこのメソッドより先に、配列への割り当てメソッド AssignSignalを呼ぶ
            // ActiveComplex は複素数の配列を内部に保持し、
            // メソッド呼出しに対して、
            // 複素数の配列を変化させ
            // また、実行結果となる有効なオブジェクトを返す。
            ActiveComplex ac = new ActiveComplex(AssignSignal(groupIndex), Fourier.WindowFunc.Blackman);
            //int rank //= 5;
            //    = 1;
            // 内部への変化 あり
            ac.FTransform(Fourier.ComplexFunc.FFT);

            // 第一返り値 magnitude : 任意短時間における上位第5位のスペクトルの大きさ
            // 内部への変化 なし
            double[] magnitude = ac.RankedMagnitude(rank).ToArray();

            List<double> heldz = new List<double>();
            //sw.WriteLine("timelength : {0}", ac.Getlength());
            foreach (List<double> item1 in ac.ReturnHeldz(rank))
            {
                foreach(double item2 in item1)
                {
                if (sw != null){
                    //sw.Write("{0},", item1[0]);// 最大の周波数を返す
                }
                heldz.Add(item2);
                }
            }

            // 第二返り値 waveform :  
            List<double> waveform = new List<double>();
            foreach(Complex cmp in ac.FTransform(Fourier.ComplexFunc.IFFT))
            {
                waveform.Add(cmp.real);
            }
            return Tuple.Create(magnitude, waveform.ToArray(), heldz);
        }
        /// <summary>
        /// 2016/10/04 拡張
        /// 有効な周波数値の配列を返す
        /// サンプルの時間間隔は、計算可能
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public double[][] GetHeldz(int rank)
        {
            List<double[]> ans = new List<double[]>();
            

            //sw.WriteLine("全データ数 : {0}", rawSign.Length);
            #region time-waveform to time-wavefor
            for (int i = 0; i < dividedNum; i++)
            //過剰な後方の要素は切り捨てる
            {
                //sw.Write("グループ = {0} : ", i);
                //AssignSignal(i);
                List<double> tmp = new List<double>();
                foreach (double item in RankedMagnitudeConvert(rank, i, null).Item3)
                {
                    tmp.Add(item);
                }
                ans.Add(tmp.ToArray());
            }
            #endregion
            return ans.ToArray();
        }
        public Tuple<double[], double[]> DoSTDFT(int rank, string filename)
        {
            List<double> magnitudes = new List<double>();
            List<double> waveform = new List<double>();

            using (System.IO.FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                using (System.IO.StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    //sw.WriteLine("全データ数 : {0}", rawSign.Length);
                    #region time-waveform to time-wavefor
                    for (int i = 0; i < dividedNum; i++)
                    //過剰な後方の要素は切り捨てる
                    {
                        //sw.Write("グループ = {0} : ", i);
                        //AssignSignal(i);
                        Tuple<double[], double[], List<double>> ans = RankedMagnitudeConvert(rank, i, sw); // Console 出力の継続
                        magnitudes.AddRange(ans.Item1);
                        waveform.AddRange(ans.Item2);
                    }
                    #endregion
                }
            }

            return Tuple.Create(magnitudes.ToArray(), waveform.ToArray());
        }
    }
}
