namespace libwz
{
    // Surface Format?
    /// <summary> <see cref="WzCanvas"/>所儲存的圖像格式 </summary>
    public enum WzCanvasFormat : int
    {
        /// <summary> 每個像素中的BGRA各佔4bytes，共16bytes </summary>
        B4G4R4A4 = 0x001,
        /// <summary> 每個像素中的BGRA各佔8bytes，共32bytes</summary>
        B8G8R8A8 = 0x002,
        /// <summary> 每個像素中的BGR各佔5bytes、6bytes、5bytes，共16bytes </summary>
        B5G6R5 = 0x201,
        /// <summary> 使用DXT3格式壓縮的圖像 </summary>
        DDS_DXT3 = 0x402,
        /// <summary> 使用DXT5格式壓縮的圖像 </summary>
        DDS_DXT5 = 0x802,
    }
}
