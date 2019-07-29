using System.ComponentModel;
using System.Collections.Generic;
using libwz.IO;

namespace libwz
{
    /// <summary> wz二維凸形物件 </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class WzConvex2D : WzShape2D
    {
        /// <summary> 取得目前<see cref="WzConvex2D"/>的Class名稱 </summary>
        public override string ClassName { get { return "Shape2D#Convex2D"; } }

        /// <summary> 取得構成這個<see cref="WzConvex2D"/>的所有的點 </summary>
        public List<WzVector2D> Vertices { get; internal set; }

        /// <summary> 建立<see cref="WzConvex2D"/>實體 </summary>
        /// <param name="name"> <see cref="WzConvex2D"/>的名稱 </param>
        public WzConvex2D(string name) : base(name)
        {
            this.Vertices = new List<WzVector2D>();
        }

        /// <summary> 產生一個<see cref="WzConvex2D"/>的拷貝 </summary>
        public override WzSerialize Clone()
        {
            WzConvex2D convex = new WzConvex2D(this.Name);

            foreach (WzVector2D vec in this.Vertices)
            {
                convex.Vertices.Add(vec.Clone() as WzVector2D);
            }

            return convex;
        }

        /// <summary> 釋放<see cref="WzConvex2D"/>所使用的資源 </summary>
        public override void Dispose()
        {
            foreach (WzSerialize obj in this.Vertices)
            {
                obj.Dispose();
            }

            base.Dispose();
        }

        internal override bool Read(WzFileStream stream)
        {
            int nSize = stream.Read4(true);

            for (int i = 0; i < nSize; ++i)
            {
                WzVector2D vec = WzSerialize.FromClassName(stream.StringPool.Read()) as WzVector2D;
                vec.Read(stream);
                this.Vertices.Add(vec);
            }
            return true;
        }
        internal override bool Write(WzFileStream stream)
        {
            int nSize = this.Vertices.Count;

            stream.Write4(nSize, true);
            for (int i = 0; i < nSize; ++i)
            {
                stream.StringPool.Write(this.Vertices[i].ClassName, 0x73, 0x1B);
                this.Vertices[i].Write(stream);
            }
            return true;
        }
    }
}
