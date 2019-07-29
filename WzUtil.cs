using libwz.AES;

namespace libwz
{
    /// <summary> </summary>
    public static class WZUtil
    {
        /// <summary> </summary>
        public static byte[] GlobalKey_K;

        /// <summary> </summary>
        public static byte[] GlobalKey_G;

        /// <summary> </summary>
        public static void XORBuffer(byte[] src, byte[] buffer, int len)
        {
            if (buffer == null)
                return;
            for (int i = 0; i < len; ++i)
                src[i] ^= buffer[i];
        }

        /// <summary> </summary>
        public static void XORKey(byte[] src, int len, WzKeyType type)
        {
            if (type == WzKeyType.None)
                return;
            if (type == WzKeyType.K)
            {
                if (WZUtil.GlobalKey_K == null || WZUtil.GlobalKey_K.Length < len)
                    WZUtil.GlobalKey_K = WzAES.GenKey(WzAES.IVk, len);
                XORBuffer(src, WZUtil.GlobalKey_K, len);
            }
            else if (type == WzKeyType.G)
            {
                if (WZUtil.GlobalKey_G == null || WZUtil.GlobalKey_G.Length < len)
                    WZUtil.GlobalKey_G = WzAES.GenKey(WzAES.IVg, len);
                XORBuffer(src, WZUtil.GlobalKey_G, len);
            }
        }

        // System function
        internal static uint _rotl(uint x, byte n)
        {
            return ((x << n) | ((x) >> (32 - n)));
        }
        internal static uint _rotr(uint x, byte n)
        {
            return ((x >> n) | ((x) << (32 - n)));
        }
    }
}
