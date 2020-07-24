using System.Diagnostics;

namespace LolReplayParser
{
    public interface IBlock
    {
        public string Type { get; }

        public int Count { get; }
    }
}