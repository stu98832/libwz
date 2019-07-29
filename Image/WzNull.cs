using libwz.IO;

namespace libwz
{
    /// <summary> </summary>
    public class WzNull : WzVariant
    {
        /// <summary> </summary>
        public WzNull(string name) :
            base(WzVariantType.Null, name)
        {

        }

        /// <summary> </summary>
        public override string ToString()
        {
            return "(null)";
        }

        /// <summary> </summary>
        public override bool Equals(WzVariant obj)
        {
            return true;
        }

        /// <summary> </summary>
        public override T GetValue<T>(T def = default(T))
        {
            return def;
        }

        /// <summary> </summary>
        public override WzVariant Clone()
        {
            return new WzNull(this.Name);
        }

        internal override void Read(WzFileStream fs)
        {
        }

        internal override void Write(WzFileStream fs)
        {
        }
    }
}
