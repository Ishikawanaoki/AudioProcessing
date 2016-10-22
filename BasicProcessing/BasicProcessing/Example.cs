using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicProcessing
{
    namespace Example
    {
        class Dafault
        {

        }
        class ExMade
        {

            private string root = @"..\..\音ファイル";
            public readonly int[] rank;
            public const int divnum = 2000;
            private double[] getSampleWave2()
            {
                double[] ans = new double[200];
                for (int i = 0; i < ans.Length; i++)
                    ans[i] = 0;
                return ans;
            }
            private List<double> getSampleWave(double A, double f0, double fs, double length)
            {
                List<double> list = new List<double>();
                //for (int i = 0; i < length; i++)
                for (int n = 0; n < (length * fs); n++)
                {
                    list.Add(
                        //A * Math.Sin(2 * Math.PI * f0 * i / length)
                        A * Math.Sin(2 * Math.PI * f0 * n / fs)
                        );
                }
                return list;
            }
        }
    }
}
