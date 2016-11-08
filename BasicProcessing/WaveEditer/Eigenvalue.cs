using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveEditer
{
    namespace Eigenvalue
    {
        public static class Union
        {
            /// <summary>
            /// double 型配列を表示する
            /// </summary>
            /// <param name="row"></param>
            public static void DispVector(double[] row)
            {
                foreach (var col in row)
                    Console.WriteLine(string.Format("{0,14:F10}\t", col));
                Console.WriteLine();
            }
            /// <summary>
            /// 正規化
            /// </summary>
            /// <param name="x0">参照引数</param>
            public static void normarize(ref double[] x0)
            {
                double s = x0.Select(c => Math.Sqrt(c * c)).Sum();
                x0 = x0.Select(c => c / s).ToArray();
            }
        }
        public class CS1101
        {
            private const int N = 4;
            public static void Exacute()
            {
                double[][] a = new double[N][]{ 
                    new double[N]{ 5.0,4.0,1.0,1.0},
                    new double[N]{4.0,5.0,1.0,1.0},
                    new double[N]{1.0,1.0,4.0,2.0},
                    new double[N]{1.0,1.0,2.0,4.0}};
                double[] x = new double[N]{ 1.0, 0.0, 0.0, 0.0 };
                double lambda = power(a, x);

                Console.WriteLine();
                Console.WriteLine("eigenvalue");
                Console.WriteLine(string.Format("{0,14:F10}", lambda));
                Console.WriteLine("eugenvector");
                Union.DispVector(x);
            }
            private static double power(double[][] a, double[] x0)
            {
                double lambda = 0.0;

                Union.normarize(ref x0);

                double e0 = x0.Sum();
                for(int k=1; k<=200; k++)
                {
                    double[] x1 = a.Select(c => {
                        return Enumerable.Range(0, x0.Count())
                        .Select(t => c.ElementAt(t) * x0.ElementAt(t))
                        .Sum();
                    }).ToArray();

                    double p0 = x1.Select(c => c * c).Sum();
                    double p1 = Enumerable.Range(0, x0.Count())
                        .Select(c => x1.ElementAt(c) * x0.ElementAt(c))
                        .Sum();
                    // 固有値
                    lambda = p0 / p1;
                    // 正規化
                    Union.normarize(ref x1);
                    
                    //収束判定
                    double e1 = x1.Sum();
                    if (Math.Abs(e0 - e1) < 1E-11) break;

                    x0 = x1.Select(c => c).ToArray();
                    e0 = e1;
                }
                return lambda;
            }
        }
        public class CS1102
        {
            private const int N = 4;
            public static void Exacute()
            {
                double[][] a = new double[N][]{
                    new double[N]{ 5.0,4.0,1.0,1.0},
                    new double[N]{4.0,5.0,1.0,1.0},
                    new double[N]{1.0,1.0,4.0,2.0},
                    new double[N]{1.0,1.0,2.0,4.0}};
                double[] x = new double[N] { 1.0, 0.0, 0.0, 0.0 };

                //forward_elimination(a);

                double lambda = inverse(a, x);

                Console.WriteLine();
                Console.WriteLine("eigenvalue");
                Console.WriteLine(string.Format("{0,14:F10}", lambda));
                Console.WriteLine("eigenvector");
                Union.DispVector(x);
            }
            /// <summary>
            /// 逆ベキ乗法
            /// </summary>
            /// <param name="a"></param>
            /// <param name="x"></param>
            /// <returns></returns>
            private static double inverse(double[][] a, double[] x0)
            {
                double lambda = 0.0;

                Union.normarize(ref x0);

                double e0 = x0.Sum();

                for(int k=1; k<=200; k++)
                {
                    // y from Ly = b(前進代入)
                    
                    double[] b = x0.Select(c => c).ToArray();
                    double[] y = forward_substitution(a, b);

                    // x from Ux = y(後退代入)

                    double[] x1 = backward_substitution(a, y);

                    // 内積

                    double p0 = x1.Select(c => c * c).Sum();
                    double p1 = Enumerable.Range(0, N)
                        .Select(c => x1.ElementAt(c) * x0.ElementAt(c))
                        .Sum();

                    // 固有値
                    lambda = p1 / p0;

                    Union.normarize(ref x1);

                    // 収束判定
                    double e1 = x1.Sum();
                    if (Math.Abs(e0 - e1) < 1E11) break;

                    x0 = x1.Select(c => c).ToArray();
                    e0 = e1;
                }
                return lambda;
            }

            private static double[] backward_substitution(double[][] a, double[] y)
            {
                throw new NotImplementedException();
            }

            private static double[] forward_substitution(double[][] a, double[] b)
            {
                throw new NotImplementedException();
            }

            private static void forward_substitution(double[][] a, object y, double[] b)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// LU分解
            /// </summary>
            /// <param name="a"></param>
            private static void forward_elimination(double[,] a)
            {
                for(int pivot=0; pivot<N-1; pivot++)
                {
                    for(int row=pivot+1; row<N; row++)
                    {
                        double s = a[row, pivot] / a[pivot, pivot];
                        // 上三角行列
                        for (int col = pivot; col < N; col++)
                            a[row, col] -= a[pivot, col] * s;

                        a[row, pivot] = s;
                    }
                }
            }
        }
    }
}
