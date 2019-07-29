using libwz.IO;
using System.ComponentModel;

namespace libwz
{
    /// <summary> wz全局物件定位器(Uniform Object Locater)，用於表示某個位於wz文件中的<see cref="WzVariant"/>的位置 </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class WzUOL : WzSerialize
    {
        /// <summary> 取得目前<see cref="WzUOL"/>的Class名稱 </summary>
        public override string ClassName { get { return "UOL"; } }

        /// <summary> </summary>
        public byte Unknow1_Byte { get; private set; }

        /// <summary> 指向<see cref="WzVariant"/>的路徑 </summary>
        public string Path { get; set; }

        /// <summary> 建立<see cref="WzUOL"/>實體 </summary>
        /// <param name="name"> <see cref="WzUOL"/>的名稱 </param>
        /// <param name="path"> <see cref="WzVariant"/>的位置 </param>
        public WzUOL(string name, string path = null) :
            base(name)
        {
            this.Unknow1_Byte = 0;
            this.Path = path;
        }

        /// <summary> 取得這個<see cref="WzUOL"/>所指向的<see cref="WzVariant"/> </summary>
        public WzVariant GetVariant()
        {
            string[] names = this.Path.Split('/');
            WzProperty parent = this.Parent as WzProperty;
            int i;

            for (i = 0; i < names.Length - 1; ++i)
            {
                if (parent == null)
                {
                    break;
                }
                if (names[i] == "..")
                {
                    parent = parent.Parent as WzProperty;
                }
                else
                {
                    parent = parent[names[i]].GetValue<WzProperty>();
                }
            }

            return parent == null ? WzVariant.Null : parent[names[i]];
        }

        /// <summary> 將這個<see cref="WzUOL"/>指向特定的<see cref="WzVariant"/>。若該<see cref="WzVariant"/>物件不在同一個映像檔中，則會連結失敗 </summary>
        /// <param name="variant"> 要指向的<see cref="WzVariant"/>物件 </param>
        public void LinkVariant(WzVariant variant)
        {
            string path = "";
            WzSerialize myparent = this.Parent;
            while (myparent != null)
            {
                string path2 = path;
                WzSerialize vparent = variant.Parent;
                while (vparent != null)
                {
                    if (myparent == vparent)
                    {
                        this.Path = path2 + variant.Name;
                        return;
                    }

                    path2 = vparent.Name + "/" + path2;
                    vparent = vparent.Parent;
                }
                path = "../" + path;
                myparent = myparent.Parent;
            }
            this.Path = "";
            //throw new Exception("Can't find variant '" + v.Name + "' in image file");
        }

        /// <summary> 產生一個<see cref="WzUOL"/>的拷貝 </summary>
        public override WzSerialize Clone()
        {
            WzUOL uol = new WzUOL(this.Path);

            uol.Unknow1_Byte = this.Unknow1_Byte;

            return uol;
        }

        /// <summary> 釋放<see cref="WzUOL"/>所使用的資源 </summary>
        public override void Dispose()
        {
            this.Path = null;
            base.Dispose();
        }

        internal override bool Read(WzFileStream stream)
        {
            this.Unknow1_Byte = stream.Read1u();
            this.Path = stream.StringPool.Read();
            return true;
        }
        internal override bool Write(WzFileStream stream)
        {
            stream.Write1u(this.Unknow1_Byte);
            stream.StringPool.Write(this.Path, 0, 1);
            return true;
        }

    }
}
