using System.Drawing;
using System.ComponentModel;
using System.IO;
using libwz.IO;
using libwz.Tools;

namespace libwz
{
    /// <summary> wz圖像物件 </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class WzCanvas : WzSerialize
    {
        /// <summary> 取得目前<see cref="WzCanvas"/>的Class名稱 </summary>
        public override string ClassName { get { return "Canvas"; } }

        /// <summary> </summary>
        public byte Unknow1_Byte { get; private set; }

        /// <summary> 取得圖像的屬性 </summary>
        public WzProperty CanvasProperty { get; private set; }

        /// <summary> 取得圖像的寬(像素) </summary>
        public int Width { get; private set; }

        /// <summary> 取得圖像的高(像素) </summary>
        public int Height { get; private set; }

        /// <summary> 取得圖像的像素格式 </summary>
        public WzCanvasFormat Format { get; private set; }

        /// <summary> 取得圖像的縮放程度。這個數值將會影響儲存時的資料大小 </summary>
        public byte Scale { get; set; }

        /// <summary> </summary>
        public int Unknow2_Int { get; private set; }

        /// <summary> 取得圖像資料所佔用的大小 </summary>
        public int DataSize { get; private set; }

        /// <summary> 取得圖像資料。若是使用動態讀取，則在每次取得圖像資料前都會從資料流把圖片資料讀取出來 </summary>
        [Browsable(false)]
        public byte[] CanvasData
        {
            get
            {
                if (this.mStream != null)
                {
                    this.mStream.Seek(this.mCanvasOffset);
                    return this.ResolveData(this.mStream);
                }
                return this.mCanvasData;
            }
            internal set
            {
                this.mCanvasData = value;
                this.mStream = null;
            }
        }

        /// <summary> 建立使用指定像素格式的<see cref="WzCanvas"/>實體 </summary>
        /// <param name="name"> <see cref="WzCanvas"/>的名稱 </param>
        /// <param name="format"> <see cref="WzCanvas"/>的像素格式 </param>
        public WzCanvas(string name, WzCanvasFormat format = WzCanvasFormat.B8G8R8A8) : base(name)
        {
            this.CanvasProperty = new WzProperty(null);
            this.Unknow1_Byte = 0;
            this.Width = 0;
            this.Height = 0;
            this.Format = format;
            this.Scale = 0;
            this.Unknow2_Int = 0;
            this.DataSize = 0;
            this.mCanvasOffset = 0;
            this.mCanvasData = null;
            this.mStream = null;
        }

        /// <summary> </summary>
        public WzVariant this[string path]
        {
            get { return this.CanvasProperty[path]; }
        }

        /// <summary> 取得這個<see cref="WzCanvas"/>所儲存的圖像資料 </summary>
        public Bitmap GetBitmap()
        {
            return CanvasTools.Decompress(this.Format, this.Scale, this.Width, this.Height, this.CanvasData); ;
        }

        /// <summary> 設定這個<see cref="WzCanvas"/>的圖像資料 </summary>
        public void SetBitmap(Bitmap bmp)
        {
            this.Width = bmp.Width;
            this.Height = bmp.Height;
            byte[] data = CanvasTools.Compress(this.Format, this.Scale, this.Width, this.Height, bmp);
            this.CanvasData = data;
            this.DataSize = data.Length;
        }

        /// <summary> 產生一個<see cref="WzCanvas"/>的拷貝 </summary>
        public override WzSerialize Clone()
        {
            WzCanvas canvas = new WzCanvas(this.Name);

            canvas.Unknow1_Byte = this.Unknow1_Byte;
            canvas.Width = this.Width;
            canvas.Height = this.Height;
            canvas.Format = this.Format;
            canvas.Scale = this.Scale;
            canvas.Unknow2_Int = this.Unknow2_Int;
            canvas.DataSize = this.DataSize;

            byte[] datas = this.CanvasData;
            canvas.mCanvasData = new byte[datas.Length];
            datas.CopyTo(canvas.mCanvasData, 0);

            return canvas;
        }

        /// <summary> 釋放<see cref="WzCanvas"/>所使用的資源 </summary>
        public override void Dispose()
        {
            if (this.CanvasProperty != null)
                this.CanvasProperty.Dispose();
            this.mCanvasData = null;
            if (this.mStream != null)
                this.mStream.Dispose(false);
            base.Dispose();
        }

        internal override bool Read(WzFileStream stream)
        {
            this.Unknow1_Byte = stream.Read1u();
            bool hasProperty = stream.ReadBool();
            if (hasProperty)
                this.CanvasProperty.Read(stream);
            this.Width = stream.Read4(true);
            this.Height = stream.Read4(true);
            this.Format = (WzCanvasFormat)stream.Read4(true);
            this.Scale = stream.Read1u();
            this.Unknow2_Int = stream.Read4();
            this.DataSize = stream.Read4();
            this.mCanvasOffset = (uint)stream.Tell();

            if (stream.DynamicRead)
            {
                stream.Skip(this.DataSize);
                this.mStream = stream;
            }
            else
                this.CanvasData = this.ResolveData(stream);

            return true;
        }
        internal override bool Write(WzFileStream stream)
        {
            bool hasprop = this.CanvasProperty.Count > 0;

            stream.Write1u(this.Unknow1_Byte);
            stream.WriteBool(hasprop);
            if (hasprop)
                this.CanvasProperty.Write(stream);
            stream.Write4(this.Width, true);
            stream.Write4(this.Height, true);
            stream.Write4((int)this.Format, true);
            stream.Write1u(this.Scale);
            stream.Write4(this.Unknow2_Int);

            stream.Write4(this.mCanvasData.Length);
            stream.Write(this.mCanvasData);

            return true;
        }

        private uint mCanvasOffset;
        private byte[] mCanvasData;
        private WzFileStream mStream;

        private byte[] ResolveData(WzFileStream zs)
        {
            byte unk = zs.Read1u();
            byte cmf = zs.Read1u();
            byte flg = zs.Read1u();
            zs.Skip(-3);

            if (CanvasZlibTool.CheckDeflate(unk, cmf, flg))
            {
                return zs.Read(this.DataSize);
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.WriteByte(zs.Read1u());
                    for (int i = 1; i < this.DataSize;)
                    {
                        int blocksize = zs.Read4();
                        ms.Write(zs.Read(blocksize, true), 0, blocksize);
                        i += blocksize + 4;
                    }
                    return ms.ToArray();
                }
            }
        }
    }
}
