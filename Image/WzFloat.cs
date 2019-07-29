using libwz.IO;

namespace libwz
{
    /// <summary> </summary>
    public class WzFloat : WzVariant
    {
        /// <summary> </summary>
        public float Value { get; set; }

        /// <summary> </summary>
        public WzFloat(string name, float value = 0) :
            base(WzVariantType.Float, name)
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
            if (typeof(T) == typeof(float))
            {
                return (T)(object)(this.Value);
            }

            return def;
        }

        /// <summary> </summary>
        public override bool Equals(WzVariant obj)
        {
            return this.Value == (obj as WzFloat).Value;
        }

        /// <summary> </summary>
        public override WzVariant Clone()
        {
            return new WzFloat(this.Name, this.Value);
        }

        internal override void Read(WzFileStream fs)
        {
            this.Value = fs.ReadFloat(true);
        }

        internal override void Write(WzFileStream fs)
        {
            fs.WriteFloat(this.Value, true);
        }
    }
}
