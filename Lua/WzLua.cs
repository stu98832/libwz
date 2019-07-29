using System;
using System.Text;
using System.ComponentModel;
using libwz.IO;
using libwz.AES;

namespace libwz
{
    /// <summary> wzLua腳本 </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class WzLua : IDisposable
    {
        /// <summary> 腳本名稱 </summary>
        public string Name { get; set; }

        /// <summary> 腳本內容 </summary>
        public string Script { get; set; }

        /// <summary> 從指定資料流讀取腳本資料 </summary>
        public bool Read(WzFileStream stream)
        {
            stream.Seek(0, true);
            byte flag = stream.Read1u();
            switch (flag)
            {
                case 1:
                    int len = stream.Read4(true);
                    this.Script = stream.ReadString(len, Encoding.UTF8, true);
                    break;
                default:
                    throw new NotSupportedException("Not supported flag : " + flag);
            }
            return true;
        }

        /// <summary> 將腳本資料寫入指定資料流 </summary>
        public bool Write(WzFileStream stream)
        {
            byte[] data = Encoding.UTF8.GetBytes(this.Script);

            stream.Write1u(1);
            stream.Write4(data.Length, true);
            stream.Write(data, data.Length, true);
            return true;
        }

        /// <summary> 釋放<see cref="WzLua"/>所使用的資源 </summary>
        public void Dispose()
        {
            this.Name = null;
            this.Script = null;

            GC.SuppressFinalize(this);

            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        /// <summary> 使用指定的加密金鑰，將加密過的<see cref="WzLua"/>資料寫入<see cref="WzFile"/>中 </summary>
        /// <param name="name"> <see cref="WzFile"/>的名子 </param>
        /// <param name="key"> 加密金鑰 </param>
        public WzFile ToWzFile(string name, WzKeyType key = WzKeyType.None)
        {
            WzFile file = new WzFile(name, key);

            WzFileStream stream = new WzFileStream(file.Stream, file.KeyType);

            this.Write(stream);
            file.Size = (int)stream.Length;
            stream.Dispose(false);

            return file;
        }

        /// <summary> 從<see cref="WzFile"/>中讀取資料並建立<see cref="WzLua"/>實體 </summary>
        public static WzLua FromWzFile(WzFile file)
        {
            WzLua lua = new WzLua();

            WzFileStream fs = new WzFileStream(file.Stream, WzKeyType.K);
            fs.BaseOffset = file.Offset;
            lua.Name = file.Name;
            lua.Read(fs);

            return lua;
        }
    }
}
