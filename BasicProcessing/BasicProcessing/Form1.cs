using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicProcessing
{
    public partial class Form1 : Form
    {
        public DataList data;
        public string[] s;
        public string fileout;
        public Form1()
        {
            InitializeComponent();
            s = new string[] {
                @".\音ファイル\g1.wav",
                @".\kekka_wav.txt" };
            fileout = @".\kekka_content_wav.txt";
            data = exMain(s, fileout);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public struct WavHeader
        {
            public byte[] riffID; // "riff"
            public uint size;  // ファイルサイズ-8
            public byte[] wavID;  // "WAVE"
            public byte[] fmtID;  // "fmt "
            public uint fmtSize; // fmtチャンクのバイト数
            public ushort format; // フォーマット
            public ushort channels; // チャンネル数
            public uint sampleRate; // サンプリングレート
            public uint bytePerSec; // データ速度
            public ushort blockSize; // ブロックサイズ
            public ushort dimBit;  // 量子化ビット数
            public byte[] dataID; // "data"
            public uint dataSize; // 波形データのバイト数
        }
        public struct DataList
        {
            public List<short> lDataList;
            public List<short> rDataList;
            public WavHeader WavHeader;

            public DataList(List<short> lDataList, List<short> rDataList, WavHeader WavHeader)
            {
                this.lDataList = lDataList;
                this.rDataList = rDataList;
                this.WavHeader = WavHeader;
            }
        }

        /// <summary>
        /// このメソッドでは外部からでも呼び出せるように、静的とする
        /// 処理の中断を明確にするために、状態を保存、のちにラベル表示できる。（未）
        /// readerとwriterは分ける。（未）
        /// 静的なメソッドでは、メソッド外とのオブジェクト参照は禁止される（要出典）
        /// 
        /// このrederとwriterに求めることは、新たなwavファイルの生成
        /// </summary>
        /// <param name="args"></param>
        /// <param name="fileout"></param>
        static DataList exMain(string[] args, string fileout)
        {
            WavHeader Header = new WavHeader();
            List<short> lDataList = new List<short>();
            List<short> rDataList = new List<short>();

            using (FileStream fs = new FileStream(args[0], FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                try
                {
                    Header.riffID = br.ReadBytes(4);
                    Header.size = br.ReadUInt32();
                    Header.wavID = br.ReadBytes(4);
                    Header.fmtID = br.ReadBytes(4);
                    Header.fmtSize = br.ReadUInt32();
                    Header.format = br.ReadUInt16();
                    if(Header.format != 1)
                    {
                        
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
                    if (br != null)
                    {
                        br.Close();
                    }
                    if (fs != null)
                    {
                        fs.Close();
                    }
                }
            }
            Console.WriteLine(args[1] + "をオープンしました。");


            //ここで加工（とりあえず素通り）

            List<short> lNewDataList = lDataList;
            List<short> rNewDataList = rDataList;

            Header.dataSize = (uint)Math.Max(lNewDataList.Count, rNewDataList.Count) * 4;

            using (FileStream fs = new FileStream(args[1], FileMode.Create, FileAccess.Write))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
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
                        if (i < lNewDataList.Count)
                        {
                            bw.Write((ushort)lNewDataList[i]);
                        }
                        else
                        {
                            bw.Write(0);
                        }

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
            }
            DataList exdata = new DataList(lDataList, rDataList, Header);
            return exdata;
        }

        private static void WriteFileContent(WavHeader Header, string fileout)
        {
            string tmp;

            StreamWriter kekkaout = new StreamWriter(fileout);
            kekkaout.WriteLine("blockSize : " + Header.blockSize);
            kekkaout.WriteLine("bps : " + Header.bytePerSec);
            kekkaout.WriteLine("ch : " + Header.channels);
            if (Header.dataID != null)
            {
                tmp = System.Text.Encoding.GetEncoding("shift_jis").GetString(Header.dataID);
                kekkaout.WriteLine("dID : " + tmp);
            }
            kekkaout.WriteLine("dSize : " + Header.dataSize);
            kekkaout.WriteLine("dmBit : " + Header.dimBit);
            if (Header.fmtID != null)
            {
                tmp = System.Text.Encoding.GetEncoding("shift_jis").GetString(Header.fmtID);
                kekkaout.WriteLine("fmtID : " + tmp);
            }
            kekkaout.WriteLine("fmtSize : " + Header.fmtSize);
            kekkaout.WriteLine("format : " + Header.format);
            if (Header.riffID != null)
            {
                tmp = System.Text.Encoding.GetEncoding("shift_jis").GetString(Header.riffID);
                kekkaout.WriteLine("riffID : " + tmp);
            }
            kekkaout.WriteLine("samplingRate : " + Header.sampleRate);
            kekkaout.WriteLine("size(-8bit) : " + Header.size);
            if (Header.wavID != null)
            {
                tmp = System.Text.Encoding.GetEncoding("shift_jis").GetString(Header.wavID);
                kekkaout.WriteLine("wavID : " + tmp);
            }
            kekkaout.Close();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            WaveShow ws = new WaveShow(data.lDataList, data.rDataList);
            ws.Show();
        }
    }
}

