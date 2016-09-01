using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace function
{
    namespace otherUser
    {
        public static class IEnumerableExtensions
        {
            public static double Nearest(this IEnumerable<double> self, double target)//Nearestメソッド//
            {
                var min = self.Min(c => Math.Abs(c - target));
                return self.First(c => Math.Abs(c - target) == min);
            }
        }
        public static class Music
        {
            public static double[] effecientMusicalScale(double[] sign)
            {
                var list = new[]
                {
                16.35, 17.32, 18.35, 19.45, 20.6, 21.83, 23.12, 24.5, 25.96, 27.5, 29.14, 30.87, 32.7,
                34.65, 36.71, 38.89, 41.2, 43.65, 46.25, 49, 51.91, 55, 58.27, 61.74, 65.41, 69.3, 73.42, 77.78, 82.41,
                87.31, 92.5, 98, 103.83, 110, 116.54, 123.47, 130.81, 138.59, 146.83, 155.56, 164.81, 174.61, 185, 196,
                207.65, 220, 233.08, 246.94, 261.63, 277.18, 293.66, 311.13, 329.63, 349.23, 369.99, 392, 415.3, 440,
                466.16, 493, 523.25, 554.37, 587.33, 622.25, 659.26, 698.46, 739.99, 783.99, 830.61, 880, 932.33, 987.77,
                1046.5, 1108.73, 1174.66, 1244.51, 1318.51, 1396.91, 1479.98, 1567.98, 1661.22, 1760, 1864.66, 1975.53,
                2093, 2217.46, 2349.32, 2489.02, 2637.02, 2793.83, 2959.96, 3135.96, 3322.44, 3520, 3729.31, 3951.07,
                4186.01, 4434.92, 4698.64, 4978.03, 5274.04, 5587.65, 5919.91, 6271.93, 6644.88, 7040, 7458.62,
                7902.13, 8372.02, 8869.84, 9397.27, 9956.06, 10548.08, 11175.3, 11839.82, 12543.85, 13289.75, 14080,
                14917, 15804, 16274, 16744
            };
                List<double> ans = new List<double>();
                double value;
                foreach (double item in sign)
                {
                    value = list.Nearest(item);
                    Console.Write(value); // 文字や数値の出力
                    ans.Add(value);
                }
                return ans.ToArray();
            }
        }
        public class MathematicalWave
        {
            // 三角波
            // 振幅、基本周波数、サンプリング周波数、長さの4つの引数をとり、三角波を返却します。
            /// <summary>
            /// 
            /// </summary>
            /// <param name="A"></param>
            /// <param name="f0"></param>
            /// <param name="fs"></param>
            /// <param name="length"></param>
            /// <returns></returns>
            public double[] createTriangleWave(double A, double f0, double fs, double length)
            {
                double[] data = new double[0];
                double[] result = new double[0];
                // [-1.0, 1.0]の小数値が入った波を作成
                for (int n = 0; n < (length * fs); n++) //nはサンプルインデックス
                {
                    double s = 0.0;

                    for (int k = 0; k < 10; k++) // サンプルごとに10個のサイン波を重ね合わせ
                    {
                        s = s 
                            + Math.Pow((-1), k)
                            * (A / Math.Pow((2 * k + 1), 2)) 
                            * Math.Sin((2 * k + 1) * 2 * Math.PI * f0 * n / fs);
                        // 振幅が大きい時はクリッピング
                        if (s > 1.0)
                        {
                            s = 1.0;

                        }

                        if (s < -1.0)
                        {
                            s = -1.0;

                        }

                        List<double> list = new List<double>(data);
                        list.Add(s);
                        result = list.ToArray();
                    }
                }
                return result;
            }
            //ノコギリ波
            //振幅、基本周波数、サンプリング周波数、長さの4つの引数をとり、ノコギリ波を返却します。
            /// <summary>
            /// 
            /// </summary>
            /// <param name="A"></param>
            /// <param name="f0"></param>
            /// <param name="fs"></param>
            /// <param name="length"></param>
            /// <returns></returns>
            double[] createSawtoothWave(double A, double f0, double fs, double length)
            {
                double[] data = new double[0];
                double[] result = new double[0];
                //[-1.0, 1.0]の小数値が入った波を作成
                for (int n = 0; n < (length * fs); n++) //nはサンプルインデックス
                {
                    double s = 0.0;

                    for (int k = 0; k < 10; k++) // サンプルごとに10個のサイン波を重ね合わせ
                    {
                        s = s + (A / k) * Math.Sin(2 * Math.PI * k * f0 * n / fs);
                        // 振幅が大きい時はクリッピング
                        if (s > 1.0)
                        {
                            s = 1.0;
                        }

                        if (s < -1.0)
                        {
                            s = -1.0;
                        }

                        List<double> list = new List<double>(data);
                        list.Add(s);
                        result = list.ToArray();
                    }
                }
                return result;
            }
        }
    }
}