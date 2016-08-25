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
        /// <summary>
        /// 線スペクトルの大きさを比較し、
        /// 
        /// </summary>
        /// <param name="x">時系列データ</param>
        /// <param name="j">時・分割数</param>
        /// <returns></returns>
        public double[] complexAnalysc01(double[] x, int j, int rank)
        {
            // x.length <= k * j
            // サンプル数 <= 一区切りのサンプル数 * 時分割数
            int k = x.Length / j;

            // 一区切りと等しい長さの配列
            double[] xx = new double[k];

            int count = 0;
            //List<double> ans = new List<double>();
            //double[] ans_spec;

            // List<short> sign_s1 = new List<short>();
            //List<double> sign_d1 = new List<double>();


            //List<short> sign_s2 = new List<short>();
            //List<double> sign_d2 = new List<double>();
            Complex[] cmptmp;

            List<Complex> ans = new List<Complex>();

            // 時系列データの先頭から、kサンプルごと、終端のサンプルは消す
            for (int n = 0; n < j; n++)
            {
                for (int m = 0; m < k; m++)
                {
                // (i) kサンプル取り出し、配列xxに格納
                    xx[m] = x[count++];
                }
                // 終端到達のため、ループ脱出
                if (count > x.Length) break;

                // (ii) 取り出した一部の時系列データへ窓関数を作用させる
                xx = Fourier.Windowing(xx, Fourier.WindowFunc.Hamming);

                // (iii) 短時間での【高速】フーリエ変換
                // 最終的な ans の要素数 = k_0 + k_1 + ... + k_j = k * j
                // 複数の周波数分析結果が　連なって格納している。
                //ans.AddRange(myfunction.DoFFT(xx)); // double 配列を返す

                cmptmp = myfunction.Manual_DoDFT(xx); // Complex 配列を返す


                // 短時間での周波数分析を時系列データ(double)へ戻す
                // 聞こえ方の変化は？
                //sign_d1.AddRange(myfunction.DoIDFT(cmptmp));

                // 短時間での周波数分析を時系列データ(short)へ戻す
                // 聞こえ方の変化は？
                //sign_s1.AddRange(myfunction.Do_s_IDFT(cmptmp));


                // 上位第 "rank" 位までの複素数列を宣言、全て0での初期化する。
                Complex[] max = new Complex[rank];
                for (int i = 0; i < max.Length; i++) max[i] = new Complex(0, 0);

                // 逐次処理のための、動径を格納するための tmp
                double tmp = 0;
                for (int i = 0; i < cmptmp.Length; i++)
                {
                    tmp = cmptmp[i].magnitude;

                         if (max[4].magnitude < tmp) max[4] = cmptmp[i];
                    else if (max[3].magnitude < tmp) max[3] = cmptmp[i];
                    else if (max[2].magnitude < tmp) max[2] = cmptmp[i];
                    else if (max[1].magnitude < tmp) max[1] = cmptmp[i];
                    else if (max[0].magnitude < tmp) max[0] = cmptmp[i];
                }
                for (int i = 0; i < cmptmp.Length; i++)
                {
                    tmp = cmptmp[i].magnitude;
                         if (max[4].magnitude == tmp) cmptmp[i] = max[4];                    
                    else if (max[3].magnitude == tmp) cmptmp[i] = max[3];                    
                    else if (max[2].magnitude == tmp) cmptmp[i] = max[2];                   
                    else if (max[1].magnitude == tmp) cmptmp[i] = max[1];                   
                    else if (max[0].magnitude == tmp) cmptmp[i] = max[0];                   
                    else                              cmptmp[i] = new Complex(0, 0);
                }
                ans.AddRange(cmptmp);
            }

            return comToTime(ans.ToArray(), j);
        }
        public double[] complexAnalysc02(double[] x, int j, int  rank)
        {
            int k = x.Length / j;
            double[] xx = new double[k];
            
            int count = 0;

            List<Complex> ans = new List<Complex>();

            Complex[] cmptmp;


            for (int n = 0; n < j; n++)
            {
                for (int m = 0; m < k; m++)
                {
                    xx[m] = x[count++];
                }
                if (count > x.Length) break;

                xx = Fourier.Windowing(xx, Fourier.WindowFunc.Hamming);

                cmptmp = myfunction.Manual_DoDFT(xx);

                Complex[] max = new Complex[5];
                for (int i = 0; i < rank; i++)
                    max[i] = new Complex(0, 0);
                
                Complex tmp;
                for (int i = 0; i < cmptmp.Length; i++)
                {
                    tmp = cmptmp[i];
                         if (max[4].real < tmp.real && max[4].img < tmp.img) max[4] = cmptmp[i];                    
                    else if (max[3].real < tmp.real && max[3].img < tmp.img) max[3] = cmptmp[i];                    
                    else if (max[2].real < tmp.real && max[2].img < tmp.img) max[2] = cmptmp[i];                    
                    else if (max[1].real < tmp.real && max[1].img < tmp.img) max[1] = cmptmp[i];
                    else if (max[0].real < tmp.real && max[0].img < tmp.img) max[0] = cmptmp[i];
                }
                for (int i = 0; i < cmptmp.Length; i++)
                {
                    tmp = cmptmp[i];
                         if (max[4].real == tmp.real && max[4].img == tmp.img) cmptmp[i] = max[4];                    
                    else if (max[3].real == tmp.real && max[3].img == tmp.img) cmptmp[i] = max[3];                    
                    else if (max[2].real == tmp.real && max[2].img == tmp.img) cmptmp[i] = max[2];                    
                    else if (max[1].real == tmp.real && max[1].img == tmp.img) cmptmp[i] = max[1];                    
                    else if (max[0].real == tmp.real && max[0].img == tmp.img) cmptmp[i] = max[0];                    
                    else                                                       cmptmp[i] = new Complex(0, 0);                    
                }
                ans.AddRange(cmptmp);
            }
            return comToTime(ans.ToArray(), j);
            
        }
        public double[] comToTime(Complex[] x, int j)
        {
            int k = x.Length / j;
            Complex[] cmptmp = new Complex[k];
            List<double> sign_data = new List<double>();

            int count = 0;

            for (int n = 0; n < j; n++)
            {
                for (int m = 0; m < k; m++)
                {
                    cmptmp[m] = x[count++];
                }
                if (count > x.Length) break;

                sign_data.AddRange(myfunction.DoIDFT(cmptmp));
            }

            double[] ans = sign_data.ToArray();
            return ans;
        }
    }
}
