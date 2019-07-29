using libwz.IO;

namespace libwz
{
    /// <summary> </summary>
    public class WzEmpty : WzVariant
    {
        /// <summary> </summary>
        public WzEmpty(string name) :
            base(WzVariantType.Empty, name)
        {

        }

        /// <summary> </summary>
        public override string ToString()
        {
            return "(empty)";
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
            return new WzEmpty(this.Name);
        }

        internal override void Read(WzFileStream fs)
        {
        }

        internal override void Write(WzFileStream fs)
        {
        }
    }
}
