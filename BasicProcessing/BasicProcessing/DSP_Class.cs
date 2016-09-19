using function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myfunction2
{
    namespace TimeDomein
    {
        public static class effector
        {
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
            /// </summary>
            /// <param name="timeSample"></param>
            /// <returns></returns>
            public static IEnumerable<int> fIndexOfChangepoint(double[] timeSample)
            {
                int i = -1;
                foreach (int item in IndexOfChangepoint(timeSample))
                {
                    if (i != item) yield return item;
                    i = item;
                }
            }
        }
    }
    namespace FrequencyDomein
    {
        public class TestPitchDetect
        {
            int Nmax;
            double[] rawSign;
            LinkedList<List<double>> storage;
            public TestPitchDetect(int Nmax, double[] rawSign)
            {
                this.Nmax = Nmax;
                this.rawSign = rawSign;
                storage = new LinkedList<List<double>>();
            }
            public void assignTime(int select)
            {
                switch (select)
                {
                    case 1:
                        double[] tmp = new double[rawSign.Length/Nmax];
                        int count = 0;
                        foreach(double sign in rawSign)
                        {
                            if(count > tmp.Length)
                            {
                                count = 0;
                                storage.AddLast(new List<double>(tmp));
                            }
                            tmp[count++] = sign;
                        }
                        break;
                    case 2:
                        int[] changepoint = TimeDomein.effector.
                            fIndexOfChangepoint(rawSign).ToArray();
                        int pointcount = 0;
                        List<double> tmp2 = new List<double>();
                        for(int j=0; j<rawSign.Length; j++)
                        {
                            if(changepoint[pointcount] <= 2)
                            {
                                pointcount++;
                                continue;
                            }
                            if (j == changepoint[pointcount] && tmp2.Count>2)
                            {
                                storage.AddLast(tmp2);
                                tmp2 = new List<double>();
                                pointcount++;
                            }
                            tmp2.Add(rawSign[j]);
                        }
                        if (tmp2.Count > 2)
                            storage.AddLast(tmp2);
                        break;
                }
            }
        }
        public class PitchDetect
        {
            // define here
            private double threshold = 0.04f;    // ピッチとして検出する最小の分布
            private int qSamples = 0;            // 配列のサイズ

            // define in costractor
            private double[] spectrum;           // FFTされたデータ
            private double fSample = 44100;      // サンプリング周波数

            // undefine : 分析結果の格納先
            private double pitchValue;           // ピッチの周波数

            public PitchDetect(double[] timeSample)
            {
                ActiveComplex ac = new ActiveComplex(timeSample, Fourier.WindowFunc.Blackman);
                ac.FTransform(Fourier.ComplexFunc.FFT); // array size is available
                spectrum = ac.GetMagnitude().ToArray();
                if (spectrum.Length > 0) qSamples = spectrum.Length;
            }
            public double AnalyzeSound()
            {
                Tuple<int, double> max = new Tuple<int, double>(0, 0.0);
                //double maxV = 0;
                //int maxN = 0;
                int count = 0;
                //最大値（ピッチ）を見つける。ただし、閾値は超えている必要がある
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
            public double ConvertHertzToScale(double hertz)
            {
                if (hertz == 0) return 0.0f;
                else return (12.0f * Math.Log(hertz / 110.0f) / Math.Log(2.0f));
            }

            // 数値音階から文字音階への変換
            public string ConvertScaleToString(double scale)
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
    class DSP_Class
    {
        private static double[] seikika(double[] data)
        {
            double max = 0;
            double min = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (max < data[i]) max = data[i];
                if (min > data[i]) min = data[i];
            }
            max += (-1) * min;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = data[i] / max * 1;
            }
            return data;
        }
        public static double[] SquareWave(int Nmax, int rate)
        {
            double[] data = new double[Nmax];

            double div = 2 * Math.PI / Nmax * rate; // 2*PI*F*Tに相当
            double tmp = 0; // jにのみ作用する
            for (int i = 0; i < data.Length; i++)
            {
                for (int j = 0; j < 30; j++)
                {
                    tmp += Math.Sin((2 * j - 1) * div) / (2 * j - 1);
                }
                data[i] = tmp * 4 / Math.PI; tmp = 0;
            }

            return seikika(data);
        }
        /// <summary>
        /// テストを行う対象を自動的に作るプログラム
        /// </summary>
        public void initsample(string root)
        {
            // データ列リストの宣言、初期化
            List<short> ldata = new List<short>();
            List<short> rdata = new List<short>();
            // テスト対象ファイル名
            string longbinarysample = root + @"\bsample";
            // 空の文字列
            string tfilename = "";
            string[] InFile = new string[]
            {
                @"..\..\音ファイル\a1.wav",//@"..\..\音ファイル\a1s.wav",
                @"..\..\音ファイル\b1.wav",
                @"..\..\音ファイル\c1.wav",//@"..\..\音ファイル\c1s.wav",@"..\..\音ファイル\c2.wav",
                @"..\..\音ファイル\d1.wav",//@"..\..\音ファイル\d1s.wav",
                @"..\..\音ファイル\e1.wav",
                @"..\..\音ファイル\f1.wav",//@"..\..\音ファイル\f1s.wav",
                @"..\..\音ファイル\g1.wav"//,@"..\..\音ファイル\g1s.wav"
            };
            WaveReAndWr.DataList<short> tmp = WaveReAndWr.WavReader(InFile[0], "", false);
            #region 音ファイルの入力、合成出力
            for (int i = 1; i < InFile.Length; i++)
            {
                tmp = WaveReAndWr.WavReader(InFile[i], "", false);
                ldata.AddRange(tmp.lDataList);
                rdata.AddRange(tmp.rDataList);
            }

            tfilename = longbinarysample + "01.wav";
            tmp = new WaveReAndWr.DataList<short>(ldata, rdata, tmp.WavHeader);
            WaveReAndWr.WavWriter(tfilename, tmp);
            #endregion
            double[] ldata2 = new double[ldata.Count];
            double[] rdata2 = new double[rdata.Count];
            #region 正規化したデータを出力
            for (int ii = 0; ii < ldata.Count; ii++)
            {
                ldata2[ii] = ldata[ii];
                rdata2[ii] = rdata[ii];
            }
            ldata2 = myfunction.seikika(ldata2).ToArray();
            rdata2 = myfunction.seikika(rdata2).ToArray();

            //PlaySound(tfilename);
            for (int iii = 0; iii < ldata2.Length; iii++)
            {
                ldata[iii] = (short)ldata2[iii];
                rdata[iii] = (short)rdata2[iii];
            }
            #endregion
            tfilename = longbinarysample + "02.wav";
            tmp = new WaveReAndWr.DataList<short>(ldata, rdata, tmp.WavHeader);
            WaveReAndWr.WavWriter(tfilename, tmp);
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
        private double[] shortSign; // 任意の単位時間内の波形データ
        public ComplexStaff(int dividedNum, double[] rawSign)
        {
            this.dividedNum = dividedNum;
            this.rawSign = rawSign;

            if (dividedNum > 0)
                shortLength = rawSign.Length / dividedNum;
            shortSign = new double[shortLength];
        }
        /// <summary>
        /// shortLength個のデータを、配列shortSignへ割り当てる。
        /// </summary>
        /// <param name="groupIndex"></param>
        /// <param name="sign"></param>
        private void AssignSignal(int groupIndex)
        {
            //shortSign = 
            //  rawSign.Skip(groupIndex * shortLength).TakeWhile(i => i <= shortLength).ToArray();
            Array.Copy(rawSign, groupIndex * shortLength, shortSign, 0, shortLength);
        }
        private double[] RankedMagnitudeConvert()
        {
            ActiveComplex ac = new ActiveComplex(shortSign, Fourier.WindowFunc.Blackman);
            ac.FTransform(Fourier.ComplexFunc.FFT);
            //ac.GetMagnitude();
            return ac.RankedMagnitude(5).ToArray();
        }
        public double[] DoSTDFT()
        {
            List<double> ans = new List<double>();

            #region time-waveform to time-wavefor
            for(int i=0; i<dividedNum; i++)//過剰な後方の要素は切り捨てる
            {
                AssignSignal(i);
                ans.AddRange(RankedMagnitudeConvert());
            }
            #endregion

            return ans.ToArray();
        }
    }
}
