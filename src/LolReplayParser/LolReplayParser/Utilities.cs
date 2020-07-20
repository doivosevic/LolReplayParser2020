using System.Collections.Generic;
using System.Linq;

namespace LolReplayParser
{
    public static class Utilities
    {
        public static string SwapEndianness(string lenAsString)
        {
            List<string> grouped = lenAsString
                .Select((x, i) => (x, i))
                .GroupBy(x => x.i / 2)
                .Select(x => x.First().x.ToString() + x.Last().x)
                .ToList();

            grouped.Reverse();

            return string.Join("", grouped);
        }

        public static bool AreEqual(byte[] bytes1, byte[] bytes2)
        {
            if (bytes1.Length != bytes2.Length) return false;

            for (int i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i]) return false;
            }

            return true;
        }
    }
}
