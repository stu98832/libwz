using System;
using System.IO;
using System.Text;
using System.ComponentModel;
using libwz.IO;
using libwz.Tools;
using libwz.SoundDX8;

namespace libwz
{
    /// <summary> wz聲音物件 </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class WzSound : WzSerialize
    {
        /// <summary> 取得目前<see cref="WzSound"/>的Class名稱 </summary>
        public override string ClassName { get { return "Sound_DX8"; } }

        /// <summary> </summary>
        public byte Unknow1_Byte { get; private set; } // 00

        /// <summary> 取得聲音資料的大小 </summary>
        public int DataSize { get; internal set; }

        /// <summary> 取得聲音的長度(ms) </summary>
        public int Duration { get; internal set; }

        /// <summary> </summary>
        public byte Unknow3_Byte { get; private set; } // 02

        /// <summary> 取得聲音的媒體類型 </summary>
        public WzMediaType MediaType { get; internal set; }

        /// <summary> 取得聲音資料。若使用動態讀取，則會在取得聲音資料前從資料流把聲音資料讀取出來 </summary>
        [Browsable(false)]
        public byte[] SoundData
        {
            get
            {
                if (this.mStream != null)
                {
                    this.mStream.Seek(this.mSoundOffset);
                    return this.mStream.Read(this.DataSize);
                }
                return this.mSoundData;
            }
            internal set
            {
                this.mSoundData = value;
                this.mStream = null;
            }
        }

        /// <summary> 建立<see cref="WzSound"/>實體 </summary>
        /// <param name="name"> <see cref="WzShape2D"/>的名稱 </param>
        public WzSound(string name) : base(name)
        {
            this.Unknow1_Byte = 0;
            this.DataSize = 0;
            this.Duration = 0;
            this.Unknow3_Byte = 2;
            this.MediaType = WzMediaType.Default;
            this.mSoundOffset = 0;
            this.mSoundData = null;
            this.mStream = null;
        }

        /// <summary> 從指定檔案中載入聲音資料 </summary>
        /// <param name="path"></param>
        public void SetSoundFromFile(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(string.Format("找不到 {0} ", path));
            BinaryReader reader = new BinaryReader(File.OpenRead(path));
            string tag = Encoding.ASCII.GetString(reader.ReadBytes(4));

            if (tag == "RIFF")
            {
                SoundTools.LoadWAV(this, reader);
                return;
            }
            else if (tag.Substring(0, 3) == "ID3")
            {
                SoundTools.LoadMP3_ID3(this, reader);
                return;
            }
            else if (reader.BaseStream.Length >= 128)
            {
                reader.BaseStream.Seek(-128, SeekOrigin.End);
                if (Encoding.ASCII.GetString(reader.ReadBytes(3)) == "TAG")
                {
                    SoundTools.LoadMP3_TAG(this, reader);
                    return;
                }
            }
            try
            {
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                SoundTools.LoadMP3_ReadFrameData(this, reader, (int)(reader.BaseStream.Length));
                return;
            }
            catch (Exception)
            {
                throw new NotSupportedException("檔案必須要是 wave 或 mp3 格式");
            }
        }

        /// <summary> 產生一個<see cref="WzSound"/>的拷貝 </summary>
        public override WzSerialize Clone()
        {
            WzSound sound = new WzSound(this.Name);

            sound.Unknow1_Byte = this.Unknow1_Byte;
            sound.DataSize = this.DataSize;
            sound.Duration = this.Duration;
            sound.Unknow3_Byte = this.Unknow3_Byte;

            WzMediaType wzmt = this.MediaType;
            wzmt.pbFormat = new byte[sound.MediaType.cbFormat];
            this.MediaType.pbFormat.CopyTo(sound.MediaType.pbFormat, 0);
            sound.MediaType = wzmt;
            sound.mSoundData = new byte[sound.DataSize];
            this.SoundData.CopyTo(sound.mSoundData, 0);

            return sound;
        }

        /// <summary> 釋放<see cref="WzSound"/>所使用的資源 </summary>
        public override void Dispose()
        {
            this.mSoundData = null;
            this.mStream = null;
            base.Dispose();
        }

        internal override bool Read(WzFileStream stream)
        {
            this.Unknow1_Byte = stream.Read1u();
            this.DataSize = stream.Read4(true);
            this.Duration = stream.Read4(true);
            this.Unknow3_Byte = stream.Read1u();

            WzMediaType wzmt = new WzMediaType();
            wzmt.Read(stream);
            this.MediaType = wzmt;
            this.mSoundOffset = (uint)stream.Tell();
            if (stream.DynamicRead)
            {
                this.mStream = stream;
                stream.Skip(this.DataSize);
            }
            else
                this.SoundData = stream.Read(this.DataSize);
            return true;
        }
        internal override bool Write(WzFileStream stream)
        {
            stream.Write1u(this.Unknow1_Byte);
            stream.Write4(this.mSoundData.Length, true);
            stream.Write4(this.Duration, true);
            stream.Write1u(this.Unknow3_Byte);

            this.MediaType.Write(stream);

            stream.Write(this.mSoundData);

            return true;
        }

        private uint mSoundOffset;
        private byte[] mSoundData;
        private WzFileStream mStream;
    }
}
