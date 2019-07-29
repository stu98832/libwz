using System;
using libwz.IO;

namespace libwz
{
    /// <summary> </summary>
    public class WzDispatch : WzVariant
    {
        /// <summary> </summary>
        public override WzProperty Parent
        {
            get
            {
                if (this.Value != null)
                {
                    return this.Value.Parent as WzProperty;
                }

                return this.mParent;
            }
            set
            {
                if (this.Value != null)
                {
                    this.Value.Parent = value;
                }

                this.mParent = value;
            }
        }

        /// <summary> </summary>
        public WzSerialize Value { get; set; }

        /// <summary> </summary>
        public WzDispatch(string name, WzSerialize value = null) :
            base(WzVariantType.Dispatch, name)
        {
            this.Value = value;
        }

        /// <summary> </summary>
        public override string ToString()
        {
            return this.Value.ToString();
        }

        /// <summary> </summary>
        public override bool Equals(WzVariant obj)
        {
            return this.Value.Equals((obj as WzDispatch).Value);
        }

        /// <summary> </summary>
        public override T GetValue<T>(T def = default(T))
        {
            if (typeof(T) == typeof(WzSerialize))
            {
                return (T)(object)this.Value;
            }
            else if (typeof(T) == typeof(WzProperty))
            {
                return (T)(object)(this.Value as WzProperty);
            }
            else if (typeof(T) == typeof(WzCanvas))
            {
                return (T)(object)(this.Value as WzCanvas);
            }
            else if (typeof(T) == typeof(WzVector2D))
            {
                return (T)(object)(this.Value as WzVector2D);
            }
            else if (typeof(T) == typeof(WzConvex2D))
            {
                return (T)(object)(this.Value as WzConvex2D);
            }
            else if (typeof(T) == typeof(WzSound))
            {
                return (T)(object)(this.Value as WzSound);
            }
            else if (typeof(T) == typeof(WzUOL))
            {
                return (T)(object)(this.Value as WzUOL);
            }

            return def;
        }

        /// <summary> </summary>
        public override WzVariant Clone()
        {
            return new WzDispatch(this.Name, this.Value.Clone());
        }

        /// <summary> </summary>
        internal override void Read(WzFileStream fs)
        {
            int blockSize = fs.Read4();
            long off = fs.Tell();

            string classname = fs.StringPool.Read();
            WzSerialize obj = WzSerialize.FromClassName(classname, this.Name);
            obj.ImageFile = this.Parent.ImageFile;
            obj.Read(fs);

            if (fs.Tell() != (off + blockSize))
            {
                fs.Seek(off + blockSize);
#if DEBUG
                System.Console.WriteLine("沒有解析完全 ： {0}", this.GetImagePath());
#endif
            }

            this.Value = obj;
        }

        /// <summary> </summary>
        internal override void Write(WzFileStream fs)
        {
            fs.Write4u(0); // Size Reserve

            long startBlock = fs.Tell();
            fs.StringPool.Write(this.Value.ClassName, 0x73, 0x1B);
            this.Value.Write(fs);
            long endBlock = fs.Tell();

            fs.Seek(startBlock - 4);
            fs.Write4u((uint)(endBlock - startBlock));
            fs.Seek(endBlock);
        }
    }
}