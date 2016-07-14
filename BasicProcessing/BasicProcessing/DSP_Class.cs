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
    }
}
