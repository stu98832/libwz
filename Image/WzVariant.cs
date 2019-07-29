using System.ComponentModel;
using libwz.IO;

namespace libwz
{
    // 原始資料結構為 Union VARIANT; 含在Wtype.h檔中
    // 這邊另外實做一個

    /// <summary> wz專用的Variant類別，可以儲存多種類型的資料。此類別無法直接建立實例 </summary>
    [Browsable(true)]
    public abstract class WzVariant
    {
        /// <summary> 代表null的<see cref="WzVariant"/> </summary>
        public readonly static WzVariant Null = new WzNull("");

        /// <summary> 代表空值的<see cref="WzVariant"/> </summary>
        public readonly static WzVariant Empty = new WzEmpty("");

        /// <summary> 設定或取得儲存這個<see cref="WzVariant"/>的<see cref="WzProperty"/> </summary>
        public virtual WzProperty Parent
        {
            get { return this.mParent; }
            set { this.mParent = value; }
        }

        /// <summary> 設定或取得這個<see cref="WzVariant"/>的名子 </summary>
        public string Name { get; set; }

        /// <summary> 取得這個<see cref="WzVariant"/>的類型 </summary>
        public WzVariantType Type { get; private set; }

        /// <summary> 建立指定類別和名子<see cref="WzVariant"/>實體 </summary>
        /// <param name="type"> 這個<see cref="WzVariant"/>的資料類型 </param>
        /// <param name="name"> 這個<see cref="WzVariant"/>的名子 </param>
        public WzVariant(WzVariantType type, string name)
        {
            this.Type = type;
            this.Name = name;
            this.mParent = null;
        }

        /// <summary> 取得這個<see cref="WzVariant"/>的所在位置 </summary>
        public string GetImagePath()
        {
            if (this.Parent == null)
            {
                return this.Name;
            }

            string prop_path = this.Parent.GetImagePath();

            return (prop_path == "" ? "" : (prop_path + "/")) + this.Name;
        }

        /// <summary>以指定的類型取得這個<see cref="WzVariant"/>裡的資料。若資料不存在或類型不允許時，則會回傳預設值 </summary>
        /// <param name="def"> 預設值 </param>
        public abstract T GetValue<T>(T def = default(T));

        /// <summary> 檢查指定的<see cref="WzVariant"/>是否和目前的<see cref="WzVariant"/>相等 </summary>
        /// <param name="obj"> 要比較的<see cref="WzVariant"/>物件 </param>
        public abstract bool Equals(WzVariant obj);

        /// <summary> 產生一個<see cref="WzVariant"/>的拷貝 </summary>
        public abstract WzVariant Clone();

        /// <summary> 將這個執行個體的值轉換為它的對等字串表示。 </summary>
        public override string ToString()
        {
            return string.Format("{0}[{1}]", this.Type.ToString(), this.GetHashCode());
        }

        /// <summary> 判斷指定的 <see cref="object"/> 和目前的 <see cref="object"/> 是否相等。 </summary>
        /// <param name="obj"> <see cref="object"/>，要與目前的 <see cref="object"/> 比較。 </param>
        public override bool Equals(object obj)
        {
            if (obj is WzVariant && this.Type == (obj as WzVariant).Type)
            {
                return this.Equals((WzVariant)obj);
            }

            return base.Equals(obj);
        }

        /// <summary> 做為特定型別的雜湊函式。 </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary> </summary>
        public static bool operator ==(WzVariant left, WzVariant right)
        {
            bool result = object.Equals(left, right);
            return result || left.Equals((object)right);
        }

        /// <summary> </summary>
        public static bool operator !=(WzVariant left, WzVariant right)
        {
            bool result = object.Equals(left, right);
            return !result || !left.Equals((object)right);
        }

        internal abstract void Read(WzFileStream fs);
        internal abstract void Write(WzFileStream fs);

        /// <summary> </summary>
        protected WzProperty mParent;
    }
}
