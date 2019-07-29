using libwz.IO;

namespace libwz
{
    /// <summary> </summary>
    public class WzShort : WzVariant
    {
        /// <summary> </summary>
        public short Value { get; set; }

        /// <summary> </summary>
        public WzShort(string name, short value = 0) :
            base(WzVariantType.Short, name)
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
            if (typeof(T) == typeof(sbyte))
            {
                return (T)(object)(sbyte)(this.Value & 0xFF);
            }
            else if (typeof(T) == typeof(byte))
            {
                return (T)(object)(byte)(this.Value & 0xFF);
            }
            else if (typeof(T) == typeof(short))
            {
                return (T)(object)(this.Value);
            }
            else if (typeof(T) == typeof(ushort))
            {
                return (T)(object)(ushort)(this.Value);
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)(int)(this.Value);
            }
            else if (typeof(T) == typeof(uint))
            {
                return (T)(object)(uint)(this.Value);
            }
            else if (typeof(T) == typeof(long))
            {
                return (T)(object)(long)(this.Value);
            }
            else if (typeof(T) == typeof(ulong))
            {
                return (T)(object)(ulong)(this.Value);
            }

            return def;
        }

        /// <summary> </summary>
        public override bool Equals(WzVariant obj)
        {
            return this.Value == (obj as WzShort).Value;
        }

        /// <summary> </summary>
        public override WzVariant Clone()
        {
            return new WzShort(this.Name, this.Value);
        }

        /// <summary> </summary>
        internal override void Read(WzFileStream fs)
        {
            this.Value = fs.Read2();
        }

        /// <summary> </summary>
        internal override void Write(WzFileStream fs)
        {
            fs.Write2(this.Value);
        }
    }
}
