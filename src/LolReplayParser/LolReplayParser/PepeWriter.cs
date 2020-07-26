using LeagueReplayReader.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LolReplayParser
{
    public static class PepeWriter
    {
        public static List<(string filename, string content)> GetStringFromRofl(byte[] roflFile, int numberOfPayloads = -1, bool justKeyframes = false)
        {
            List<(string filename, string content)> files = new List<(string filename, string content)>();

            using (var roflStream = new MemoryStream(roflFile))
            {
                var payloads = new Replay(roflStream).GetAllPayloads(numberOfPayloads, justKeyframes);

                var lmaos = payloads.Where(p => p.Item2.Type == ReplayPayloadEntryType.Keyframe).Select(p => (p.Item2.ToString(), LmaoParser.GetBlocksFromLmao(p.Item2.Data)));

                var lmaoFiles = lmaos.Select(l => (l.Item1, string.Join(Environment.NewLine, l.Item2.Select(line =>
                {
                    return line.Type + "," + line.Time + "," + line.Blockparam + "," + line.Content;
                    string v = line.ToString().TrimStart('(').TrimEnd(')').Replace(" ", "");
                    return v;
                }))));

                return lmaoFiles.ToList();
            }
        }
    }
}
