using System;
using System.IO;
using System.Text;
using libwz.AES;
using libwz.Text;

namespace libwz.IO
{
    /// <summary> 使用little-endian的編碼模式讀取wz文件的資料流 </summary>
    public class WzFileStream : IDisposable
    {
        /// <summary> 序列化文字池 </summary>
        public SerializeStringPool StringPool { get; private set; }

        /// <summary> 基底資料流 </summary>
        public Stream BaseStream { get; private set; }

        /// <summary> 加密金鑰 </summary>
        public WzKeyType KeyType { get; private set; }

        /// <summary> 資料流起始點 </summary>
        public long BaseOffset { get; set; }

        /// <summary> 使用動態讀取。這會使<see cref="WzImage"/>在讀取時略過一些資料，以避免佔用太多記憶體 </summary>
        public bool DynamicRead { get; set; }

        /// <summary> 取得資料流的總長度(位元組) </summary>
        public long Length
        {
            get
            {
                if (this.BaseStream != null)
                {
                    return this.BaseStream.Length;
                }
                else
                {
                    return -1L;
                }
            }
        }

        /// <summary> 設定或取得資料流的位置 </summary>
        public long Position
        {
            get
            {
                if (this.BaseStream != null)
                {
                    return this.BaseStream.Position;
                }
                else
                {
                    return -1L;
                }
            }
            set
            {
                if (this.BaseStream != null)
                {
                    this.BaseStream.Position = value;
                }
            }
        }

        /// <summary> 使用指定的模式建立<see cref="WzFileStream"/>實體 </summary>
        /// <param name="path"> 檔案路徑 </param>
        /// <param name="mode"> 開啟模式 </param>
        /// <param name="key"> 加密金鑰 </param>
        public WzFileStream(string path, FileMode mode, WzKeyType key = WzKeyType.None)
        {
            this.Init(File.Open(path, mode), key);
        }

        /// <summary> 使用指定的資料流和金鑰建立<see cref="WzFileStream"/>實體 </summary>
        /// <param name="stream"> 檔案資料流 </param>
        /// <param name="key"> 加密金鑰 </param>
        public WzFileStream(Stream stream, WzKeyType key = WzKeyType.None)
        {
            this.Init(stream, key);
        }

        /// <summary> 指定資料流指向的位置 </summary>
        /// <param name="off"> 新的資料流位置 </param>
        /// <param name="usebase"> 是否從基址開始算起 </param>
        public void Seek(long off, bool usebase = false)
        {
            if (this.BaseStream != null)
            {
                this.BaseStream.Position = off + (usebase ? this.BaseOffset : 0);
            }
        }

        /// <summary> 跳過指定位元組大小的資料，並移動資料流指向的位置 </summary>
        /// <param name="size"> 需要跳過的位元組數 </param>
        public void Skip(long size)
        {
            if (this.BaseStream != null)
                this.BaseStream.Position += size;
        }

        /// <summary> 取得目前資料流所指向的位置 </summary>
        /// <param name="usebase"> 是否從基址開始算起 </param>
        public long Tell(bool usebase = false)
        {
            if (this.BaseStream != null)
                return this.BaseStream.Position - (usebase ? this.BaseOffset : 0);
            else
                return -1L;
        }

        /// <summary> 清除資料流的緩衝資料，並將緩衝資料寫入基礎裝置中 </summary>
        public void Flush()
        {
            if (this.BaseStream != null)
                this.BaseStream.Flush();
        }

        /// <summary> 釋放<see cref="WzFileStream"/>所使用的所有資源 </summary>
        public void Dispose()
        {
            this.Dispose(true);
        }

        /// <summary> 釋放<see cref="WzFileStream"/>所使用的資源 </summary>
        /// <param name="closeBase"> 是否連同Basestream一同釋放 </param>
        public void Dispose(bool closeBase)
        {
            if (closeBase)
            {
                if (this.BaseStream != null)
                {
                    this.BaseStream.Flush();
                    this.BaseStream.Close();
                    this.BaseStream.Dispose();
                }
                if (this.StringPool != null)
                {
                    this.StringPool.Dispose();
                    this.StringPool = null;
                }
            }
            this.BaseStream = null;
        }

        // read functions
        /// <summary> 從目前的資料流讀取指定位元組大小的資料到指定的緩衝區內，並移動資料流指向的位置 </summary>
        /// <param name="buffer"> 接收資料的資料緩衝區 </param>
        /// <param name="length"> 需要讀取的資料長度 </param>
        /// <param name="decrypt"> 是否需要進行解密 </param>
        public void Read(byte[] buffer, int length, bool decrypt = false)
        {
            this.BaseStream.Read(buffer, 0, length);

            if (decrypt)
            {
                WZUtil.XORKey(buffer, length, this.KeyType);
            }
        }

        /// <summary> 從目前的資料流讀取指定位元組大小的資料，並移動資料流指向的位置 </summary>
        /// <param name="length"> 需要讀取的資料長度 </param>
        /// <param name="decrypt"> 是否需要進行解密 </param>
        /// <returns></returns>
        public byte[] Read(int length, bool decrypt = false)
        {
            byte[] buffer = new byte[length];
            this.Read(buffer, length, decrypt);
            return buffer;
        }

        /// <summary> 讀取一個<see cref="bool"/>值並將資料流前進1個byte </summary>
        public bool ReadBool()
        {
            return this.Read1u() == 1;
        }

        /// <summary> 讀取一個<see cref="byte"/>值並將資料流前進1個byte </summary>
        public byte Read1u()
        {
            return (byte)this.Read(1)[0];
        }

        /// <summary> 讀取一個<see cref="ushort"/>值並將資料流前進2個byte </summary>
        public ushort Read2u()
        {
            byte[] buf = this.Read(2);
            return (ushort)(buf[0] | (buf[1] << 8));
        }

        /// <summary> 讀取一個<see cref="uint"/>值並將資料流前進4個byte </summary>
        public uint Read4u()
        {
            byte[] buf = this.Read(4);
            return (uint)(buf[0] | (buf[1] << 8) | (buf[2] << 16) | (buf[3] << 24));
        }

        /// <summary> 讀取一個<see cref="ulong"/>值並將資料流前進8個byte </summary>
        public ulong Read8u()
        {
            byte[] buf = this.Read(8);
            return (buf[0] | ((ulong)buf[1] << 8) | ((ulong)buf[2] << 16) | ((ulong)buf[3] << 24) | ((ulong)buf[4] << 32) | ((ulong)buf[5] << 40) | ((ulong)buf[6] << 48) | ((ulong)buf[7] << 56));
        }

        /// <summary> 讀取一個<see cref="sbyte"/>值並將資料流前進1個byte </summary>
        public sbyte Read1()
        {
            return (sbyte)this.Read1u();
        }

        /// <summary> 讀取一個<see cref="short"/>值並將資料流前進2個byte </summary>
        public short Read2()
        {
            return (short)this.Read2u();
        }

        /// <summary> 讀取一個<see cref="int"/>值並將資料流前進4個byte </summary>
        /// <param name="compressed"> 是否為被壓縮的資料 </param>
        public int Read4(bool compressed = false)
        {
            if (compressed)
            {
                int val = this.Read1();
                if (val != -128)
                {
                    return val;
                }
            }

            return (int)this.Read4u();
        }

        /// <summary> 讀取一個<see cref="long"/>值並將資料流前進8個byte </summary>
        /// <param name="compressed"> 是否為被壓縮的資料 </param>
        public long Read8(bool compressed = false)
        {
            if (compressed)
            {
                long val = this.Read1();
                if (val != -128)
                {
                    return val;
                }
            }

            return (long)this.Read8u();
        }

        /// <summary> 讀取一個<see cref="float"/>值並將資料流前進4個byte </summary>
        /// <param name="compressed"> 是否為被壓縮的資料 </param>
        public float ReadFloat(bool compressed = false)
        {
            if (compressed)
            {
                int val = this.Read1();
                if (val != -128)
                {
                    return val;
                }
            }

            return BitConverter.ToSingle(this.Read(4), 0);
        }

        /// <summary> 讀取一個<see cref="double"/>值並將資料流前進8個byte </summary>
        public double ReadDouble()
        {
            return BitConverter.ToSingle(this.Read(8), 0);
        }

        /// <summary> 以指定的編碼讀取一串文字並將資料流往前進 </summary>
        /// <param name="len"> 文字的長度 </param>
        /// <param name="codepage"> 文字編碼 </param>
        /// <param name="decrypt"> 是否需要進行解密 </param>
        public string ReadString(int len, Encoding codepage = null, bool decrypt = false)
        {
            if (codepage == null)
            {
                codepage = Encoding.ASCII;
            }

            return codepage.GetString(this.Read(len, decrypt));
        }

        /// <summary> 從讀取一串資料並將結果輸出到指定的資料流中 </summary>
        /// <param name="dest"> 目的資料流 </param>
        /// <param name="offset"> 讀取資料的起始點 </param>
        /// <param name="size"> 讀取大小 </param>
        public void ReadDataToStream(Stream dest, long offset, int size)
        {
            const int BufferSize = 0x1000;

            this.Seek(offset);

            // read data
            int blockCount = (size / BufferSize);
            int remains = size % BufferSize;
            byte[] buffer = new byte[BufferSize];

            for (int i = 0; i < blockCount; ++i)
            {
                this.Read(buffer, BufferSize);
                dest.Write(buffer, 0, BufferSize);
            }
            if (remains > 0)
            {
                this.Read(buffer, remains);
                dest.Write(buffer, 0, remains);
            }
        }

        // write function
        /// <summary> 將指定位元組大小的資料寫入到緩衝區中，並將資料流位置往前移動 </summary>
        /// <param name="buffer"> 要寫入的資料 </param>
        /// <param name="length"> 要寫入的長度 </param>
        /// <param name="encrypt"> 是否要進行加密 </param>
        public void Write(byte[] buffer, int length, bool encrypt = false)
        {
            if (encrypt)
            {
                WZUtil.XORKey(buffer, length, this.KeyType);
            }

            this.BaseStream.Write(buffer, 0, length);
        }

        /// <summary> 將一組資料寫入到緩衝區中，並將資料流位置往前移動 </summary>
        /// <param name="buffer"> 要寫入的資料 </param>
        /// <param name="encrypt"> 是否要進行加密 </param>
        public void Write(byte[] buffer, bool encrypt = false)
        {
            byte[] t = new byte[buffer.Length];

            buffer.CopyTo(t, 0);
            this.Write(t, t.Length, encrypt);
        }

        /// <summary> 寫入一個<see cref="bool"/>值進入緩衝區，並將資料流往前1個byte </summary>
        public void WriteBool(bool b)
        {
            this.Write1u((byte)(b ? 1 : 0));
        }

        /// <summary> 寫入一個<see cref="byte"/>值進入緩衝區，並將資料流往前1個byte </summary>
        public void Write1u(byte v)
        {
            byte[] b = { v };

            this.Write(b, 1);
        }

        /// <summary> 寫入一個<see cref="ushort"/>值進入緩衝區，並將資料流往前2個byte </summary>
        public void Write2u(ushort v)
        {
            byte[] b = new byte[2];

            for (int i = 0; i < 2; ++i)
            {
                b[i] = (byte)((v >> i * 8) & 0xFF);
            }

            this.Write(b, 2);
        }

        /// <summary> 寫入一個<see cref="uint"/>值進入緩衝區，並將資料流往前4個byte </summary>
        public void Write4u(uint v)
        {
            byte[] b = new byte[4];

            for (int i = 0; i < 4; ++i)
            {
                b[i] = (byte)((v >> i * 8) & 0xFF);
            }

            this.Write(b, 4);
        }

        /// <summary> 寫入一個<see cref="ulong"/>值進入緩衝區，並將資料流往前8個byte </summary>
        public void Write8u(ulong v)
        {
            byte[] b = new byte[8];

            for (int i = 0; i < 8; ++i)
            {
                b[i] = (byte)((v >> i * 8) & 0xFF);
            }

            this.Write(b, 8);
        }

        /// <summary> 寫入一個<see cref="sbyte"/>值進入緩衝區，並將資料流往前1個byte </summary>
        public void Write1(sbyte v)
        {
            this.Write1u((byte)v);
        }

        /// <summary> 寫入一個<see cref="short"/>值進入緩衝區，並將資料流往前2個byte </summary>
        public void Write2(short v)
        {
            this.Write2u((ushort)v);
        }

        /// <summary> 寫入一個<see cref="int"/>值進入緩衝區，並將資料流往前4個byte </summary>
        /// <param name="v"> </param>
        /// <param name="compressed"> 是否要進行壓縮 </param>
        public void Write4(int v, bool compressed = false)
        {
            if (compressed)
            {
                bool compressable = -127 <= v && v <= 127;

                this.Write1(compressable ? (sbyte)v : (sbyte)-128);

                if (compressable)
                {
                    return;
                }
            }
            this.Write4u((uint)v);
        }

        /// <summary> 寫入一個<see cref="long"/>值進入緩衝區，並將資料流往前8個byte </summary>
        /// <param name="v"> </param>
        /// <param name="compressed"> 是否要進行壓縮 </param>
        public void Write8(long v, bool compressed = false)
        {
            if (compressed)
            {
                bool compressable = -127 <= v && v <= 127;

                this.Write1(compressable ? (sbyte)v : (sbyte)-128);

                if (compressable)
                {
                    return;
                }
            }

            this.Write8u((ulong)v);
        }

        /// <summary> 寫入一個<see cref="float"/>值進入緩衝區，並將資料流往前4個byte </summary>
        /// <param name="v"> </param>
        /// <param name="compressed"> 是否要進行壓縮 </param>
        public void WriteFloat(float v, bool compressed = false)
        {
            if (compressed)
            {
                bool compressable = v == 0.0f;

                this.Write1(compressable ? (sbyte)0 : (sbyte)-128);

                if (compressable)
                {
                    return;
                }
            }
            this.Write(BitConverter.GetBytes(v));
        }

        /// <summary> 寫入一個<see cref="double"/>值進入緩衝區，並將資料流往前2個byte </summary>
        public void WriteDouble(double v)
        {
            this.Write(BitConverter.GetBytes(v));
        }

        /// <summary> 將一串文字以指定編碼寫入緩衝區，並將資料流往前 </summary>
        /// <param name="str"> 要寫入的字串 </param>
        /// <param name="codepage"> 字串的編碼 </param>
        /// <param name="encrypt"> 是否需要進行加密 </param>
        public void WriteString(string str, Encoding codepage = null, bool encrypt = false)
        {
            if (codepage == null)
            {
                codepage = Encoding.ASCII;
            }

            byte[] strbuffer = codepage.GetBytes(str);
            this.Write(strbuffer, strbuffer.Length, encrypt);
        }

        /// <summary> 將指定資料流中的一串資料寫入目前的資料流中 </summary>
        /// <param name="source"> 來源資料流 </param>
        /// <param name="offset"> 來源資料的起始點 </param>
        /// <param name="size"> 讀取大小 </param>
        public void WriteDataFromStream(Stream source, long offset, int size)
        {
            const int BufferSize = 0x1000;

            source.Seek(offset, SeekOrigin.Begin);

            // write data
            int blockCount = (size / BufferSize);
            int remains = size % BufferSize;
            byte[] buffer = new byte[BufferSize];

            for (int i = 0; i < blockCount; ++i)
            {
                source.Read(buffer, 0, BufferSize);
                this.Write(buffer, BufferSize);
            }
            if (remains > 0)
            {
                source.Read(buffer, 0, remains);
                this.Write(buffer, remains);
            }
        }

        private void Init(Stream fs, WzKeyType k)
        {
            this.KeyType = k;
            this.BaseOffset = 0;
            this.BaseStream = fs;
            this.DynamicRead = false;
            this.StringPool = new SerializeStringPool(this);
        }
    }
}
