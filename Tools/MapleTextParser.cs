using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace libwz.Tools
{
    /// <summary> To parse text used in maplestory. </summary>
    public class MapleTextParser
    {

        /// <summary> source text </summary>
        public string Source { get; private set; }

        /// <summary> prase position </summary>
        public int Position { get; private set; }

        /// <summary> token </summary>
        public string Token { get; private set; }

        /// <summary> </summary>
        public bool EndOfText
        {
            get
            {
                return this.Source == null || this.Position >= this.Source.Length;
            }
        }

        /// <summary> load and initialize source text and position </summary>
        public void Load(string src)
        {
            this.Source = src;
            this.Position = 0;
        }

        /// <summary> </summary>
        public MapleTextFormat Next(bool reserveTag = true)
        {
            if (this.EndOfText)
            {
                return MapleTextFormat.ParseFail;
            }

            if (this.Source[this.Position] == '#')
            {
                this.Token = "";
                ++this.Position;

                if (this.EndOfText)
                {
                    return MapleTextFormat.ParseFail;
                }

                char ch = this.Source[this.Position];

                if (Enum.IsDefined(typeof(MapleTextFormat), (int)ch))
                {
                    ++this.Position;
                    return (MapleTextFormat)ch;
                }

                return MapleTextFormat.UnknowFormat;
            }
            else
            {
                string token = "";

                while (!this.EndOfText && this.Source[this.Position] != '#')
                {
                    token += this.Source[this.Position++];
                }

                if (!reserveTag)
                {
                    ++this.Position;
                }

                this.Token = token.Replace("\\r", "\r").Replace("\\n", "\n");

                return MapleTextFormat.PlainText;
            }
        }

        /// <summary> </summary>
        public int NumToken()
        {
            Regex number = new Regex("^([0-9]+)");
            Match result = number.Match(this.Token);

            if (result.Success)
            {
                return int.Parse(result.Value);
            }
            else
            {
                return -1;
            }
        }
    }
}
