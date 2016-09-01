using function;
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
        class ComplexStaff
        {
            private int dividedNum;
            private double[] rawSign;
            private int shortLength;

            private double[] shortSign;
            //private int indexshortSign;

            public int DividedNum
            {
                set { this.dividedNum = value; }
                // get {}
            }
            public double[] RawSign
            {
                set
                {
                    rawSign = value;

                    // any process
                    if( dividedNum > 0)
                    {
                        shortLength = (int)( rawSign.Length / dividedNum );
                        // memory assign
                        shortSign = new double[shortLength];
                    }
                }
            }
            public double[] MaxIndex(int groupIndex, ref int ansIndex, ref double[] speAna)
            {
                if (!(dividedNum > 0))
                {
                    return null;
                }
                else {
                    FitShortSign(groupIndex, rawSign);

                    shortSign = Fourier.Windowing( shortSign, Fourier.WindowFunc.Hamming );

                    Complex[] cmptmp = myfunction.Manual_DoDFT( shortSign );


                    speAna = SeeMagnitude(cmptmp);
                    ansIndex = ComplexProcessing( ref cmptmp ); // cmptmp is changed
                    return myfunction.DoIDFT(cmptmp);
                }
                
            }
            private void FitShortSign (int groupIndex, double[] sign)
            {
                int tmp;
                // if (shortLength != (sign.Length / dividedNum)) return;
                for (int i = 0; i < shortLength; i++)
                {
                    tmp = (shortLength * groupIndex) + i;
                    if (tmp > sign.Length) break; // avoid null access
                    shortSign[i] = sign[tmp];
                }
            }
            private int ComplexProcessing( ref Complex[] cmptmp )
            {
                Complex tmp;

                Complex max = new Complex(0, 0);
                int indextmp = 0;

                for (int i = 0; i < cmptmp.Length; i++)
                {
                    tmp = cmptmp[i];
                    if (max.real < tmp.real && max.img < tmp.img)
                    {
                        max = cmptmp[i];
                        indextmp = i;
                    }
                }

                for (int i = 0; i < cmptmp.Length; i++)
                {
                    tmp = cmptmp[i];
                    if (max.real == tmp.real && max.img == tmp.img)
                    {
                        // OK
                        //cmptmp[i] = tmp;
                    }
                    else
                    {
                        //NG
                        cmptmp[i] = new Complex(0, 0);
                    }
                }
                return indextmp;
            }
            private double[] SeeMagnitude(Complex[] cmp)
            {
                double[] speAna = new double[cmp.Length];
                for (int i = 0; i < cmp.Length; i++)
                    speAna[i] = cmp[i].magnitude;
                return speAna;
            }
        }
        /// <summary>
        /// このクラスで唯一呼び出せるメソッドで次の操作を行うものとする
        ///  - 短い時間に分割された時系列で離散フーリエし、任意の操作をして時系列へ戻す
        ///  -- この操作での結果を複数のパラメータで扱う
        ///  ---    int[] : 返却値 : 各単位時間での採用した周波数を示す添え字、double[]で周波数値を返すようにする
        ///  ---    double[] : 参照渡し変数 : 操作した後の時系列
        ///  ---    double[] : 参照渡し変数 : 操作した後のスペクトルグラフ
        ///  - 
        /// </summary>
        /// <param name="rawSign"></param>
        /// <param name="dividedNum"></param>
        /// <returns></returns>
        public int[] complexSearchv02(ref double[] rawSign, int dividedNum, ref double[] speAna)
        {

            List<double> newSign = new List<double>();
            List<int> maxIndexByGroupe = new List<int>();

            List<double> cList = new List<double>(); 

            ComplexStaff cs = new ComplexStaff();
            cs.DividedNum = dividedNum; 
            cs.RawSign = rawSign;

            for(int i=0; i< dividedNum; i++)
            {
                int tmp = 0;
                double[] ctmp = new double[ rawSign.Length / dividedNum ];
                newSign.AddRange(
                    cs.MaxIndex( i, ref tmp, ref ctmp)
                    );
                maxIndexByGroupe.Add(tmp);
                cList.AddRange(ctmp);
            }
            
            // end process

            rawSign = newSign.ToArray();
            return maxIndexByGroupe.ToArray();
        }
        public int[] complexSearchv01(ref double[] RawSign, int dividedNum)
        {
            // 変数
            int indexRawSign = 0;
            // [定数]
            int ShortLength = RawSign.Length / dividedNum;
            // 変数
            double[] ShortSign = new double[ShortLength];
            // 変数
            int indexShortSign = 0;

            List<Complex> ans = new List<Complex>();

            Complex[] cmptmp;

            List<int> maxIndex = new List<int>();

            for (int n = 0; n < dividedNum; n++)
            {
                for (int m = 0; m < ShortLength; m++) ShortSign[m] = RawSign[indexRawSign++];
                if (indexRawSign > RawSign.Length) break;

                ShortSign = Fourier.Windowing(ShortSign, Fourier.WindowFunc.Hamming);
                cmptmp = myfunction.Manual_DoDFT(ShortSign);


                Complex max = new Complex(0, 0);    // 最も大きなスペクトル
                Complex tmp;                        // 各短時間毎に、先頭から、周波数解析結果を取得

                // 第一次周波数探索実行
                // 　最大のスペクトル長と、その添え字 i を取得
                for (int i = 0; i < cmptmp.Length; i++)
                {
                                tmp = cmptmp[i];
                                if (max.real < tmp.real && max.img < tmp.img)
                                {
                                    max = cmptmp[i];
                                    indexShortSign = i;
                                }
                }
                maxIndex.Add(indexShortSign);
                // 第一次周波数探索実行
                // 　最大のスペクトル長以外は0へ変換する
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
            RawSign = comToTime(cmptmp, dividedNum);
            return maxIndex.ToArray();
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
