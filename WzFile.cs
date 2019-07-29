using System.IO;
using libwz.AES;
using libwz.IO;
using libwz.Tools;

namespace libwz
{
    /// <summary> 代表wz文件內的檔案結構 </summary>
    public class WzFile : WzArchiveItem
    {
        /// <summary> </summary>
        public WzKeyType KeyType { get; private set; }

        /// <summary> </summary>
        public Stream Stream { get; internal set; }

        /// <summary> </summary>
        public WzFile(string name, WzKeyType key = WzKeyType.None) : base(name, WzArchiveItemType.File)
        {
            this.KeyType = key;
            this.Stream = new MemoryStream();
        }

        /// <summary> </summary>
        public static WzFile FromFile(string path, WzKeyType key = WzKeyType.None)
        {
            WzFile file = new WzFile(Path.GetFileName(path), key);

            file.Stream = File.OpenRead(path);
            file.Size = (int)file.Stream.Length;

            return file;
        }

        internal override void Update()
        {
            if (this.Stream != null)
            {
                this.Checksum = (int)HashTools.GenerateChecksum(this.Stream, this.Offset, this.Size);
            }
        }
        internal override void Write(WzFileStream zs)
        {
            uint newoff = (uint)zs.Tell();

            if (this.Stream != null)
            {
                zs.WriteDataFromStream(this.Stream, this.Offset, this.Size);
            }

            this.Offset = newoff;
        }
    }
}
