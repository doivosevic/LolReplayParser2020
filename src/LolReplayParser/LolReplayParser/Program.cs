using LeagueReplayReader.Types;
using System;
using System.Diagnostics;
using System.IO;

namespace LolReplayParser
{
    class Program
    {
        static void Main(string[] args)
        {
            //Debugger.Launch();

            //if (args.Length != 2)
            //{
            //    Console.WriteLine("Args: <source> <dest>");
            //    return;
            //}

            //string source = args[0];
            //string destination = args[1];

            var currDir = Environment.CurrentDirectory;

            var files = Directory.GetFiles(currDir, "*.rofl");

            foreach (var file in files)
            {
                var source = file; 

                var lmaoFiles = PepeWriter.GetStringFromRofl(File.ReadAllBytes(file));
                //Debugger.Launch();

                for (int i = 0; i < lmaoFiles.Count; i++)
                {
                    string filename = file + i + ".lmao";
                    File.WriteAllText(filename, lmaoFiles[i].content);
                }
            }
        }

        private static void ParseAndDump(string sourceFilename)
        {
            if (!File.Exists(sourceFilename))
            {
                Console.WriteLine("Error: file not found: {0}", sourceFilename);
                return;
            }

            //if (!Directory.Exists(destination))
            //{
            //    Directory.CreateDirectory(destination);
            //}

            // init the replay file
            Replay replay = new Replay(sourceFilename);

            // handle the entries within the replay file
            while (replay.ReadEntry())
            {
                Console.WriteLine(replay.PayloadEntry);

                //if (replay.PayloadEntry.Type == ReplayPayloadEntryType.Keyframe)
                {
                    // write the payload out to disk
                    File.WriteAllBytes(string.Format(@"{0}-{1}-{2}.lmao", replay.PayloadHeader.GameId, replay.PayloadEntry.ID, replay.PayloadEntry.Type), replay.PayloadEntry.Data);

                    //return;
                }
            }
        }
    }
}
