using System;
using System.Collections.Generic;
using libwz.IO;

namespace libwz.Text
{
    /// <summary> 用來儲存<see cref="SerializeString"/>實例的物件池 </summary>
    public class SerializeStringPool : IDisposable
    {
        /// <summary> 透過指定的<see cref="WzFileStream"/>建立<see cref="SerializeStringPool"/>實體 </summary>
        public SerializeStringPool(WzFileStream stream)
        {
            this.mStrTable = new Dictionary<uint, string>();
            this.mRefTable = new Dictionary<string, uint>();
            this.mStream = stream;
        }

        /// <summary> 清除所有的緩衝資料 </summary>
        public void Clear()
        {
            this.mStrTable.Clear();
            this.mRefTable.Clear();
        }

        /// <summary> 讀取一串序列化文字。如果池裡面已經存在同樣的字串，會從池裡面讀取 </summary>
        public string Read()
        {
            byte header = this.mStream.Read1u();

            switch (header)
            {
                case 0x00: // normal serialize string
                case 0x73: // class  serialize string
                    uint off = (uint)this.mStream.Tell(true);
                    string str = SerializeString.Read(this.mStream);
                    this.mStrTable.Add(off, str);
                    return str;
                case 0x01: // reference normal serialize string
                case 0x1B: // reference class  serialize string
                    return this.mStrTable[this.mStream.Read4u()];
                default:

                    throw new ArgumentException("invalid header : " + header);
            }
        }

        /// <summary> 寫入一串序列化文字。如果池裡已經存在相同的字串，則會以快取的形式寫入  </summary>
        /// <param name="str"> </param>
        /// <param name="readsign"> 表示需要從資料流讀取的標記 </param>
        /// <param name="refsign"> 表示需要從池裡讀取的標記 </param>
        public void Write(string str, byte readsign, byte refsign)
        {
            if (str == null)
            {
                str = "";
            }

            bool cacheable = this.mRefTable.ContainsKey(str);

            this.mStream.Write1u(cacheable ? refsign : readsign);

            if (cacheable)
            {
                this.mStream.Write4u(this.mRefTable[str]);
            }
            else
            {
                this.mRefTable.Add(str, (uint)(this.mStream.Tell(true)));
                SerializeString.Write(this.mStream, str);
            }
        }

        /// <summary> 釋放<see cref="SerializeStringPool"/>所使用的資源 </summary>
        public void Dispose()
        {
            this.Clear();
            this.mStream = null;
        }

        // For WZDirectory
        internal string DirectoryRead(out WzArchiveItemType type, WzArchiveItemType reftype)
        {
            uint off = (uint)this.mStream.Tell(true);
            type = (WzArchiveItemType)this.mStream.Read1u();
            if (type == reftype)
            {
                uint cacheoff = this.mStream.Read4u();
                long org = this.mStream.Tell();
                this.mStream.Seek(cacheoff, true);
                type = (WzArchiveItemType)this.mStream.Read1u();
                this.mStream.Seek(org);
                return this.mStrTable[cacheoff];
            }
            else
            {
                string str = SerializeString.Read(this.mStream);
                this.mStrTable.Add(off, str);
                return str;
            }
        }
        internal void DirectoryWrite(string str, WzArchiveItemType type, WzArchiveItemType cachetype)
        {
            bool cacheable = false;
            bool hascache = this.mRefTable.ContainsKey(str);
            long org = this.mStream.Tell(true);

            if (hascache)
            {
                this.mStream.Seek(this.mRefTable[str], true);
                cacheable = this.mStream.Read1u() == (byte)type;
                this.mStream.Seek(org, true);
            }

            this.mStream.Write1u((byte)(cacheable ? cachetype : type));

            if (cacheable)
                this.mStream.Write4u(this.mRefTable[str]);
            else
            {
                this.mRefTable.Add(str, (uint)org);
                SerializeString.Write(this.mStream, str);
            }
        }

        private Dictionary<uint, string> mStrTable;
        private Dictionary<string, uint> mRefTable;
        private WzFileStream mStream;
    }
}
