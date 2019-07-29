using System.ComponentModel;
using libwz.IO;

namespace libwz
{
    /// <summary> wz二維向量物件 </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class WzVector2D : WzShape2D
    {
        /// <summary> 取得目前<see cref="WzVector2D"/>的Class名稱 </summary>
        public override string ClassName { get { return "Shape2D#Vector2D"; } }

        /// <summary> 取得或設定這個<see cref="WzVector2D"/>的X座標 </summary>
        public int X { get; set; }

        /// <summary> 取得或設定這個<see cref="WzVector2D"/>的Y座標 </summary>
        public int Y { get; set; }

        /// <summary> 建立<see cref="WzVector2D"/>實體 </summary>
        /// <param name="name"> <see cref="WzVector2D"/>的名稱 </param>
        public WzVector2D(string name) : this(name, 0, 0) { }

        /// <summary> 建立<see cref="WzVector2D"/>實體 </summary>
        /// <param name="name"> <see cref="WzVector2D"/>的名稱 </param>
        /// <param name="x"> x座標 </param>
        /// <param name="y"> y座標 </param>
        public WzVector2D(string name, int x, int y) :
            base(name)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary> 產生一個<see cref="WzVector2D"/>的拷貝 </summary>
        public override WzSerialize Clone()
        {
            return new WzVector2D(this.Name, this.X, this.Y);
        }

        /// <summary> 釋放<see cref="WzVector2D"/>所使用的資源 </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        internal override bool Read(WzFileStream stream)
        {
            this.X = stream.Read4(true);
            this.Y = stream.Read4(true);
            return true;
        }
        internal override bool Write(WzFileStream stream)
        {
            stream.Write4(this.X, true);
            stream.Write4(this.Y, true);
            return true;
        }
    }
}
