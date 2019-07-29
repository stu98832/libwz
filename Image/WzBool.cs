using System;
using libwz.IO;

namespace libwz
{
    /// <summary> </summary>
    public class WzBool : WzVariant
    {
        /// <summary> </summary>
        public const ushort True = 0xFFFF;
        /// <summary> </summary>
        public const ushort False = 0x0000;

        /// <summary> </summary>
        public ushort Value { get; set; }

        /// <summary> </summary>
        public WzBool(string name, bool value = false) :
            base(WzVariantType.Boolean, name)
        {
            this.Value = value ? True : False;
        }

        /// <summary> </summary>
        public override string ToString()
        {
            return this.Value == True ? "true" :
                   this.Value == False ? "false" : "invalid(" + this.Value + ")";
        }

        /// <summary> </summary>
        public override T GetValue<T>(T def = default(T))
        {
            ValidateValue();

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
                return (T)(object)(short)(this.Value);
            }
            else if (typeof(T) == typeof(ushort))
            {
                return (T)(object)(this.Value);
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
            else if (typeof(T) == typeof(bool))
            {
                return (T)(object)(this.Value == True);
            }

            return def;
        }

        /// <summary> </summary>
        public override bool Equals(WzVariant obj)
        {
            return this.Value == (obj as WzBool).Value;
        }

        /// <summary> </summary>
        public override WzVariant Clone()
        {
            WzBool clone = new WzBool(this.Name);
            clone.Value = this.Value;

            return clone;
        }

        internal override void Read(WzFileStream fs)
        {
            this.Value = fs.Read2u();
            ValidateValue();
        }

        internal override void Write(WzFileStream fs)
        {
            ValidateValue();
            fs.Write2u(this.Value);
        }

        private void ValidateValue()
        {
            if (this.Value != True && this.Value != False)
            {
                throw new ArgumentException();
            }
        }
    }
}
