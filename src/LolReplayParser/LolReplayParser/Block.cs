using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LolReplayParser
{
    [DebuggerDisplay("{Type} {ContentLen}")]
    public class Block: IBlock
    {
        public static string EMPTY = "    ";

        public int Count => 1;

        public string Mask { get; }
        public string Time { get; }
        public string ContentLen { get; }
        public string Type { get; }
        public string Blockparam { get; }
        public string Content { get; }

        public Block(string mask, string time, string contentLen, string type, string blockparam, string content)
        {
            this.Mask = mask;
            this.Time = time;
            this.ContentLen = contentLen;
            this.Type = type.PadLeft(4, ' ');
            this.Blockparam = blockparam;
            this.Content = content;
        }
    }
}
