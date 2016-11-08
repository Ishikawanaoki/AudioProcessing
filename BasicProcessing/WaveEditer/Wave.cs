using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace WaveEditer
{
    public static class Wave
    {
        public static double count = 1; // 周期]
        public static int fs = 100;
        /// <summary>
        /// get omega [rad per sec]
        /// return means 2nPI / fs [rad / (sec,sample)]
        /// </summary>
        /// <returns>theta line, contains a number of items , where length times sampling frequency </returns>
        private static IEnumerable<double> Merge(IEnumerable<double> arg1, IEnumerable<double> arg2)
        {
            int N1 = arg1.Count(); int N2 = arg2.Count();
            N1 = N1 > N2 ? N2 : N1;
            return Enumerable.Range(0, N1)
                .Select(c => arg1.ElementAt(c) + arg2.ElementAt(c));
        }
        private static IEnumerable<double> Multiply(IEnumerable<double> arg1, IEnumerable<double> arg2)
        {
            int N1 = arg1.Count(); int N2 = arg2.Count();
            N1 = N1 > N2 ? N2 : N1;
            return Enumerable.Range(0, N1)
                .Select(c => arg1.ElementAt(c) * arg2.ElementAt(c));
        }
        private static IEnumerable<double> ManyOpe(IEnumerable<double>[] arg, EnuamerOperate argc)
        {
            if (arg.Length < 2) return arg.SingleOrDefault();
            else
            {
                var ans = arg.ElementAt(0);
                switch (argc)
                {
                    case EnuamerOperate.Merge:
                        ans = Merge(ans, arg.ElementAt(1));
                        break;
                    case EnuamerOperate.Multiply:
                        ans = Merge(ans, arg.ElementAt(1));
                        break;
                }
                var tmp = arg.Skip(2);

                //tmp.Concat(ans);
                int count = tmp.Count();
                tmp = Enumerable.Range(0, count + 1)
                    .Select((c, index) =>
                    {
                        if (index < count) return tmp.ElementAt(c);
                        else return ans;
                    });

                return ManyOpe(tmp.ToArray(), argc);
            }
        }
        public enum EnuamerOperate
        {
            Merge,
            Multiply
        }
        private static IEnumerable<double> GetOneSesond()
        {
            double DivTheta = 2 * PI / fs;
            int num = (int)(fs * count);
            return Enumerable.Range(0, num).Select(c => c * DivTheta);
        }
        private static IEnumerable<double> GetOneSesond(double seconds)
        {
            double DivTheta = 2 * PI / fs;
            int num = (int)(fs * seconds);
            return Enumerable.Range(0, num).Select(c => c * DivTheta);
        }
        /// <summary>
        /// 1秒に対して、
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="particle"></param>
        /// <returns></returns>
        private static IEnumerable<double> GetOneSesondDelay(int seconds, double particle)
        {
            double DivTheta = 2 * PI / fs;
            int num = (int)(fs * seconds);
            int start = (int)(num * particle);
            return Enumerable.Range(start, num).Select(c => c * DivTheta);

        }
        /// <summary>
        ///  データ数はA0が丁度一周期分入るのに対して、
        ///  高い音階程入る周期は非整数倍に増えていく
        /// </summary>
        /// <param name="times">回転速度の制御</param>
        /// <param name="A">振幅は0(無入力)または1</param>
        /// <param name="num">データ数</param>
        /// <returns></returns>
        private static IEnumerable<double> GetOneNote(double sec,double times)
        {
            return SinWave(1, 1).Concat(
                SinWave(2, 100));
        }
        public static IEnumerable<double> GetNoteWave(double times, int A)
        {
            return GetOneNote(0.025,times).Concat(GetOneNote(1,times))
                .Select(c => A * Sin(c));
        }
        public static IEnumerable<double> SinWave(int A, double f0)
        {
            return GetOneSesond().Select(c => A * Sin(c * f0));
        }
        public static IEnumerable<double> SinWave(int A, double f0, int Length)
        {//
            return GetOneSesond().Select(c => A * Sin(c * f0));
        }
        public static IEnumerable<double> QSinWave(int A, double f0)
        {
            int length = SinWave(A, f0).Count();
            return SinWave(A, f0).TakeWhile((val, index) => index < length / 2);
        }
        public static IEnumerable<double> SawtoothWave(int A, double f0)
        {
            return GetOneSesond().Select(c => {
                return Enumerable.Range(1, 10)
                    .Select(k => A / k * Sin(c * k * f0))
                    .Sum();
            });
        }
        public static IEnumerable<double> filter(IEnumerable<double> str, int A)
        {
            return str.Select(c =>
            {
                if (c > A) return A;
                else if (c < A) return -A;
                else return c;
            });
        }
        public static IEnumerable<double> TriangleWave(int A, double f0)
        {
            return GetOneSesond().Select(c => {
                return Enumerable.Range(0, 10).Select(k => {
                    var m = 2 * k + 1; // m = 1,3,5,7,9,11,...
                    return Pow((-1), k) * (A / Pow(m, 2)) * Sin(c * k);
                }).Sum();
            });
        }
    }
}
