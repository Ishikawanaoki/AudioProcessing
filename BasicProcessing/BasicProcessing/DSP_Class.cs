using myfuntion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace myfunction2
{
    /// <summary>
    /// ディジタルフィルタの作成や
    /// 方形波や三角波の作成のために新たなクラスを作ります。
    /// このクラスでは、staticなクラス及び、staticなメソッドのみを定義し、
    /// 全てdouble型配列を返すものとします。
    /// また、ヴォリュームを1に正規化することを忘れない。耳又はオーディオに
    /// 想定外のエラーが起きる。
    /// 
    /// こちらの名前空間へのプルリクエストは優先的に見ていきます。
    /// </summary>
    class DSP_Class
    {
        private static void seikika(ref double[] data)
        {
            double max = 0;
            double min = 0;
            for(int i=0; i<data.Length; i++)
            {
                if (max < data[i]) max = data[i];
                if (min > data[i]) min = data[i];
            }
            max += (-1) * min;
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = data[i] / max * 1;
            }
        }
        public static double[] SquareWave(int Nmax, int rate)
        {
            double[] data = new double[Nmax];

            double div = 2 * Math.PI / Nmax * rate; // 2*PI*F*Tに相当
            double tmp = 0; // jにのみ作用する
            for(int i=0; i<data.Length; i++)
            {
                for(int j=0; j<30; j++)
                {
                    tmp += Math.Sin((2 * j - 1) * div) / (2 * j - 1);
                }
                data[i] = tmp*4/Math.PI; tmp = 0;
            }

            seikika(ref data);

            return data;
        }
        public int[] complexSearchF(ref double[] x, int j)
        {
            int k = x.Length / j;
            double[] xx = new double[k];

            int count = 0;
            int iMemory = 0;

            List<Complex> ans = new List<Complex>();

            Complex[] cmptmp;

            List<int> maxFs = new List<int>();

            for (int n = 0; n < j; n++)
            {
                for (int m = 0; m < k; m++)
                {
                    xx[m] = x[count++];
                }
                if (count > x.Length) break;

                xx = Fourier.Windowing(xx, Fourier.WindowFunc.Hamming);

                cmptmp = myfunction.Manual_DoDFT(xx);

                Complex max = new Complex(0, 0);
                Complex tmp;

                for (int i = 0; i < cmptmp.Length; i++)
                {
                    tmp = cmptmp[i];
                    if (max.real < tmp.real && max.img < tmp.img)
                    {
                        max = cmptmp[i];
                        iMemory = i;
                    }
                }
                maxFs.Add(iMemory);

                for (int i = 0; i < cmptmp.Length; i++)
                {
                    tmp = cmptmp[i];
                    if (max.real == tmp.real && max.img ==tmp.img)
                    {
                        cmptmp[i] = tmp;
                    }
                    else
                    {
                        cmptmp[i] = new Complex(0, 0);
                    }
                    // ans へ k 個の要素を追加
                    ans.AddRange(cmptmp);
                }

            }
            cmptmp = ans.ToArray();
            // 参照渡しによる、計算結果の更新
            // ここでのxでは、短時間(Div[t] = T / j *但し、後尾は切り捨て）
            x = comToTime(cmptmp, j);
            return maxFs.ToArray();
        }
        /// <summary>
        /// 「任意の周波数の正弦波を創りだす」
        /// これを呼び出すメソッド、つまり前段のメソッドにより、
        /// 任意にスペクトルを減らしたデータが x となる。
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public double[] comToTime(Complex[] x, int j)
        {
            int k = x.Length / j;
            Complex[] cmptmp = new Complex[k];
            List<double> sign_data = new List<double>();

            int count = 0;

            for (int n = 0; n < j; n++)
            {
                for (int m = 0; m < k; m++) cmptmp[m] = x[count++];

                if (count > x.Length) break;

                sign_data.AddRange(myfunction.DoIDFT(cmptmp));
            }

            double[] ans = sign_data.ToArray();
            return ans;
        }
        public int[] comToTime01(int length_x, int j, int[] maxFs)
        {
            int k = length_x / j;
            
            int[] targetF = new int[k];
            int targetI = 0;
            foreach (int l in maxFs)
            {
                double tmp = l;

                targetF[targetI++]= (int)(44100 / tmp);
            }
            return targetF;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public double[] comToGenSignal(Complex[] x, int j)
        {

            double[] ans = new double[x.Length];
            return ans;
        }
    }
}
