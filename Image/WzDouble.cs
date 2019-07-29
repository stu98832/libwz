using libwz.IO;

namespace libwz
{
    /// <summary> </summary>
    public class WzDouble : WzVariant
    {
        /// <summary> </summary>
        public double Value { get; set; }

        /// <summary> </summary>
        public WzDouble(string name, double value = 0) :
            base(WzVariantType.Double, name)
        {
            this.Value = value;
        }

        /// <summary> </summary>
        public override string ToString()
        {
            return this.Value.ToString();
        }

        /// <summary> </summary>
        public override T GetValue<T>(T def = default(T))
        {
            if (typeof(T) == typeof(double))
            {
                return (T)(object)(this.Value);
            }

            return def;
        }

        /// <summary> </summary>
        public override bool Equals(WzVariant obj)
        {
            return this.Value == (obj as WzDouble).Value;
        }

        /// <summary> </summary>
        public override WzVariant Clone()
        {
            return new WzDouble(this.Name, this.Value);
        }

        internal override void Read(WzFileStream fs)
        {
            this.Value = fs.ReadDouble();
        }

        internal override void Write(WzFileStream fs)
        {
            fs.WriteDouble(this.Value);
        }
    }
}
