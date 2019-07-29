using System;
using System.Collections.Generic;
using System.Text;
using libwz.IO;

namespace libwz
{
    /// <summary> wz二維圖形物件。此類別無法直接建立實例 </summary>
    public abstract class WzShape2D : WzSerialize
    {
        /// <summary> 取得目前<see cref="WzShape2D"/>的Class名稱 </summary>
        public override string ClassName { get { return "Shape2D"; } }

        /// <summary> 建立<see cref="WzShape2D"/>實體 </summary>
        /// <param name="name"> <see cref="WzShape2D"/>的名稱 </param>
        public WzShape2D(string name) : base(name) { }

        /// <summary> 產生一個<see cref="WzShape2D"/>的拷貝 </summary>
        public abstract override WzSerialize Clone();

        /// <summary> 釋放<see cref="WzShape2D"/>所使用的資源 </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        internal abstract override bool Read(WzFileStream stream);
        internal abstract override bool Write(WzFileStream stream);
    }
}
