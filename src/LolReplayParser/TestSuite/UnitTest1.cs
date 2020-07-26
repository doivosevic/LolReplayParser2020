using LeagueReplayReader.Types;
using LolReplayParser;
using LeagueReplayParser;
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

                var replay = new LeagueReplayReader.Types.Replay(roflStream);
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

            var groupations = Stuff.GetRepetitionGroupations(blocks);
            var groupations2 = Stuff.GetRepetitionGroupations(key1Blocks);

            var flattened = groupations.SelectMany(g => g.Item4).SelectMany(g => g).ToList();
            int flattenedCount = flattened.Sum(b => b.Count);

            Assert.AreEqual(blocks.Count(), flattenedCount);

            //Assert.IsTrue(arrayedGroupations.SelectMany(g => g.SelectMany(gg => gg)).All(g => blocks.Any(b => b == g)));
            //Assert.IsTrue(arrayedGroupations[0].Count() == 4 && arrayedGroupations[2].Count() == 97 && arrayedGroupations[27].Count() == 4 && arrayedGroupations[59].Count() == 23 && arrayedGroupations[140].Count() == 7);
        }

        [TestMethod]
        public void TestContent123()
        {
            var pattern = new[] { ("F901", "BE00"), ("6B00", "8B00"), ("1C02", "8601"), ("A800", "7B01"), ("1701", "A200"),
                                  ("5502", "7400"), ("F000", "0A02"), ("2101", "2F02"), ("2101", "9C01") };

            ValidatePatternForAllFilesMatchingFilePattern("-1-Key", pattern);

            ValidatePatternForAllFilesMatchingFilePattern("-2-Key", pattern);

            var correct = new[] { 0, 1, 2, 3, 4, 5, 7, 8, 11 };
        }

        private static void ValidatePatternForAllFilesMatchingFilePattern(string filePattern, (string, string)[] pattern, int[] correct = null)
        {
            string dirName = @"C:\Users\domin\Documents\League of Legends\ReplaysNewer";

            Func<string, bool> filenameFilter = f => f.Contains(filePattern) && !f.EndsWith(".bic");

            List<(string f, byte[])> resources = Directory.GetFiles(dirName).Where(filenameFilter).Select(f => (f, File.ReadAllBytes(f))).ToList();

            var sample = resources[2];
            var g = Stuff.GetRepetitionGroupations(LmaoParser.GetBlocksFromLmao(sample.Item2));

            var samples = resources.Select(r => Stuff.GetRepetitionGroupations(LmaoParser.GetBlocksFromLmao(r.Item2))).ToList();

            var all = Enumerable.Range(0, resources.Count).ToArray();

            correct = correct ?? all;

            foreach (var i in correct.Concat(all))
            {
                var res = resources[i];
                var grouped = Stuff.GetRepetitionGroupations(LmaoParser.GetBlocksFromLmao(res.Item2));

                for (int j = 0; j < pattern.Length; j++)
                {
                    bool isCorrect = pattern[j].Item1 == grouped[j].Item1 && pattern[j].Item2 == grouped[j].Item2;
                    if (!isCorrect && correct.Contains(i)) Assert.IsTrue(isCorrect, i.ToString() + " " + j);
                    else if (!isCorrect)
                    {

                    }
                }
            }
        }

        [TestMethod]
        public void TestContent1234()
        {
            var pattern = new[] { ("F901", "BE00"), ("6B00", "8B00"), ("1C02", "8601"), ("A800", "7B01"), ("1701", "A200"),
                                  ("5502", "7400"), ("F000", "0A02"), ("2101", "2F02"), ("2101", "9C01") };

            ValidatePattern(pattern);
        }

        private static void ValidatePattern((string, string)[] pattern, int[] correct = null)
        {
            string dirName = @"C:\Users\domin\Documents\League of Legends\ReplaysNewer";

            Func<string, bool> filenameFilter = f => f.EndsWith(".rofl");

            var filenames = Directory.GetFiles(dirName).Where(filenameFilter);
            List<(string f, byte[])> resources = filenames.Select(f => (f, File.ReadAllBytes(f))).ToList();
            var asLmaos = resources.Select(r => LmaoParser.GetFullPayloadsFromRofl(r.Item2, 1, true)).ToList();
            var asGrouped = asLmaos.Select(l => Stuff.GetRepetitionGroupations(LmaoParser.GetBlocksFromLmao(l.First().Item2.Data))).ToList();

            var filesAsSplitLines = filenames.Select(f => File.ReadLines(f));
            var statReplays = filesAsSplitLines.Select(r => LeagueReplayParser.Parser.FillReplay(new LeagueReplayParser.Replay(null), r)).ToList();

            var sample = resources[2];
            var g = Stuff.GetRepetitionGroupations(LmaoParser.GetBlocksFromLmao(sample.Item2));

            var samples = resources.Select(r => Stuff.GetRepetitionGroupations(LmaoParser.GetBlocksFromLmao(r.Item2))).ToList();

            var all = Enumerable.Range(0, resources.Count).ToArray();

            correct = correct ?? all;

            foreach (var i in correct.Concat(all))
            {
                for (int j = 0; j < pattern.Length; j++)
                {
                    var grouped = asGrouped[i];
                    bool isCorrect = pattern[j].Item1 == grouped[j].Item1 && pattern[j].Item2 == grouped[j].Item2;
                    if (!isCorrect && correct.Contains(i)) Assert.IsTrue(isCorrect, i.ToString() + " " + j);
                    else if (!isCorrect)
                    {

                    }
                }
            }
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
