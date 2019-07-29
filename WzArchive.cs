using System.IO;
using libwz.AES;
using libwz.IO;
using libwz.Tools;

namespace libwz
{
    /// <summary> wz打包文件 </summary>
    public class WzArchive
    {
        /// <summary> 文件大小 </summary>
        public long DataSize { get; private set; }

        /// <summary> 文件位址 </summary>
        public uint DataOffset { get; private set; }

        /// <summary> 文件描述 </summary>
        public string Description { get; set; }

        /// <summary> 根目錄 </summary>
        public WzDirectory RootDirectory { get; private set; }

        /// <summary> 建立一個<see cref="WzArchive"/>實體 </summary>
        public WzArchive()
        {
            this.Init(new WzDirectory(""));
        }

        /// <summary> 建立一個連接指定<see cref="WzDirectory"/>的<see cref="WzArchive"/>實體 </summary>
        /// <param name="link"> 要進行連結的<see cref="WzDirectory"/>實例 </param>
        public WzArchive(WzDirectory link)
        {
            this.Init(link);
        }

        /// <summary> 文件雜湊碼 </summary>
        public int Hash { get; private set; }

        /// <summary> 文件版本 </summary>
        public int Version { get; set; }

        /// <summary> 文件資料流 </summary>
        public WzFileStream Stream { get; set; }

        /// <summary></summary>
        public WzArchiveItem this[string path]
        {
            get
            {
                path = path.Replace('\\', '/');

                int index = path.IndexOf("/"), i = 0;

                if (index == -1)
                {
                    return this.RootDirectory[path];
                }

                string linkName = path.Substring(0, index);
                string linkPath = path.Substring(index + 1);
                WzArchiveItem obj = null;

                do
                {
                    WzDirectory zcLink = this.RootDirectory[linkName + (++i == 1 ? "" : i.ToString())] as WzDirectory;
                    if (zcLink == null)
                    {
                        return null;
                    }
                    obj = zcLink[linkPath];
                } while (obj == null);

                return obj;
            }
        }

        /// <summary> 從指定路徑建立<see cref="WzFileStream"/>並讀取資料 </summary>
        public bool Open(string path, WzKeyType key)
        {
            this.Stream = new WzFileStream(path, FileMode.Open, key);
            return this.Read(this.Stream);
        }

        /// <summary> 使用自身的<see cref="WzFileStream"/>讀取資料 </summary>
        public bool Read()
        {
            return this.Read(this.Stream);
        }

        /// <summary> 使用外部的<see cref="WzFileStream"/>讀取資料 </summary>
        public bool Read(WzFileStream zs)
        {
            string identifier = zs.ReadString(4);

            if (!identifier.Equals("PKG1"))
            {
                return false;
            }

            this.DataSize = zs.Read8();
            this.DataOffset = zs.Read4u();
            this.Description = zs.ReadString((int)(this.DataOffset - zs.Tell()));

            zs.BaseOffset = this.DataOffset;

            ushort cryptedHash = zs.Read2u();

            this.RootDirectory.Offset = (uint)zs.Tell();
            for (int j = 0; j < 0xFFFF; j++)
            {
                this.Hash = HashTools.GenerateArchiveVersionHash(j.ToString());
                if (HashTools.EncryptArchiveVersionHash(this.Hash) == cryptedHash)
                {
                    zs.StringPool.Clear();
                    if (this.RootDirectory.Read(zs))
                    {
                        this.Version = j;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary> 使用自身的<see cref="WzFileStream"/>儲存資料 </summary>
        public bool Write()
        {
            return this.Write(this.Stream);
        }

        /// <summary> 使用外部的的<see cref="WzFileStream"/>儲存資料 </summary>
        public bool Write(WzFileStream zs)
        {
            this.Hash = HashTools.GenerateArchiveVersionHash(this.Version.ToString());

            // header
            zs.Write(new byte[] { (byte)'P', (byte)'K', (byte)'G', (byte)'1' }, 4);
            zs.Write8(0);//Reserve
            zs.Write4u(0);//Reserve
            zs.WriteString(this.Description);

            // data
            long off = zs.Tell();
            this.DataOffset = (uint)off;
            zs.BaseOffset = this.DataOffset;

            zs.Write2u((ushort)HashTools.EncryptArchiveVersionHash(this.Hash));
            this.RootDirectory.Update();
            this.RootDirectory.Write(zs);

            long endoff = zs.Tell();

            // rewrite size, offset
            zs.Seek(4);
            zs.Write8(endoff - off);
            zs.Write4u((uint)off);

            // end write
            zs.Flush();

            return true;
        }

        /// <summary> 從指定路徑讀取wz文件並與自身的<see cref="WzDirectory"/>進行連結，用於Base.wz</summary>
        /// <param name="path"> 連接檔案路徑 </param>
        public bool BaseLink(string path)
        {
            if (this.RootDirectory == null)
            {
                return false;
            }

            bool result = true;
            foreach (WzArchiveItem item in this.RootDirectory)
            {
                if (item.Type != WzArchiveItemType.Directory)
                {
                    continue;
                }
                WzDirectory catalog = item as WzDirectory;
                WzArchive zp = new WzArchive(catalog);
                zp.Stream = new WzFileStream(Path.Combine(path, catalog.Name + ".wz"), FileMode.Open, this.Stream.KeyType);
                result = zp.Read() ? result : false;
            }
            return result;
        }

        /// <summary> 透過指定路徑取得對應的<see cref="WzVariant"/> </summary>
        /// <param name="path"> <see cref="WzVariant"/>的所在位置 </param>
        /// <param name="img"> 尋找時所開啟的<see cref="WzImage"/>物件 </param>
        public WzVariant GetVariant(string path, ref WzImage img)
        {
            int index = path.IndexOf(".img/") + 4;
            WzFile file = this[path.Substring(0, index)] as WzFile;
            if (file != null)
            {
                img = WzImage.FromWzFile(file);
                return img[path.Substring(index + 1)];
            }
            return WzVariant.Null;
        }

        /// <summary> 取得指定路徑的<see cref="WzVariant"/>拷貝 </summary>
        /// <param name="path"> <see cref="WzVariant"/>的所在位置 </param>
        public WzVariant CloneVariant(string path)
        {
            WzImage img = null;
            WzVariant v = GetVariant(path, ref img).Clone();
            if (img != null)
            {
                img.Dispose();
            }
            return v;
        }

        private void Init(WzDirectory linkCatalog)
        {
            linkCatalog.Archive = this;
            this.Description = "\0";
            this.Version = 0;
            this.RootDirectory = linkCatalog;
        }
    }
}
