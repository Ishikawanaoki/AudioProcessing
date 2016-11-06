using System;
using System.Collections.Generic;
using System.Linq;
namespace Fourier
{
    /// <summary>
    /// 複素数の処理
    /// </summary>
    public class Complex
    {
        public double real = 0.0;
        public double img = 0.0;
        /// <summary>
        /// 一つの複素数を生成
        /// </summary>
        /// <param name="real">実部</param>
        /// <param name="img">虚部</param>
        public Complex(double real, double img)
        {
            this.real = real;
            this.img = img;
        }
        /// <summary>
        /// "実部"+"虚部"j : String
        /// </summary>
        /// <returns>"実部"+"虚部"j</returns>
        override public string ToString()
        {
            string data = real.ToString() + "+" + img.ToString() + "i";
            return data;
        }
        /// <summary>
        /// 極座標から直交座標への変換
        /// </summary>
        /// <param name="r"></param>
        /// <param name="radians"></param>
        /// <returns>直交化</returns>
        public static Complex from_polar(double r, double radians)
        {
            return new Complex(r * Math.Cos(radians), r * Math.Sin(radians));
        }
        public static Complex Polar(double radians)
        {
            return new Complex(Math.Cos(radians), Math.Sin(radians));
        }
        /// <summary>
        /// 複素数同士の和
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>和</returns>
        public static Complex operator +(Complex a, Complex b)
        {
            return new Complex(a.real + b.real, a.img + b.img);
        }
        /// <summary>
        /// 複素数同士の差
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>差</returns>
        public static Complex operator -(Complex a, Complex b)
        {
            return new Complex(a.real - b.real, a.img - b.img);
        }
        /// <summary>
        /// 複素数同士の積
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>積</returns>
        public static Complex operator *(Complex a, Complex b)
        {
            return new Complex((a.real * b.real) - (a.img * b.img),
           (a.real * b.img + (a.img * b.real)));
        }
        /// <summary>
        /// 振幅スペクトル プロパティ
        /// </summary>
        public double magnitude
        {
            get
            {
                return Math.Sqrt(Math.Pow(real, 2) + Math.Pow(img, 2));
            }
        }
        /// <summary>
        /// 位相スペクトル プロパティ
        /// </summary>
        public double phase
        {
            get
            {
                return Math.Atan(img / real); // アークタンジェントを返し、-n/2<=theta<=n/2となる値を返す
            }
        }
        /// <summary>
        /// 他方の複素共役
        /// </summary>
        /// <returns></returns>
        public Complex ChangeToConjugate()
        {
            return new Complex(real, img * (-1));
        }
    }

    // 複素変換
    public enum ComplexFunc
    {
        DFT,
        IDFT,
        FFT,
        IFFT
    }
    // 窓関数
    public enum WindowFunc
    {
        Hamming,
        Hanning,
        Blackman,
        Rectangular
    }


    /// <summary>
    /// フーリエ変換
    /// </summary>
    public static class Fourier
    {
        /// <summary>
        /// 窓関数の実行
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
        public static Complex[] DFT(Complex[] x)
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
        public static Complex[] MusicalDFT(Complex[] x)
        {
            int N = x.Length;
            double fA0 = 27.5; int num = 11;
            Complex[] X = new Complex[12 * num];
            double theta = (-2) * Math.PI;
            
            
            for (int k = 0; k < num*12 ; k++)
            {
                X[k] = new Complex(0, 0);
                for (int n = 0; n < N; n++)
                {
                    Complex temp = Complex.from_polar(1, theta * n * Math.Pow(fA0, k/12));
                    X[k] += temp * x[n];
                }
            }
            return X;
        }
        /// <summary>
        /// 離散フーリエ逆変換
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Complex[] IDFT(Complex[] x)
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
        public static int EnableLines(int length)
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
        public static Complex[] FFT(Complex[] x)
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
        public static Complex[] IFFT(Complex[] x)
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
                    new Complex(item.real / Nmax, item.img / Nmax)
                );
            }
            return y.ToArray();
        }
    }

    /// <summary>
    /// フーリエ変換の列挙
    /// （注）動作未確認
    /// </summary>
    public static class IEnumerableFourier
    {
        public static IEnumerable<double> GasussianWindow(int Length)
        {
            //int dim = x.Count() / wLen;
            //if (dim <= 1) return Enumerable.Range(1,1).Select(c => x.Take(wLen));
            //var y = Enumerable.Range(0, dim).Select(c => x.Skip(wLen * c).Take(wLen));

            int w = EnableLines(Length);

            return Enumerable.Range(0, w).Select(c => Math.Exp((-1 / 2) * Math.Pow(2.5 * (2 * c - w) / w, 2)));
        }
        public static IEnumerable<double> Window(int length, WindowFunc windowFunc)
        {

            var y = Enumerable.Range(0, length);

            switch (windowFunc)
            {
                case WindowFunc.Hamming:
                   return y.Select((_,index) => 0.54 - 0.46 * Math.Cos(2 * Math.PI * index / (length - 1)));

                case WindowFunc.Hanning:
                    return y.Select((_, index) => 0.5 - 0.5 * Math.Cos(2 * Math.PI * index / (length - 1)));

                case WindowFunc.Blackman:
                    return y.Select((_, index) => 0.42 - 0.5 * Math.Cos(2 * Math.PI * index / (length - 1)) + 0.08 * Math.Cos(4 * Math.PI * index / (length - 1)));

                case WindowFunc.Rectangular:
                    return y.Select((_, index) => 1.0);

                default:
                    return y.Select((_, index) => 1.0);

            }
        }
        public static IEnumerable<double> Serialization(IEnumerable<IEnumerable<double>> x)
        {
            foreach(var c1 in x)
            {
                foreach (var c2 in c1)
                    yield return c2;
            }
        }
        public static IEnumerable<double> Product(IEnumerable<double> x1, IEnumerable<double> x2)
        {
            int length = x1.Count() > x2.Count() ? x2.Count() : x1.Count();
            return Enumerable.Range(0, length).Select(c => x1.ElementAt(c) * x2.ElementAt(c));
        }

        public static IEnumerable<double> Windowing(IEnumerable<double> x, WindowFunc windowFunc)
        {
            int length = EnableLines(x.Count());

            return Product(x, Window(length, windowFunc));
        }
        public static IEnumerable<double> GWindowing(IEnumerable<double> x)
        {
            int length = EnableLines(x.Count());

            return Product(x, GasussianWindow(length));
        }
        static int count = 0;
        /// </summary>
        /// 高速フーリエ変換
        /// <param name="x"></param>
        /// <returns></returns>
        public static IEnumerable<Complex> FFT(IEnumerable<Complex> x)
        {
            int N = EnableLines(x.Count());
            int tmp = count++;Console.WriteLine("{0}::{1}=>{2}", new string(Enumerable.Range(0, tmp).Select(c => ' ').ToArray()), N, N / 2);

            if (N == 1)
            {
                return new Complex[1] { new Complex(0, 0) };
            }
            else
            {
                var e = Enumerable.Range(0, N / 2).Select((_, index) => x.ElementAt(2 * index));
                var d = Enumerable.Range(0, N / 2).Select((_, index) => x.ElementAt(2 * index + 1));

                var D = FFT(d);
                var E = FFT(e);

                double d_theta = (-2) * Math.PI / N;
                D = D.Select((val, index) => val * Complex.Polar(d_theta * index));

                Console.WriteLine("{0}::{1}<={2}", new string(Enumerable.Range(0, tmp).Select(c => ' ').ToArray()), N, N / 2);

                return Enumerable.Range(0, N).Select((val, index) =>
                {
                    int k = index - N / 2;
                    if (index < N / 2) return E.ElementAt(index) + D.ElementAt(index);
                    else return E.ElementAt(k) - D.ElementAt(k);
                });
            }
        }
        public static Complex[] sFFT(Complex[] x)
        {
            #region out
            //初期宣言
            int N = EnableLines(x.Length);
            Complex[] X = new Complex[N];
            Complex[] d, D, e, E;

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
            D = sFFT(d);
            E = sFFT(e);
            double d_theta = (-2) * Math.PI / N;
            for (k = 0; k < N / 2; k++)
            {
                Complex temp = Complex.from_polar(1, d_theta * k);
                D[k] *= temp;
            }
            #endregion
            for (k = 0; k < N / 2; k++)
            {
                X[k] = E[k] + D[k]; // 正
                X[k + N / 2] = E[k] - D[k]; // 負
            }
            return X;
        }
        /// <summary>
        /// 高速逆フーリエ変換
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static IEnumerable<Complex> IFFT(IEnumerable<Complex> x)
        {
            var y = x.Select(c => {
                return c.ChangeToConjugate();
            });
            x = FFT(y);
            return x.Select(c => {
                return new Complex(c.real / x.Count(), c.img / x.Count());
            });
        }
        /// <summary>
        /// 有効データ数へ取得
        /// 末尾からデータを切り捨て
        /// </summary>
        /// <param name="length"></param>
        /// <returns>2^n<length<2^(n+1)</returns>
        private static int EnableLines(int length)
        {
            int ansLen = 1;
            //Console.Write("{");
            while (length > ansLen)
            {
                ansLen *= 2;
                //Console.Write("{0},", ansLen);
            }
            //Console.WriteLine("}");
            if (ansLen == length) return length;
            else return ansLen / 2;
        }
    }
}