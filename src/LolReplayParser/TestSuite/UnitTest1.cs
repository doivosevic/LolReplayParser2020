using LeagueReplayReader.Types;
using LolReplayParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Rofl.Reader.Parsers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using TestSuite.resources;


namespace TestSuite
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestRoflToLmao()
        {
            var roflFile = TestResources.EUN1_2389388796;

            using (var roflStream = new MemoryStream(roflFile))
            {
                var chunk1 = TestResources._2389388796_1_Chunk;
                var keyframe1 = TestResources._2389388796_1_Keyframe;
                var chunk10 = TestResources._2389388796_10_Chunk;
                var keyframe10 = TestResources._2389388796_10_Keyframe;

                var wrongKeyframe = TestResources._2440330860_9_Keyframe;

                var replay = new Replay(roflStream);
                List<(ReplayPayloadHeader, ReplayPayloadEntry)> payloads = replay.GetAllPayloads();

                Assert.IsTrue(payloads.Any(p => Utilities.AreEqual(p.Item2.Data, chunk1)));
                Assert.IsTrue(payloads.Any(p => Utilities.AreEqual(p.Item2.Data, chunk10)));
                Assert.IsTrue(payloads.Any(p => Utilities.AreEqual(p.Item2.Data, keyframe1)));
                Assert.IsTrue(payloads.Any(p => Utilities.AreEqual(p.Item2.Data, keyframe10)));

                Assert.IsTrue(payloads.All(p => !Utilities.AreEqual(p.Item2.Data, wrongKeyframe)));
            }
        }

        //private static List<(string, byte[])> GetResources()
        //{
        //    ResourceSet resourceSet = RoflFilesCollection.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

        //    var resources = new List<(string, byte[])>();

        //    foreach (DictionaryEntry entry in resourceSet)
        //    {
        //        string resourceKey = entry.Key.ToString();
        //        byte[] resource = (byte[])entry.Value;

        //        resources.Add((resourceKey, resource));
        //    }

        //    return resources;
        //}

        //[TestMethod]
        //public void BulkTestRoflToLmao()
        //{
        //    var resources = GetResources();
        //    var replayPayloads = resources.Select(r =>
        //    {
        //        using (var roflStream = new MemoryStream(r.Item2))
        //        {
        //            var replay = new Replay(roflStream);
        //            List<(ReplayPayloadHeader, ReplayPayloadEntry)> payloads = replay.GetAllPayloads(limit: 5);
        //            return payloads;
        //        }
        //    }).ToList();

        //    var keyframesPerFile = replayPayloads.Select(p => p.Where(pp => pp.Item2.Type == ReplayPayloadEntryType.Keyframe).Select(pp => pp.Item2.Data));
        //    var lmaoBlocksPerFile = keyframesPerFile.Select(k => k.Select(kk => LmaoParser.GetBlocksFromLmao(kk)));

        //    // List of files -> List of keyframes -> List of blocks

        //    var grouped = lmaoBlocksPerFile.Select(l => l.Select(b => GetRepetitionGroupations(b)).ToList()).ToList();
        //}

        [TestMethod]
        public void TestRoflToLmaoFile()
        {
            var roflFile = TestResources.EUN1_2389388796;

            var lmaoFiles = PepeWriter.GetStringFromRofl(roflFile);
        }

        [TestMethod]
        public void TestRoflJsonParsing()
        {
            var roflFile = TestResources.EUN1_2389388796;

            using (var stream = new MemoryStream(roflFile))
            {
                var replay = new RoflParser();
                var result = replay.ReadReplayAsync(stream).Result;
            }

            var json = RoflJson.GetJSON(Encoding.UTF8.GetString(roflFile));
            var statsRaw = json["statsJson"].ToObject<string>();
            var stats = JArray.Parse(statsRaw);
        }

        [TestMethod]
        public void TestLmaoToPepe()
        {
            byte[] keyframe1 = TestResources._2389388796_1_Keyframe;
            byte[] keyframe10 = TestResources._2389388796_10_Keyframe;

            var key1Blocks = LmaoParser.GetBlocksFromLmao(keyframe1);
            var key10Blocks = LmaoParser.GetBlocksFromLmao(keyframe10);

            Assert.IsTrue(key1Blocks.Count == 1904);
            Assert.IsTrue(key10Blocks.Count == 4269);

            var blocks = key10Blocks;
            var castBlocks = blocks.Cast<IBlock>().ToList();
            List<List<List<IBlock>>> groupations = GetRepetitionGroupations(castBlocks);

            var arrayedGroupations = groupations.Select(g => g.ToArray()).ToArray();

            var composedWithEmpties = CreateCompositionWithEmpties(castBlocks);
            int composedCount = composedWithEmpties.Sum(g => g.Count);

            var all = new List<Block>();
            for (int i = 0; i < composedWithEmpties.Count; i++)
            {
                if (composedWithEmpties[i] is Block) all.Add(composedWithEmpties[i] as Block);
                else all.AddRange((composedWithEmpties[i] as BlockComposed).Blocks.Cast<Block>());
            }

            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i] != all[i])
                {

                }
            }

            var flattened = groupations.SelectMany(g => g).SelectMany(g => g).ToList();
            int flattenedCount = flattened.Sum(b => b.Count);

            int inGroupations = arrayedGroupations.SelectMany(g => g).SelectMany(g => g).Sum(b => b.Count);
            Assert.AreEqual(blocks.Count(), inGroupations);
            //Assert.IsTrue(arrayedGroupations.SelectMany(g => g.SelectMany(gg => gg)).All(g => blocks.Any(b => b == g)));

            //Assert.IsTrue(arrayedGroupations[0].Count() == 4 && arrayedGroupations[2].Count() == 97 && arrayedGroupations[27].Count() == 4 && arrayedGroupations[59].Count() == 23 && arrayedGroupations[140].Count() == 7);
        }

        private static List<List<List<IBlock>>> GetRepetitionGroupations(List<IBlock> blocks)
        {
            var groupations = new List<List<List<IBlock>>>();

            var typeCount = blocks.GroupBy(g => g.Type).Select(g => (g.Key, g.Count())).OrderByDescending(g => g.Item2).ToList();

            Func<string, string> ToBin = hexstring => String.Join(String.Empty, hexstring.Select(c => Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')));

            var asString = typeCount.Where(t => t.Key[0] != ' ').Select(t => ToBin(t.Key)).ToList();

            List<IBlock> rest = CreateCompositionWithEmpties(blocks);

            Dictionary<string, (int, List<string>)> repeatingPatterns = new Dictionary<string, (int, List<string>)>();

            for (int i = 0; i < rest.Count; i++)
            {
                var n = rest[i];
                if (n.Type == Block.EMPTY) continue;

                if (repeatingPatterns.ContainsKey(n.Type))
                {
                    // verify pattern
                    int k = 0;
                    var nsTypes = repeatingPatterns[n.Type].Item2;

                    for (int j = i + 1; j < rest.Count; j++)
                    {
                        if (k >= nsTypes.Count || (nsTypes[k] != rest[j].Type && nsTypes[k] != rest[j - 1].Type && rest[j].Type != Block.EMPTY)) break;
                        if (nsTypes[k] == rest[j].Type) k++;
                    }

                    if ((k * 1.0 / nsTypes.Count) > 0.6)
                    {
                        string first = n.Type;
                        string last = nsTypes.Last();
                        // pattern matches
                        (var newRest, var newGroupations) = Collect(typeCount, first, last, rest);
                        if (newGroupations.Last().Count < 5)
                        {
                            // not enough repetitions
                            repeatingPatterns.Remove(n.Type);
                        }
                        else
                        {
                            groupations.AddRange(newGroupations);
                            rest = newRest;
                            repeatingPatterns.Clear();
                            i = 0;
                        }
                    }
                }
                else
                {
                    // start the pattern list
                    repeatingPatterns[n.Type] = (i, new List<string>());
                }

                // add type to pattern list of others
                foreach (var k in repeatingPatterns.Keys.Where(k => k != n.Type))
                {
                    var list = repeatingPatterns[k].Item2;
                    if (!list.Any() || list.Last() != n.Type)
                        repeatingPatterns[k].Item2.Add(n.Type);
                }
            }

            //Collect(groupations, typeCount, "6B00", "8B00", ref rest);
            //Collect(groupations, typeCount, "1C02", "4B01", ref rest); // LEN 2
            //Collect(groupations, typeCount, "A800", "7B01", ref rest); 
            //Collect(groupations, typeCount, "1701", "BB00", ref rest);
            ////Collect(groupations, typeCount, "9400", "BB00", ref rest);
            //Collect(groupations, typeCount, "5502", "7400", ref rest);
            //Collect(groupations, typeCount, "2101", "9C01", ref rest);

            groupations.Add(new[] { rest.ToList() }.ToList());
            return groupations;
        }

        private static List<IBlock> CreateCompositionWithEmpties(List<IBlock> blocks)
        {
            List<IBlock> rest = new List<IBlock>();
            BlockComposed lastComposed = null;

            var lastActual = blocks[0];
            for (int i = 0; i < blocks.Count; i++)
            {
                if (blocks[i].Type == Block.EMPTY)
                {
                    if (lastComposed == null)
                    {
                        var last = blocks[i - 1];
                        rest.RemoveAt(rest.Count - 1);
                        lastComposed = new BlockComposed(last.Type, new List<IBlock> { last });
                        rest.Add(lastComposed);
                    }

                    lastComposed.Blocks.Add(blocks[i]);
                }
                else
                {
                    rest.Add(blocks[i]);
                    lastComposed = null;
                }
            }

            return rest;
        }

        private static (List<IBlock> rest, List<List<List<IBlock>>> groupations) Collect(List<(string Key, int)> typeCount, string from, string to, IEnumerable<IBlock> restt)
        {
            var rest = restt.ToList();

            List<List<List<IBlock>>> groupations = new List<List<List<IBlock>>>();

            if (typeCount.Any(t => t.Key == from) == false || typeCount.Any(t => t.Key == to) == false) return default;

            // Collect group before the pattern
            var next = rest.TakeWhile(b => b.Type != from).ToList();
            if (next.Count() > 0) groupations.Add(new List<List<IBlock>> { next });
            rest = rest.Skip(next.Count()).ToList();


            rest = GroupIntoNextX(rest: rest, groupations: groupations, untilType: to);

            return (rest, groupations);
        }

        private static List<IBlock> GroupIntoNextX(
            List<IBlock> rest,
            List<List<List<IBlock>>> groupations,
            string untilType)
        {
            var group = new List<List<IBlock>>();
            int iOfNextUntilType;
            double avg;

            do
            {
                var next = TakeWhileIncluding(rest, untilType);
                rest = rest.Skip(next.Count).ToList();

                group.Add(next);

                iOfNextUntilType = rest.FindIndex(r => r.Type == untilType) + 1;
                avg = group.Average(g => g.Count);
            }
            while (iOfNextUntilType >= 0 && (iOfNextUntilType / avg > 0.8 && iOfNextUntilType / avg < 1.2));

            groupations.Add(group);

            return rest;
        }

        private static List<IBlock> TakeWhileIncluding(List<IBlock> rest, string untilType)
        {
            int indexOfType = rest.FindIndex(b => b.Type == untilType);

            if (indexOfType >= 0) return rest.Take(indexOfType + 1).ToList();
            else return rest;
        }

        [TestMethod]
        public void TestContent4()
        {
            string dirName = @"C:\Users\domin\Documents\League of Legends\Replays - Copy";
            Func<string, bool> filenameFilter = f => f.Contains("-3-Key") && !f.EndsWith(".bic");

            IEnumerable<(string f, byte[])> resources = Directory.GetFiles(dirName).Where(filenameFilter).Select(f => (f, File.ReadAllBytes(f)));

            var splitFile = new List<List<Block>>();

            foreach (byte[] res in resources.Select(r => r.Item2))
            {
                var blocks = LmaoParser.GetBlocksFromLmao(res);

                var byType = blocks.GroupBy(b => b.Type).Select(g => (g.Key, g.ToArray())).OrderByDescending(g => g.Item2.Length).ToArray();

                var groupedByType = new List<(string, List<Block>)>();

                groupedByType.Add(("first", new List<Block>()));

                foreach (var b in blocks)
                {
                    if (groupedByType.Last().Item1 == b.Type) groupedByType.Last().Item2.Add(b);
                    else groupedByType.Add((b.Type, new List<Block> { b }));
                }

                var gbt = groupedByType.OrderByDescending(g => g.Item2.Count).Select(g => g.Item2.ToArray()).ToArray();

                List<List<(string, List<Block>)>> evenMoreGroupedByType =
                    new List<List<(string, List<Block>)>>();

                evenMoreGroupedByType.Add(groupedByType.GetRange(1, 4));

                evenMoreGroupedByType.Add(groupedByType.GetRange(5, 20));
                evenMoreGroupedByType.Add(groupedByType.GetRange(25, 21));
                evenMoreGroupedByType.Add(groupedByType.GetRange(46, 20));
                evenMoreGroupedByType.Add(groupedByType.GetRange(66, 17));
                evenMoreGroupedByType.Add(groupedByType.GetRange(83, 17));

                evenMoreGroupedByType.Add(groupedByType.GetRange(100, 18));
                evenMoreGroupedByType.Add(groupedByType.GetRange(118, 20));
                evenMoreGroupedByType.Add(groupedByType.GetRange(138, 18));
                evenMoreGroupedByType.Add(groupedByType.GetRange(156, 17));
                evenMoreGroupedByType.Add(groupedByType.GetRange(173, 16));

                splitFile.Add(blocks);
            }

            var all = splitFile.SelectMany(f => f).ToList();

            var distinct = all.GroupBy(k => k.Mask + k.Time + k.ContentLen + k.Type + k.Blockparam + k.Content).OrderByDescending(g => g.Count()).Select(g => g.ToArray()).ToArray();

            int total = resources.Count();
        }
    }
}
