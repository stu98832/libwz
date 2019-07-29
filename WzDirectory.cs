using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using libwz.IO;
using libwz.Tools;

namespace libwz
{
    /// <summary> </summary>
    public class WzDirectory : WzArchiveItem, ICollection<WzArchiveItem>
    {
        /// <summary> </summary>
        public List<WzArchiveItem> Items { get; private set; }

        /// <summary> </summary>
        public int Count { get { return this.Items.Count; } }

        /// <summary> </summary>
        public bool IsReadOnly { get { return false; } }

        /// <summary> </summary>
        public WzDirectory(string name) : base(name, WzArchiveItemType.Directory)
        {
            this.Items = new List<WzArchiveItem>();
        }

        /// <summary> </summary>
        public WzArchiveItem this[string path]
        {
            get
            {
                WzDirectory dir = this;
                string[] names = path.Replace('\\', '/').Split('/');
                int i;

                for (i = 0; i < names.Length - 1 && dir != null; ++i)
                {
                    dir = dir[names[i]] as WzDirectory;
                }

                if (dir != null)
                {
                    foreach (WzArchiveItem o in dir.Items)
                    {
                        if (o.Name == names[i])
                        {
                            return o;
                        }
                    }
                }
                return null;
            }
        }

        /// <summary> </summary>
        public void Extract(string name, Stream s)
        {
            WzFile zf = this[name] as WzFile;

            if (zf != null)
            {
                this.Archive.Stream.ReadDataToStream(s, zf.Offset, zf.Size);
            }
        }

        /// <summary> </summary>
        public void Add(WzArchiveItem item)
        {
            bool samename = this[item.Name] != null;
            if (!samename)
            {
                item.Parent = this;
                item.Archive = this.Archive;
                this.Items.Add(item);
            }
        }

        /// <summary> </summary>
        public void Clear()
        {
            this.Items.Clear();
        }

        /// <summary> </summary>
        public bool Contains(WzArchiveItem item)
        {
            return this.Items.Contains(item);
        }

        /// <summary> </summary>
        public void CopyTo(WzArchiveItem[] array, int arrayIndex)
        {
            this.Items.CopyTo(array, arrayIndex);
        }

        /// <summary> </summary>
        public bool Remove(WzArchiveItem item)
        {
            return this.Items.Remove(item);
        }

        /// <summary> </summary>
        public IEnumerator<WzArchiveItem> GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }

        internal bool Read(WzFileStream zs)
        {
            this.Items.Clear();
            zs.Seek(this.Offset);

            int count = zs.Read4(true);

            for (int i = 0; i < count; i++)
            {
                WzArchiveItem item = null;
                WzArchiveItemType itemtype;
                string itemname = zs.StringPool.DirectoryRead(out itemtype, WzArchiveItemType.Reference);

                if (itemtype == WzArchiveItemType.Directory)
                {
                    item = new WzDirectory(itemname);
                }
                else if (itemtype == WzArchiveItemType.File)
                {
                    item = new WzFile(itemname, zs.KeyType) { Stream = zs.BaseStream };
                }
                else
                {
                    throw new InvalidDataException("Undefined item type : " + (int)itemtype);
                }

                item.Size = zs.Read4(true);
                item.Checksum = zs.Read4(true);
                uint offKey = HashTools.GenerateOffsetKey((uint)zs.Tell(), this.Archive.DataOffset, this.Archive.Hash);
                item.Offset = HashTools.DecryptOffsetHash(zs.Read4u(), offKey, this.Archive.DataOffset);

                if ((item.Offset + item.Size) > zs.Length)
                {
                    return false;
                }

                this.Add(item);
            }
            for (int i = 0; i < this.Items.Count; i++)
                if (this.Items[i].Type == WzArchiveItemType.Directory)
                    (this.Items[i] as WzDirectory).Read(zs);

            return true;
        }
        internal override void Write(WzFileStream zf)
        {
            // start write
            int count = this.Items.Count;
            long[] itemoff = new long[count];

            this.Offset = (uint)zf.Tell();

            zf.Write4(count, true);

            // write header
            for (int i = 0; i < count; i++)
            {
                WzArchiveItem item = this.Items[i];
                zf.StringPool.DirectoryWrite(item.Name, item.Type, WzArchiveItemType.Reference);
                zf.Write4(item.Size, true);
                zf.Write4(item.Checksum, true);

                itemoff[i] = zf.Tell();
                zf.Write4u(0u); // reserve
            }

            // package items
            for (int i = 0; i < count; i++)
                this.Items[i].Write(zf);

            long endoff = zf.Tell();

            // rewrite offset
            for (int i = 0; i < count; i++)
            {
                zf.Seek(itemoff[i]);
                uint offKey = HashTools.GenerateOffsetKey((uint)zf.Tell(), this.Archive.DataOffset, this.Archive.Hash);
                uint encoff = HashTools.EncryptOffsetHash(this.Items[i].Offset, offKey, this.Archive.DataOffset);
                zf.Write4u(encoff);
            }

            // end write
            zf.Seek(endoff);
        }
        internal override void Update()
        {
            uint checksum = 0u;
            int size = 0;

            for (int i = 0; i < this.Items.Count; i++)
            {
                this.Items[i].Update();
                checksum += (uint)this.Items[i].Checksum;
                size += this.Items[i].Size;
            }

            this.Checksum = (int)checksum;
            this.Size = size;
        }

        /// <summary> </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Items.GetEnumerator();
        }
    }
}
