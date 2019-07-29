using libwz.IO;

namespace libwz
{
    /// <summary> </summary>
    public class WzString : WzVariant
    {
        /// <summary> </summary>
        public string Value { get; set; }

        /// <summary> </summary>
        public WzString(string name, string value = "") :
            base(WzVariantType.String, name)
        {
            this.Value = value;
        }

        /// <summary> </summary>
        public override string ToString()
        {
            return this.Value;
        }

        /// <summary> </summary>
        public override bool Equals(WzVariant obj)
        {
            return this.ToString() == obj.ToString();
        }

        /// <summary> </summary>
        public override T GetValue<T>(T def = default(T))
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)this.Value;
            }

            return def;
        }

        /// <summary> </summary>
        public override WzVariant Clone()
        {
            return new WzString(this.Name, this.Value);
        }

        internal override void Read(WzFileStream fs)
        {
            this.Value = fs.StringPool.Read();
        }

        internal override void Write(WzFileStream fs)
        {
            fs.StringPool.Write(this.Value, 0, 1);
        }
    }
}
