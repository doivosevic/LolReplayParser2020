using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LolReplayParser
{
    [DebuggerDisplay("{Type} {AdditionalTypeInfo}")]
    public class BlockComposed : IBlock
    {
        public string Type { get; }

        public int Count => Blocks.Sum(b => b.Count);

        public List<IBlock> Blocks { get; }

        public BlockComposed(string type, List<IBlock> blocks)
        {
            this.Type = type;
            this.Blocks = blocks;
        }

        public string AdditionalTypeInfo
        {
            get
            {
                if (Blocks.Count > 1 && Blocks[1].Type == Block.EMPTY)
                {
                    return " + EMPTY " + (Blocks.Count - 1);
                }

                return "";
            }
        }

        public bool Same(IBlock other)
        {
            if (other is BlockComposed == false) return false;

            var cast = other as BlockComposed;
            if (cast.Blocks.Count != this.Blocks.Count) return false;

            return cast.Blocks.Zip(this.Blocks).All(bb => bb.First.Same(bb.Second));
        }
    }
}
