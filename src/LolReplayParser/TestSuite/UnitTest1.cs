using LeagueReplayReader.Types;
using LolReplayParser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using TestSuite.resources;
using TestSuite.resources.manyRoflFiles;

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

        private static List<(string, byte[])> GetResources()
        {
            ResourceSet resourceSet = RoflFilesCollection.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);

            var resources = new List<(string, byte[])>();

            foreach (DictionaryEntry entry in resourceSet)
            {
                string resourceKey = entry.Key.ToString();
                byte[] resource = (byte[])entry.Value;

                resources.Add((resourceKey, resource));
            }

            return resources;
        }

        [TestMethod]
        public void BulkTestRoflToLmao()
        {
            var resources = GetResources();
            var replayPayloads = resources.Select(r =>
            {
                using (var roflStream = new MemoryStream(r.Item2))
                {
                    var replay = new Replay(roflStream);
                    List<(ReplayPayloadHeader, ReplayPayloadEntry)> payloads = replay.GetAllPayloads(limit: 5);
                    return payloads;
                }
            }).ToList();

            var keyframesPerFile = replayPayloads.Select(p => p.Where(pp => pp.Item2.Type == ReplayPayloadEntryType.Keyframe).Select(pp => pp.Item2.Data));
            var lmaoBlocksPerFile = keyframesPerFile.Select(k => k.Select(kk => LmaoParser.GetBlocksFromLmao(kk)));

            // List of files -> List of keyframes -> List of blocks

            var grouped = lmaoBlocksPerFile.Select(l => l.Select(b => GetRepetitionGroupations(b)).ToList()).ToList();
        }

        [TestMethod]
        public void TestRoflToLmaoFile()
        {
            var roflFile = TestResources.EUN1_2389388796;

            var lmaoFiles = PepeWriter.GetStringFromRofl(roflFile);
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
            List<List<List<(string mask, string time, string contentLen, string type, string blockparam, string content)>>> groupations = GetRepetitionGroupations(blocks);

            var arrayedGroupations = groupations.Select(g => g.ToArray()).ToArray();

            int inGroupations = arrayedGroupations.SelectMany(g => g).SelectMany(g => g).Count();
            Assert.AreEqual(blocks.Count(), inGroupations);
            Assert.IsTrue(arrayedGroupations.SelectMany(g => g.SelectMany(gg => gg)).All(g => blocks.Any(b => b == g)));

            //Assert.IsTrue(arrayedGroupations[0].Count() == 4 && arrayedGroupations[2].Count() == 97 && arrayedGroupations[27].Count() == 4 && arrayedGroupations[59].Count() == 23 && arrayedGroupations[140].Count() == 7);
        }

        private static List<List<List<(string mask, string time, string contentLen, string type, string blockparam, string content)>>> GetRepetitionGroupations(List<(string mask, string time, string contentLen, string type, string blockparam, string content)> blocks)
        {
            var groupations = new List<List<List<(string mask, string time, string contentLen, string type, string blockparam, string content)>>>();

            var typeCount = blocks.GroupBy(g => g.type).Select(g => (g.Key, g.Count())).OrderByDescending(g => g.Item2).ToList();

            IEnumerable<(string mask, string time, string contentLen, string type, string blockparam, string content)> next, rest;

            rest = blocks.ToList();

            Collect(groupations, typeCount, "6B00", "8B00", ref rest);
            Collect(groupations, typeCount, "1C02", "4B01", ref rest);
            Collect(groupations, typeCount, "A800", "7B01", ref rest);
            Collect(groupations, typeCount, "1701", "BB00", ref rest);
            //Collect(groupations, typeCount, "9400", "BB00", ref rest);
            Collect(groupations, typeCount, "5502", "7400", ref rest);
            Collect(groupations, typeCount, "2101", "9C01", ref rest);

            groupations.Add(new[] { rest.ToList() }.ToList());
            return groupations;
        }

        private static void Collect(
            List<List<List<(string mask, string time, string contentLen, string type, string blockparam, string content)>>> groupations, 
            List<(string Key, int)> typeCount, string from, string to, ref IEnumerable<(string mask, string time, string contentLen, string type, string blockparam, string content)> rest)
        {
            if (typeCount.Any(t => t.Key == from) == false || typeCount.Any(t => t.Key == to) == false) return;

            int fromCount = typeCount.First(t => t.Key == from).Item2;
            int toCount = typeCount.First(t => t.Key == to).Item2;

            int howMany = Math.Min(fromCount, toCount);
            var next = rest.TakeWhile(b => b.type != from);

            if (next.Count() > 0) groupations.Add(new[] { next.ToList() }.ToList());

            rest = rest.Skip(next.Count());
            rest = GroupIntoNextX(rest: rest, groupations: groupations, times: howMany, untilType: to);
        }

        private static IEnumerable<(string mask, string time, string contentLen, string type, string blockparam, string content)> GroupIntoNextX(
            IEnumerable<(string mask, string time, string contentLen, string type, string blockparam, string content)> rest,
            List<List<List<(string mask, string time, string contentLen, string type, string blockparam, string content)>>> groupations, int times, string untilType)
        {
            var group = new List<List<(string mask, string time, string contentLen, string type, string blockparam, string content)>>();

            for (int i = 0; i < times; i++)
            {
                var next = rest.TakeWhile(b => b.type != untilType).ToList();
                rest = rest.Skip(next.Count());
                next.Add(rest.First());
                rest = rest.Skip(1);

                group.Add(next);
            }

            groupations.Add(group);

            return rest;
        }

        [TestMethod]
        public void TestContent4()
        {
            string dirName = @"C:\Users\domin\Documents\League of Legends\Replays - Copy";
            Func<string, bool> filenameFilter = f => f.Contains("-3-Key") && !f.EndsWith(".bic");

            IEnumerable<(string f, byte[])> resources = Directory.GetFiles(dirName).Where(filenameFilter).Select(f => (f, File.ReadAllBytes(f)));

            var splitFile = new List<List<(string mask, string time, string contentLen, string type, string blockparam, string content)>>();

            foreach (byte[] res in resources.Select(r => r.Item2))
            {
                var blocks = LmaoParser.GetBlocksFromLmao(res);

                var byType = blocks.GroupBy(b => b.type).Select(g => (g.Key, g.ToArray())).OrderByDescending(g => g.Item2.Length).ToArray();

                var groupedByType = new List<(string, List<(string mask, string time, string contentLen, string type, string blockparam, string content)>)>();

                groupedByType.Add(("first", new List<(string mask, string time, string contentLen, string type, string blockparam, string content)>()));

                foreach (var b in blocks)
                {
                    if (groupedByType.Last().Item1 == b.type) groupedByType.Last().Item2.Add(b);
                    else groupedByType.Add((b.type, new List<(string mask, string time, string contentLen, string type, string blockparam, string content)> { b }));
                }

                var gbt = groupedByType.OrderByDescending(g => g.Item2.Count).Select(g => g.Item2.ToArray()).ToArray();

                List<List<(string, List<(string mask, string time, string contentLen, string type, string blockparam, string content)>)>> evenMoreGroupedByType =
                    new List<List<(string, List<(string mask, string time, string contentLen, string type, string blockparam, string content)>)>>();

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

            var distinct = all.GroupBy(k => k.mask + k.time + k.contentLen + k.type + k.blockparam + k.content).OrderByDescending(g => g.Count()).Select(g => g.ToArray()).ToArray();

            int total = resources.Count();
        }
    }
}
