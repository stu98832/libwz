using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using libwz.IO;

namespace libwz
{
    /// <summary> wz屬性物件 </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public partial class WzProperty : WzSerialize, ICollection<WzVariant>
    {
        /// <summary> 取得目前<see cref="WzProperty"/>的Class名稱 </summary>
        public override string ClassName { get { return "Property"; } }

        /// <summary>  </summary>
        public ushort Unknow1_UShort { get; set; }

        /// <summary> 建立<see cref="WzProperty"/>實體 </summary>
        /// <param name="name"> <see cref="WzProperty"/>的名稱 </param>
        public WzProperty(string name) : base(name)
        {
            this.Unknow1_UShort = 0;
            this.mList = new List<WzVariant>();
        }

        /// <summary> </summary>
        public WzVariant this[string path]
        {
            get
            {
                WzProperty sub = this;
                string[] names = path.Replace('\\', '/').Split('/');
                int i;

                for (i = 0; i < names.Length - 1 && sub != null; ++i)
                {
                    sub = sub[names[i]].GetValue<WzProperty>();
                }

                if (sub != null)
                {
                    foreach (WzVariant v in sub)
                    {
                        if (v.Name == names[i])
                        {
                            return v;
                        }
                    }
                }

                return WzVariant.Null;
            }
        }

        /// <summary> 取得這個<see cref="WzProperty"/>目前有幾個屬性 </summary>
        public int Count { get { return this.mList.Count; } }

        /// <summary> 表示這個<see cref="WzProperty"/>是否為唯讀 </summary>
        public bool IsReadOnly { get { return false; } }

        /// <summary> </summary>
        public WzVariant this[int index]
        {
            get { return this.mList[index]; }
            set { this.mList[index] = value; }
        }

        /// <summary> 加入指定的<see cref="WzVariant"/>物件 </summary>
        public void Add(WzVariant variant)
        {
            variant.Parent = this;
            this.mList.Add(variant);
        }

        /// <summary> 加入一系列<see cref="WzVariant"/>物件 </summary>
        public void AddRange(IEnumerable item)
        {
            foreach (WzVariant v in item)
            {
                this.Add(v);
            }
        }

        /// <summary> 清空這個<see cref="WzProperty"/>的所有屬性 </summary>
        public void Clear()
        {
            this.mList.Clear();
        }

        /// <summary> 確認指定的<see cref="WzVariant"/>物件是否存在於其中 </summary>
        public bool Contains(WzVariant variant)
        {
            return this.mList.Contains(variant);
        }

        /// <summary> 將<see cref="WzProperty"/>內的所有<see cref="WzVariant"/>複製到相容的<see cref="WzVariant"/>一維陣列中 </summary>
        public void CopyTo(WzVariant[] array, int arrayIndex)
        {
            this.mList.CopyTo(array, arrayIndex);
        }

        /// <summary> 傳回可以逐一查看的舉列值 </summary>
        public IEnumerator<WzVariant> GetEnumerator()
        {
            return this.mList.GetEnumerator();
        }

        /// <summary> 刪除指定的<see cref="WzVariant"/>物件 </summary>
        public bool Remove(WzVariant variant)
        {
            return this.mList.Remove(variant);
        }

        /// <summary> 傳回可以逐一查看的舉列值 </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.mList.GetEnumerator();
        }

        /// <summary> 產生一個<see cref="WzProperty"/>的拷貝 </summary>
        public override WzSerialize Clone()
        {
            WzProperty prop = new WzProperty(this.Name);

            prop.Unknow1_UShort = this.Unknow1_UShort;

            foreach (WzVariant v in this.mList)
            {
                prop.Add(v.Clone());
            }

            return prop;
        }

        /// <summary> 釋放<see cref="WzProperty"/>所使用的資源 </summary>
        public override void Dispose()
        {
            foreach (WzVariant v in this.mList)
            {
                if (v.Type == WzVariantType.Dispatch)
                {
                    v.GetValue<WzSerialize>().Dispose();
                }
            }

            this.mList.Clear();
            this.mList = null;
            base.Dispose();
        }

        internal override bool Read(WzFileStream stream)
        {
            this.mList.Clear();

            this.Unknow1_UShort = stream.Read2u(); //0
            int count = stream.Read4(true);

            for (int i = 0; i < count; ++i)
                this.Add(this.ReadVariant(stream));

            return true;
        }
        internal override bool Write(WzFileStream stream)
        {
            stream.Write2u(0);
            stream.Write4(this.mList.Count, true);

            for (int i = 0; i < this.mList.Count; ++i)
                this.WriteVariant(this.mList[i], stream);

            return true;
        }

        private List<WzVariant> mList;
        private WzVariant ReadVariant(WzFileStream stream)
        {
            WzVariant variant = null;

            string name = stream.StringPool.Read();
            WzVariantType type = (WzVariantType)stream.Read1u();

            switch (type)
            {
                case WzVariantType.Empty:
                    variant = new WzEmpty(name);
                    break;
                case WzVariantType.Null:
                    variant = new WzNull(name);
                    break;
                case WzVariantType.Short:
                    variant = new WzShort(name);
                    break;
                case WzVariantType.Int:
                    variant = new WzInt(name);
                    break;
                case WzVariantType.Float:
                    variant = new WzFloat(name);
                    break;
                case WzVariantType.Double:
                    variant = new WzDouble(name);
                    break;
                case WzVariantType.String:
                    variant = new WzString(name);
                    break;
                case WzVariantType.Boolean: // (0xFFFF = 1, 0x0000 = 0)
                    variant = new WzBool(name);
                    break;
                case WzVariantType.UInt:
                    variant = new WzUInt(name);
                    break;
                case WzVariantType.Long:
                    variant = new WzLong(name);
                    break;
                case WzVariantType.Dispatch:
                    variant = new WzDispatch(name);
                    break;
            }

            if (variant != null)
            {
                variant.Parent = this;
                variant.Read(stream);
            }

            return variant;
        }
        private void WriteVariant(WzVariant variant, WzFileStream stream)
        {
            stream.StringPool.Write(variant.Name, 0, 1);
            stream.Write1u((byte)variant.Type);
            variant.Write(stream);
        }
    }
}
