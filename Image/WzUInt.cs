using libwz.IO;

namespace libwz
{
    /// <summary> </summary>
    public class WzUInt : WzVariant
    {
        /// <summary> </summary>
        public uint Value { get; set; }

        /// <summary> </summary>
        public WzUInt(string name, uint value = 0) :
            base(WzVariantType.UInt, name)
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
                return (T)(object)(short)(this.Value & 0xFFFF);
            }
            else if (typeof(T) == typeof(ushort))
            {
                return (T)(object)(ushort)(this.Value & 0xFFFF);
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)(int)(this.Value);
            }
            else if (typeof(T) == typeof(uint))
            {
                return (T)(object)(this.Value);
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
            return this.Value == (obj as WzUInt).Value;
        }

        /// <summary> </summary>
        public override WzVariant Clone()
        {
            return new WzUInt(this.Name, this.Value);
        }

        internal override void Read(WzFileStream fs)
        {
            this.Value = (uint)fs.Read4(true);
        }

        internal override void Write(WzFileStream fs)
        {
            fs.Write4((int)this.Value, true);
        }
    }
}
