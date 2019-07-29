using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using libdds;

namespace libwz.Tools
{
    /// <summary> </summary>
    public class CanvasTools
    {
        /// <summary> </summary>
        public static unsafe Bitmap Decompress(WzCanvasFormat format, byte scale, int width, int height, byte[] datas)
        {
            int cbPixel = width * height;

            PixelFormat pxFormat = (format == WzCanvasFormat.B5G6R5) ? PixelFormat.Format16bppRgb565 : PixelFormat.Format32bppArgb;
            Bitmap bmp = new Bitmap(width, height, pxFormat);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pxFormat);

            int decw = width >> scale, dech = height >> scale;
            int cbSize = (format == WzCanvasFormat.B5G6R5 || format == WzCanvasFormat.B4G4R4A4) ? 2 : (format == WzCanvasFormat.B8G8R8A8) ? 4 : 1;
            byte[] buffer = new byte[cbPixel * cbSize];

            ZoomInPixels(buffer, CanvasZlibTool.Decompress(datas, decw * dech * cbSize), decw, dech, 1 << scale, cbSize);

            switch (format)
            {
                case WzCanvasFormat.B4G4R4A4:
                    fixed (byte* pSrc = buffer)
                    {
                        Unpack4444((int*)bmpData.Scan0, (short*)pSrc, width, height);
                    }
                    break;
                case WzCanvasFormat.B5G6R5:
                case WzCanvasFormat.B8G8R8A8:
                    Marshal.Copy(buffer, 0, bmpData.Scan0, buffer.Length);
                    break;
                case WzCanvasFormat.DDS_DXT3:
                {
                    byte[] imagedata = dxt.decompressImage(buffer, width, height, dxt.kDxt3);
                    Marshal.Copy(imagedata, 0, bmpData.Scan0, imagedata.Length);
                }
                break;
                case WzCanvasFormat.DDS_DXT5:
                {
                    byte[] imagedata = dxt.decompressImage(buffer, width, height, dxt.kDxt5);
                    Marshal.Copy(imagedata, 0, bmpData.Scan0, imagedata.Length);
                }
                break;
            }
            bmp.UnlockBits(bmpData);

            return bmp;
        }

        /// <summary> </summary>
        public static unsafe byte[] Compress(WzCanvasFormat format, byte scale, int width, int height, Bitmap src)
        {
            PixelFormat pxFormat = (format == WzCanvasFormat.B5G6R5) ? PixelFormat.Format16bppRgb565 : PixelFormat.Format32bppArgb;

            int cw = width >> scale, ch = height >> scale;
            int cbSize = (format == WzCanvasFormat.B5G6R5 || format == WzCanvasFormat.B4G4R4A4) ? 2 : (format == WzCanvasFormat.B8G8R8A8) ? 4 : 1;
            Rectangle rect = new Rectangle(0, 0, cw, ch);
            Bitmap bmp = new Bitmap(src, cw, ch).Clone(rect, pxFormat);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.WriteOnly, pxFormat);

            byte[] compress = null;

            switch (format)
            {
                case WzCanvasFormat.B4G4R4A4:
                    compress = new byte[cw * ch * cbSize];
                    fixed (byte* pDest = compress)
                    {
                        Pack4444(pDest, (byte*)bmpData.Scan0, cw, ch);
                    }
                    break;
                case WzCanvasFormat.B5G6R5:
                case WzCanvasFormat.B8G8R8A8:
                    compress = new byte[bmpData.Stride * bmpData.Height];
                    Marshal.Copy(bmpData.Scan0, compress, 0, compress.Length);
                    break;
                case WzCanvasFormat.DDS_DXT3:
                {
                    byte[] buffer = new byte[bmpData.Stride * bmpData.Height];
                    Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length);
                    compress = dxt.compressImage(buffer, cw, ch, dxt.kDxt3);
                }
                break;
                case WzCanvasFormat.DDS_DXT5:
                {
                    byte[] buffer = new byte[bmpData.Stride * bmpData.Height];
                    Marshal.Copy(bmpData.Scan0, buffer, 0, buffer.Length);
                    compress = dxt.compressImage(buffer, cw, ch, dxt.kDxt5);
                }
                break;
            }
            bmp.UnlockBits(bmpData);

            return CanvasZlibTool.Compress(compress, compress.Length);
        }

        private static unsafe void Unpack4444(int* pDest, short* pSrc, int width, int height)
        {
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x, ++pSrc)
                {
                    *pDest++ = ((*pSrc & 0x000F) * 0x00011) |
                               ((*pSrc & 0x00F0) * 0x00110) |
                               ((*pSrc & 0x0F00) * 0x01100) |
                               ((*pSrc & 0xF000) * 0x11000);
                }
            }
        }
        private static unsafe void Pack4444(byte* pDest, byte* pSrc, int width, int height)
        {
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    for (int i = 0; i < 2; ++i)
                    {
                        byte b1 = (byte)((pSrc++[0] + 0x08) / 0x11);
                        byte b2 = (byte)((pSrc++[0] + 0x08) / 0x11);

                        pDest++[0] = (byte)((b1) | (b2 << 4));
                    }
                }
            }
        }

        private static void ZoomInPixels(byte[] dest, byte[] src, int width, int height, int scale, int size)
        {
            if (scale == 1)
            {
                src.CopyTo(dest, 0);
                return;
            }

            int LineWidth1 = width * size * scale;
            for (int y0 = 0; y0 < height; ++y0)
            {
                int y1 = y0 * scale;
                for (int x0 = 0; x0 < width; ++x0)
                {
                    int pos0 = (y0 * width + x0);
                    int pos1 = ((y1 * width + x0) * scale);
                    for (int k = 0; k < scale; ++k)
                        Array.Copy(src, pos0 * size, dest, (pos1 + k) * size, size);
                }

                for (int k = 0; k < scale; ++k)
                    Array.Copy(dest, y1 * LineWidth1, dest, (y1 + k) * LineWidth1, LineWidth1);
            }
        }
    }
}
