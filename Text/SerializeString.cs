using System;
using System.Text;
using libwz.IO;

namespace libwz.Text
{
    /// <summary> 序列化文字 </summary>
    public class SerializeString
    {
        /// <summary> 從指定的<see cref="WzFileStream"/>中讀取文字 </summary>
        public static string Read(WzFileStream stream)
        {
            int len = stream.Read1();
            bool uni = len > 0;
            int flag = (uni ? len : ~len);

            if (len == 0)
            {
                return "";
            }

            len = flag == 0x7F ? stream.Read4() : Math.Abs(len);

            byte[] str = stream.Read(len * (uni ? 2 : 1), true);

            Process(str, len, uni);

            return (uni ? Encoding.Unicode : Encoding.ASCII).GetString(str);
        }
        /// <summary> 將文字寫入指定的<see cref="WzFileStream"/>中 </summary>
        public static void Write(WzFileStream stream, string str)
        {
            int len = str.Length;
            bool uni = false;

            foreach (char ch in str)
            {
                if ((int)ch > 0xFF)
                {
                    uni = true;
                    break;
                }
            }

            byte[] chars = (uni ? Encoding.Unicode : Encoding.ASCII).GetBytes(str);

            Process(chars, len, uni);

            if ((uni && len <= 127) || (!uni && len <= 128))
            {
                stream.Write1((sbyte)(uni ? len : -len));
            }
            else
            {
                stream.Write1u((byte)(uni ? 0x7F : 0x80));
                stream.Write4(len);
            }

            stream.Write(chars, true);
        }

        // Process string to serialize string
        private static unsafe void Process(byte[] src, int len, bool unicode)
        {
            uint chkey = 0xAAAA;
            fixed (byte* pointer = src)
            {
                byte* ch = pointer;
                ushort* wch = (ushort*)pointer;
                for (int i = 0; i < len; ++i)
                {
                    if (unicode)
                        *(wch++) ^= (ushort)chkey++;
                    else
                        *(ch++) ^= (byte)chkey++;
                }
            }
        }
    }
}
