using System;
using System.ComponentModel;
using libwz.IO;
using libwz.SoundDX8;

namespace libwz
{
    /// <summary> wz所使用的MediaType。類似 DirectSound 的 AM_MEDIA_TYPE </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct WzMediaType
    {
        /// <summary> </summary>
        public Guid majortype { get; internal set; }
        /// <summary> </summary>
        public Guid subtype { get; internal set; }
        /// <summary> </summary>
        public byte Unknow1_Byte { get; internal set; }
        /// <summary> </summary>
        public byte Unknow2_Byte { get; internal set; }
        /// <summary> </summary>
        public Guid formattype { get; internal set; }
        /// <summary> </summary>
        public uint cbFormat { get; internal set; }
        /// <summary> </summary>
        public byte[] pbFormat { get; internal set; }

        /// <summary> <see cref="WzSound"/>默認的<see cref="WzMediaType"/>配置(MP3) </summary>
        public readonly static WzMediaType Default = new WzMediaType()
        {
            majortype = SoundDX8Constants.MEDIATYPE_Stream,
            subtype = SoundDX8Constants.MEDIASUBTYPE_WAVE,
            Unknow1_Byte = 0,
            Unknow2_Byte = 1,
            formattype = SoundDX8Constants.WMFORMAT_WaveFormatEx,
            cbFormat = 30, // WaveFormat(18) + MPGELayer3(12)
            pbFormat = new byte[] {
                0x55, 0x00,             // wFormat         : WAVE_FORMAT_MPGELAYER3
                0x02, 0x00,             // nChannels       : 2
                0xE4, 0x57, 0x00, 0x00, // nSamplesPerSec  : 22500
                0x10, 0x27, 0x00, 0x00, // nAvgBytesPerSec : 10000
                0x01, 0x00,             // nBlockAlign     : 1
                0x00, 0x00,             // wBitsPerSample  : 0
                0x0C, 0x00,             // cbSize          : 12
                0x01, 0x00,             // wID             : MPEGLAYER3_ID_MPEG
                0x02, 0x00, 0x00, 0x00, // fdwFlags        : MPEGLAYER3_FLAG_PADDING_OFF
                0x0A, 0x02,             // nBlockSize      : 522
                0x01, 0x00,             // nFramesPerBlock : 1
                0x00, 0x00,             // nCodecDelay     : 0
            }
        };

        internal void Read(WzFileStream stream)
        {
            this.majortype = new Guid(stream.Read(16));          //MEDIATYPE_Stream
            this.subtype = new Guid(stream.Read(16));            //MEDIASUBTYPE_WAVE
            this.Unknow1_Byte = stream.Read1u();
            this.Unknow2_Byte = stream.Read1u();
            this.formattype = new Guid(stream.Read(16));         //WMFORMAT_WaveFormatEx

            if (this.formattype == SoundDX8Constants.WMFORMAT_WaveFormatEx)
            {
                this.cbFormat = (uint)stream.Read4(true);
                this.pbFormat = stream.Read((int)this.cbFormat);
            }
        }
        internal void Write(WzFileStream stream)
        {
            stream.Write(this.majortype.ToByteArray());
            stream.Write(this.subtype.ToByteArray());
            stream.Write1u(this.Unknow1_Byte);
            stream.Write1u(this.Unknow2_Byte);
            stream.Write(this.formattype.ToByteArray());

            if (this.formattype == SoundDX8Constants.WMFORMAT_WaveFormatEx)
            {
                stream.Write4((int)this.cbFormat, true);
                stream.Write(this.pbFormat);
            }
        }
    }
}
