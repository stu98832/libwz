using System;
using System.ComponentModel;
using libwz.IO;

namespace libwz
{
    /// <summary> wz序列化物件。此類別無法直接建立實例 </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public abstract class WzSerialize : IDisposable
    {
        /// <summary> 表示儲存目前<see cref="WzSerialize"/>物件的<see cref="WzImage"/>物件 </summary>
        public WzImage ImageFile { get; internal set; }

        /// <summary> 設定或取得目前<see cref="WzSerialize"/>的名子 </summary>
        public string Name { get; set; }

        /// <summary> 設定或取得儲存目前<see cref="WzSerialize"/>的<see cref="WzSerialize"/>物件 </summary>
        public WzSerialize Parent { get; set; }

        /// <summary> 取得目前<see cref="WzSerialize"/>的Class名稱 </summary>
        public abstract string ClassName { get; }

        /// <summary> 建立一個<see cref="WzSerialize"/>實體 </summary>
        /// <param name="name"> 這個<see cref="WzSerialize"/>物件的名子 </param>
        public WzSerialize(string name)
        {
            this.Name = name;
            this.Parent = null;
        }

        /// <summary> 將<see cref="WzSerialize"/>物件包裝成<see cref="WzVariant"/>的形式 </summary>
        public WzVariant ToVariant()
        {
            return new WzDispatch(this.Name, this);
        }

        /// <summary> 由指定的class名稱來建立<see cref="WzSerialize"/>實體 </summary>
        /// <param name="classname"> class名稱 </param>
        /// <param name="name"> 這個<see cref="WzSerialize"/>物件的名稱 </param>
        public static WzSerialize FromClassName(string classname, string name = null)
        {
            switch (classname)
            {
                case "Property":
                    return new WzProperty(name);
                case "Canvas":
                    return new WzCanvas(name);
                case "Shape2D#Vector2D":
                    return new WzVector2D(name);
                case "Shape2D#Convex2D":
                    return new WzConvex2D(name);
                case "UOL":
                    return new WzUOL(name);
                case "Sound_DX8":
                    return new WzSound(name);
                default:
                    throw new NotSupportedException("Not supported class. \nName:" + classname);
            }
        }

        /// <summary> 取得這個<see cref="WzSerialize"/>的所在位置 </summary>
        public string GetImagePath()
        {
            WzSerialize obj = this.Parent;
            string pathstring = "";

            if (obj != null)
            {
                pathstring += this.Name;
                while (obj.Parent != null)
                {
                    pathstring = obj.Name + "/" + pathstring;
                    obj = obj.Parent;
                }
            }
            return pathstring;
        }

        /// <summary> 產生一個<see cref="WzSerialize"/>的拷貝 </summary>
        public abstract WzSerialize Clone();

        /// <summary> 釋放<see cref="WzSerialize"/>所使用的資源 </summary>
        public virtual void Dispose()
        {
            this.Name = null;
            this.Parent = null;
            GC.SuppressFinalize(this);
        }

        internal abstract bool Read(WzFileStream stream);
        internal abstract bool Write(WzFileStream stream);
    }
}
