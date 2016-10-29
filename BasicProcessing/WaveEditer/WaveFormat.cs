using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveEditer
{
    public static class Staff
    {
        public const string root = @"..\..\..\BasicProcessing\音ファイル\";
    }
    public static class WaveReAndWr
    {
        /// <summary>
        /// The header is used to provide specifications on the file type,
        /// sample rate,
        /// sample size
        /// and bit size of the file,
        /// as well as its overall length.
        /// The header of a WAV(RIFF) file is 44 bytes long.
        /// （約）ヘッダーの大きさは、ファイルタイプや標本化周波数、標本数、ファイルサイズによって変わる。
        /// ＊基準として 44byte を取るとすると、
        /// 2番目 size := filesize    -  8
        /// 13番目 data := filesize   - 14
        /// また、
        /// bytePerSec, blockSizeは相関値である。
        /// その為ファイルを更新したい場合には次のメンバのみが独立で、変更可能である。（推定、要確認）
        ///     dimBit
        /// 
        /// *filesize = size+8 = dataSize+8
        /// 
        /// </summary>
        public struct WavHeader
        {
            public byte[] riffID;       // (固定)"riff"
            public uint size;           // ファイルサイズ-8, Typically, you'd fill this in after creation.
            public byte[] wavID;        // (固定)"WAVE"
            public byte[] fmtID;        // (固定)"fmt "
            public uint fmtSize;        // (固定)fmtチャンクのバイト数, Length of format data.  Always 16
            public ushort format;       // (固定)フォーマット, Wave type PCM mustbe 1
            public ushort channels;     // (固定)チャンネル数 = 2(stereo)
            public uint sampleRate;     // (固定)サンプリングレート = 44100(CD基準)
            public uint bytePerSec;     //（相関）データ速度 = 176400 = SampleRate * 4
            public ushort blockSize;    //（相関）ブロックサイズ = 4
            public ushort dimBit;       // 量子化ビット数(Bits per sample) = 16
            public byte[] dataID;       // (固定)"data"
            public uint dataSize;       // 波形データのバイト数, ファイルサイズ-44

            // bytePerSec := (sampleRate * dimBit * channels) / 8
            // blockSize  := (BitsPerSample * Channels) / 8
        }
        /// <summary>
        /// 
        /// </summary>
        public struct DataList<T>
        {
            public List<T> lDataList;
            public List<T> rDataList;
            public WavHeader WavHeader;

            public DataList(List<T> lDataList, List<T> rDataList, WavHeader WavHeader)
            {
                this.lDataList = lDataList;
                this.rDataList = rDataList;
                this.WavHeader = WavHeader;
            }
        }
        public static WavHeader GetHeader()
        {
            string defoult = Staff.root + "a1.wav";

            WavHeader Header = new WavHeader();

            using (FileStream fs = new FileStream(defoult, FileMode.Open, FileAccess.Read))
            using (System.IO.BinaryReader br = new BinaryReader(fs))
            {
                #region 読み込み作業
                try
                {
                    Header.riffID = br.ReadBytes(4);
                    Header.size = br.ReadUInt32();
                    Header.wavID = br.ReadBytes(4);
                    Header.fmtID = br.ReadBytes(4);
                    Header.fmtSize = br.ReadUInt32();
                    Header.format = br.ReadUInt16();
                    Header.channels = br.ReadUInt16();
                    Header.sampleRate = br.ReadUInt32();
                    Header.bytePerSec = br.ReadUInt32();
                    Header.blockSize = br.ReadUInt16();
                    Header.dimBit = br.ReadUInt16();
                    Header.dataID = br.ReadBytes(4);
                    Header.dataSize = br.ReadUInt32();
                    // 以下 br 切り捨て
                }
                finally
                {
                    if (br != null) br.Close();
                    if (fs != null) fs.Close();
                }
                #endregion
            }

            return Header;
        }
        /// <summary>
        /// このメソッドでは外部からでも呼び出せるように、静的とする
        /// 処理の中断を明確にするために、状態を保存、のちにラベル表示できる。（未）
        /// readerとwriterは分ける。（未）
        /// 静的なメソッドでは、メソッド外とのオブジェクト参照は禁止される（要出典）
        /// このrederとwriterに求めることは、新たなwavファイルの生成
        /// </summary>
        /// <param name="args"></param>
        /// 入力側のWaveファイル
        /// ここでの処理ではリニアPCMであるWaveファイルしか処理されません。
        /// <param name="fileout"></param>
        /// デフォルトのヘッダー情報の保存先
        /// if分岐が無効であれば適当な文字列でもよいです。
        /// <param name="flag"></param>
        /// <returns></returns>
        public static DataList<short> WavReader(string UniquName)
        {
            WavHeader Header = new WavHeader();
            List<short> lDataList = new List<short>();
            List<short> rDataList = new List<short>();

            using (FileStream fs = new FileStream(UniquName, FileMode.Open, FileAccess.Read))
            using (System.IO.BinaryReader br = new BinaryReader(fs))
            {
                #region 読み込み作業
                try
                {
                    Header.riffID = br.ReadBytes(4);
                    Header.size = br.ReadUInt32();
                    Header.wavID = br.ReadBytes(4);
                    Header.fmtID = br.ReadBytes(4);
                    Header.fmtSize = br.ReadUInt32();
                    Header.format = br.ReadUInt16();
                    if (Header.format != 1) // 入力がリニアPCM出ない時のダミー操作
                    {
                        WavHeader dm = new WavHeader();
                        DataList<short> dmd = new DataList<short>(lDataList, rDataList, dm);
                        return dmd;
                    }
                    Header.channels = br.ReadUInt16();
                    Header.sampleRate = br.ReadUInt32();
                    Header.bytePerSec = br.ReadUInt32();
                    Header.blockSize = br.ReadUInt16();
                    Header.dimBit = br.ReadUInt16();
                    Header.dataID = br.ReadBytes(4);
                    Header.dataSize = br.ReadUInt32();

                    for (int i = 0; i < Header.dataSize / Header.blockSize; i++)
                    {
                        lDataList.Add((short)br.ReadUInt16());
                        rDataList.Add((short)br.ReadUInt16());
                    }
                }
                finally
                {
                    if (br != null) br.Close();
                    if (fs != null) fs.Close();
                }
                #endregion
            }


            DataList<short> datalist = new DataList<short>(lDataList, rDataList, Header);
            return datalist;

        }
        /// <summary>
        /// ﾊﾞｲﾅﾘ書き込み先 args、DataListを引数
        /// </summary>
        /// <param name="args"></param>
        /// <param name="datalist"></param>
        public static void WavWriter(string UniquName, DataList<short> datalist)
        {

            List<short> lNewDataList = datalist.lDataList;
            List<short> rNewDataList = datalist.rDataList;
            WavHeader Header = datalist.WavHeader;

            Header.dataSize = (uint)Math.Max(lNewDataList.Count, rNewDataList.Count) * 4;

            using (System.IO.FileStream fs = new FileStream(Staff.root + UniquName, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                #region 書き込み作業
                try
                {
                    bw.Write(Header.riffID);
                    bw.Write(Header.size);
                    bw.Write(Header.wavID);
                    bw.Write(Header.fmtID);
                    bw.Write(Header.fmtSize);
                    bw.Write(Header.format);
                    bw.Write(Header.channels);
                    bw.Write(Header.sampleRate);
                    bw.Write(Header.bytePerSec);
                    bw.Write(Header.blockSize);
                    bw.Write(Header.dimBit);
                    bw.Write(Header.dataID);
                    bw.Write(Header.dataSize);

                    for (int i = 0; i < Header.dataSize / Header.blockSize; i++)
                    {
                        // 1st IF turning point
                        if (i < lNewDataList.Count)
                        {
                            bw.Write((ushort)lNewDataList[i]);
                        }
                        else
                        {
                            bw.Write(0);
                        }

                        // 2st IF turning point
                        if (i < rNewDataList.Count)
                        {
                            bw.Write((ushort)rNewDataList[i]);
                        }
                        else
                        {
                            bw.Write(0);
                        }
                    }
                }
                finally
                {
                    if (bw != null) bw.Close();
                    if (fs != null) fs.Close();
                }
                #endregion
            }
        }
        public static void DefWavWriter(string UniquName, IEnumerable<double> query)
        {
            WavHeader Header = GetHeader();

            Header.dataSize = (uint)query.Count() * 4;

            using (System.IO.FileStream fs = new FileStream(Staff.root + UniquName, FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                #region 書き込み作業
                try
                {
                    bw.Write(Header.riffID);
                    bw.Write(Header.size);
                    bw.Write(Header.wavID);
                    bw.Write(Header.fmtID);
                    bw.Write(Header.fmtSize);
                    bw.Write(Header.format);
                    bw.Write(Header.channels);
                    bw.Write(Header.sampleRate);
                    bw.Write(Header.bytePerSec);
                    bw.Write(Header.blockSize);
                    bw.Write(Header.dimBit);
                    bw.Write(Header.dataID);
                    bw.Write(Header.dataSize);

                    ushort tmp = 0;
                    for (int i = 0; i < Header.dataSize / Header.blockSize; i++)
                    {
                        try
                        {
                            tmp = Convert.ToUInt16(query.ElementAtOrDefault(i));
                        }
                        catch (FormatException e)
                        {
                            Console.WriteLine("Input string is not a sequence of digits.");
                        }
                        catch (OverflowException e)
                        {
                            Console.WriteLine("The number cannot fit in an Int32.");
                        }

                        bw.Write(tmp);

                        bw.Write(tmp);
                    }
                }
                finally
                {
                    if (bw != null) bw.Close();
                    if (fs != null) fs.Close();
                }
                #endregion
            }
        }
        /// <summary>
        /// バイナリファイルの行数を取得し、
        /// その値以下の最大の2の乗数を返します。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static int GetLinesOfTextFile(string UniquName)
        {
            StreamReader StReader = new StreamReader(Staff.root + UniquName);
            int LineCount = 0; int LineValidCount = 2;
            while (StReader.Peek() >= 0)
            {
                StReader.ReadLine();
                LineCount++;
            }
            StReader.Close();
            while (LineCount >= LineValidCount) LineValidCount *= 2;
            LineValidCount /= 2;
            return LineValidCount;
        }
    }

}
