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
            /*
            private void test_Filter()
            {
                chart1_state = true; chart2_state = true;

                //Ldata
                function.ActiveComplex ac = new function.ActiveComplex(lDataList.ToArray(), function.Fourier.WindowFunc.Hamming);

                //Plot(chart1, ac.HighPassDSP(2000).ToArray());
                //Plot(chart2, ac.LowPassDSP(2000).ToArray());
            }
            private void CompareWindows()
            {
                //testMyAnalys(2000);
                //DSP.FrequencyDomein.TestPitchDetect test = new DSP.FrequencyDomein.TestPitchDetect(100, lDataList);
                //test.Execute();
                List<Fourier.WindowFunc> func = new List<Fourier.WindowFunc>();
                func.Add(Fourier.WindowFunc.Blackman);
                func.Add(Fourier.WindowFunc.Hamming);
                func.Add(Fourier.WindowFunc.Hanning);
                func.Add(Fourier.WindowFunc.Rectangular);
                ActiveComplex ac;
                double[] lData; double[] rData; string filename;
                WaveReAndWr.DataList<double> dlist;
                foreach (function.Fourier.WindowFunc fu in func)
                {
                    ac = new ActiveComplex(lDataList.ToArray(), fu);
                    lData = ac.FunctionTie();
                    ac = new ActiveComplex(rDataList.ToArray(), fu);
                    rData = ac.FunctionTie();
                    filename = root + @"\sound_" + fu.ToString() + ".wav";
                    dlist = new WaveReAndWr.DataList<double>(
                            new List<double>(lData),
                            new List<double>(rData),
                            header);

                    WaveReAndWr.WavWriter(filename,
                        function.File.ConvertDoubletoShort(dlist, 10));
                    Console.WriteLine("{0}が保存されました。", filename);
                }
            }
            */
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

            /// <summary>
            /// 
            /// </summary>
            /// <param name="divnum">波形の等分する分割数</param>
            /*private void testMyAnalys()
            {
                double[] RspeAna;
                double[] LspeAna;

                // 短時間フーリエ変換するための格納・実行クラスの生成
                DSP.ComplexStaff ex;


                ex = new DSP.ComplexStaff(divnum, lDataList);
                LspeAna = ex.DoSTDFT(rank);
                Console.WriteLine("LFの実行");

                // 結果のグラフ表示
                // 左側の波形について
                // chart1 : 実行前の波形
                // chart2 : 実行後の波形
                Plot(chart1, lDataList);
                Plot(chart2, LspeAna);

                ex = new DSP.ComplexStaff(divnum, rDataList);
                RspeAna = ex.DoSTDFT(rank);
                Console.WriteLine("RFの実行");

                string filename = root + @"\mypractice.wav";
                WaveReAndWr.DataList<double> dlist
                    = new WaveReAndWr.DataList<double>(
                        new List<double>(LspeAna),
                        new List<double>(RspeAna), 
                        header);

                //function.File.Write(filename, dlist, 5);
                WaveReAndWr.WavWriter(filename, function.File.ConvertDoubletoShort(dlist));
                Console.WriteLine("{0}を保存しました", filename);
            }*/
        }
    }
}
