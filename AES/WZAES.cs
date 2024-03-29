﻿using System.Security.Cryptography;
using System.IO;

namespace libwz.AES
{
    /// <summary> </summary>
    public class WzAES
    {
        /// <summary> </summary>
        private static byte[] AESKey = new byte[] {
            0x13, 0, 0, 0, 0x08, 0, 0, 0,
            0x06, 0, 0, 0, 0xB4, 0, 0, 0,
            0x1b, 0, 0, 0, 0x0F, 0, 0, 0,
            0x33, 0, 0, 0, 0x52, 0, 0, 0
        };

        /// <summary> </summary>
        public static readonly byte[] IVk = { 0xB9, 0x7D, 0x63, 0xE9, };

        /// <summary> </summary>
        public static readonly byte[] IVg = { 0x4D, 0x23, 0xC7, 0x2B, };

        /// <summary> </summary>
        public static byte[] GenKey(byte[] iv, int len)
        {
            byte[] buffer = new byte[len];

            using (Rijndael rij = Rijndael.Create())
            {
                rij.Key = WzAES.AESKey;
                rij.Mode = CipherMode.ECB;

                using (MemoryStream stream = new MemoryStream())
                {
                    ICryptoTransform ct = rij.CreateEncryptor();
                    CryptoStreamMode csm = CryptoStreamMode.Write;
                    using (CryptoStream crypto = new CryptoStream(stream, ct, csm))
                    {
                        byte[] transform = new byte[16];
                        for (int i = 0; i < 4; ++i)
                            for (int j = 0; j < 4; ++j)
                                transform[i * 4 + j] = iv[j];

                        for (int i = 0; i < (len / 16) + 1; ++i)
                        {
                            crypto.Write(transform, 0, 16);
                            stream.Seek(-16, SeekOrigin.Current);
                            stream.Read(transform, 0, 16);
                        }
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Read(buffer, 0, len);
                    }

                    return buffer;
                }
            }
        }

        /// <summary> </summary>
        public static byte[] GenKey(WzKeyType key, int len)
        {
            if (len == 0)
                return null;
            if (key == WzKeyType.G)
                return WzAES.GenKey(IVg, len);
            if (key == WzKeyType.K)
                return WzAES.GenKey(IVk, len);
            return null;
        }
    }

    /* 加密過的 ClassName
       Property
       K : 0xF8 0x51 0xBC 0x8A 0xD8 0xAC 0x10 0x93
       G : 0xF8 0x6C 0x77 0xFC 0x79 0x83 0x27 0x19
       - : 0xF8 0xFA 0xD9 0xC3 0xDD 0xCB 0xDD 0xC4
       Canvas
       K : 0xFA 0x42 0xAF 0x8B 0xDE 0xA8
       G : 0xFA 0x7F 0x64 0xFD 0x7F 0x87
       - : 0xFA 0xE9 0xCA 0xC2 0xDB 0xCF
       Shape2D#Vector2D
       K : 0xF0 0x52 0xA6 0x84 0xD8 0xAC 0x50 0xA3 0x98 0x7C 0xAD 0x50 0xCB 0x35 0xA0 0x8D
       G : 0xF0 0x6F 0x6D 0xF2 0x79 0x83 0x67 0x29 0x02 0xA2 0xA0 0xD2 0xA0 0x4E 0x0B 0xF2
       - : 0xF0 0xF9 0xC3 0xCD 0xDD 0xCB 0x9D 0xF4 0x92 0xE4 0xD6 0xD7 0xC1 0xD9 0xC5 0x8A
       Shape2D#Convex2D
       K : 0xF0 0x52 0xA6 0x84 0xD8 0xAC 0x50 0xA3 0x98 0x69 0xA7 0x5D 0xC9 0x3F 0xAA 0x8D
       G : 0xF0 0x6F 0x6D 0xF2 0x79 0x83 0x67 0x29 0x02 0xB7 0xAA 0xDF 0xA2 0x44 0x01 0xF2
       - : 0xF0 0xF9 0xC3 0xCD 0xDD 0xCB 0x9D 0xF4 0x92 0xF1 0xDC 0xDA 0xC3 0xD3 0xCF 0x8A
       UOL
       K : 0xFD 0x54 0x81
       G : 0xFD 0x69 0x4A
       - : 0xFD 0xFF 0xE4
       Sound_DX8
       K : 0xF7 0x52 0xA1 0x90 0xC6 0xAD 0x3D 0xA3 0xE3
       G : 0xF7 0x6F 0x6A 0xE6 0x67 0x82 0x0A 0x29 0x79
       - : 0xF7 0xF9 0xC4 0xD9 0xC3 0xCA 0xF0 0xF4 0xE9
    */
}
