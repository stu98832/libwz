using libwz.IO;

namespace libwz
{
    /// <summary> </summary>
    public abstract class WzArchiveItem
    {
        /// <summary> </summary>
        public WzArchiveItemType Type { get; private set; }

        /// <summary> </summary>
        public string Name { get; set; }

        /// <summary> </summary>
        public int Size { get; internal set; }

        /// <summary> </summary>
        public int Checksum { get; internal set; }

        /// <summary> </summary>
        public uint Offset { get; internal set; }

        /// <summary> </summary>
        public WzArchive Archive { get; internal set; }

        /// <summary> </summary>
        public WzDirectory Parent { get; internal set; }

        /// <summary> </summary>
        public WzArchiveItem(string name, WzArchiveItemType type)
        {
            this.Type = type;
            this.Name = name;
            this.Offset = 0;
            this.Checksum = 0;
            this.Size = 0;
            this.Archive = null;
            this.Parent = null;
        }

        internal abstract void Update();
        internal abstract void Write(WzFileStream zs);
    }
}
