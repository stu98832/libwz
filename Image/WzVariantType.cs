namespace libwz
{
    /// <summary> wz專用的Variant類型。對應Wtype.h標頭檔中的VARIANT TYPE(VT)部份</summary>
    public enum WzVariantType : byte
    {
        /// <summary> 空資料。對應VT_EMPTY </summary>
        Empty = 0,
        /// <summary> null。對應VT_NULL </summary>
        Null = 1,
        /// <summary> <see cref="short"/>。對應VT_I2 </summary>
        Short = 2,
        /// <summary> <see cref="int"/>。對應VT_I4 </summary>
        Int = 3,
        /// <summary> <see cref="float"/>。對應VT_R4 </summary>
        Float = 4,
        /// <summary> <see cref="double"/>。對應VT_R8 </summary>
        Double = 5,
        /// <summary> <see cref="string"/>。對應VT_BSTR </summary>
        String = 8,
        /// <summary> IUnknown物件，這邊專門儲存<see cref="WzSerialize"/>物件。對應VT_DISPATCH </summary>
        Dispatch = 9,
        /// <summary> VARIANT_BOOL，0xFFFF為true，0x0000為false。對應VT_BOOL </summary>
        Boolean = 11,
        /// <summary> <see cref="uint"/>。對應VT_UI4 </summary>
        UInt = 19,
        /// <summary> <see cref="long"/>。對應VT_I8 </summary>
        Long = 20,
    }
}
