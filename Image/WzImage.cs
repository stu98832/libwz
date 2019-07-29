using System;
using System.ComponentModel;
using libwz.IO;
using libwz.AES;

namespace libwz
{
    /// <summary> wz映像檔 </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class WzImage : IDisposable
    {
        /// <summary> 取得目前<see cref="WzImage"/>的序列化資料 </summary>
        public WzSerialize Data { get; private set; }

        /// <summary> 建立<see cref="WzImage"/>實體 </summary>
        public WzImage(WzSerialize data = null)
        {
            if (data != null)
            {
                data.ImageFile = this;
            }
            this.Data = data;
        }

        /// <summary> 從指定的資料流中讀取<see cref="WzImage"/>的資料 </summary>
        /// <param name="stream"> 來源資料流 </param>
        public void Read(WzFileStream stream)
        {
            stream.Seek(0, true);
            stream.StringPool.Clear();
            this.Data = WzSerialize.FromClassName(stream.StringPool.Read());
            this.Data.ImageFile = this;
            this.Data.Read(stream);
        }

        /// <summary> 將<see cref="WzImage"/>的資料寫入指定的資料流中 </summary>
        /// <param name="stream"> 目的資料流 </param>
        public void Write(WzFileStream stream)
        {
            stream.StringPool.Clear();
            stream.StringPool.Write(this.Data.ClassName, 0x73, 0x1B);
            this.Data.Write(stream);
        }

        /// <summary> 產生一個<see cref="WzImage"/>的拷貝 </summary>
        public WzImage Clone()
        {
            WzImage img = new WzImage();

            img.Data = this.Data.Clone();
            return img;
        }

        /// <summary> 釋放<see cref="WzImage"/>所使用的資源 </summary>
        public void Dispose()
        {
            this.Data.Dispose();
            this.Data = null;

            GC.SuppressFinalize(this);

            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        /// <summary> </summary>
        public WzVariant this[string path]
        {
            get
            {
                WzProperty obj = this.Data as WzProperty;

                return obj == null ? WzVariant.Null : obj[path];
            }
        }

        /// <summary> 使用指定的加密金鑰，將加密過的<see cref="WzImage"/>資料寫入<see cref="WzFile"/>中 </summary>
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

        /// <summary> 從<see cref="WzFile"/>中讀取資料並建立<see cref="WzImage"/>實體 </summary>
        /// <param name="file"> </param>
        /// <param name="dynamic"> 是否使用動態讀取。使用動態讀取可以減少圖片和聲音佔用大量記憶體空間，但讀取速度會稍微慢一點 </param>
        public static WzImage FromWzFile(WzFile file, bool dynamic = false)
        {
            if (file == null)
                return null;

            WzImage img = new WzImage();
            WzFileStream fs = new WzFileStream(file.Stream, file.KeyType);

            fs.BaseOffset = file.Offset;
            fs.Seek(file.Offset);
            fs.DynamicRead = dynamic;
            img.Read(fs);

            if (!dynamic)
            {
                fs.Dispose(false);
            }
            return img;
        }
    }
}
