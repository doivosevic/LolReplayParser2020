using System;
using System.Collections.Generic;
using System.Text;

namespace LolReplayParser
{
    public class Block
    {
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
            this.Type = type;
            this.Blockparam = blockparam;
            this.Content = content;
        }
    }
}
