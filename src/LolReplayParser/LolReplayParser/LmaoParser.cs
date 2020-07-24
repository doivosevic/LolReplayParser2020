using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LolReplayParser
{
    public static class LmaoParser
    {
        public static List<Block> GetBlocksFromLmao(byte[] inputLmao)
        {
            string hexed = BitConverter.ToString(inputLmao).Replace("-", "");
            int fullLen = hexed.Length;

            int startIndex = 34;

            var blocks = new List<(string mask, string time, string contentLen, string type, string blockparam, string content)>();

            while (startIndex < hexed.Length)
            {
                var mask = hexed[startIndex].ToString() + hexed[startIndex + 1];
                string maskBits = Convert.ToString(Convert.ToInt32(mask, 16), 2).PadLeft(8, '0');

                bool timeSmall = maskBits[0] == '1';
                bool includeTypeByte = maskBits[1] == '0';
                bool blockparamSmall = maskBits[2] == '1';
                bool contentLenSmall = maskBits[3] == '1';

                startIndex += 2;

                string timeStr = hexed.Substring(startIndex, timeSmall ? 2 : 8);
                startIndex += timeSmall ? 2 : 8;

                string contentLenStr = hexed.Substring(startIndex, contentLenSmall ? 2 : 8);
                startIndex += contentLenSmall ? 2 : 8;

                string typeStr = hexed.Substring(startIndex, includeTypeByte ? 4 : 0);
                startIndex += includeTypeByte ? 4 : 0;

                string blockParamStr = hexed.Substring(startIndex, blockparamSmall ? 2 : 8);
                startIndex += blockparamSmall ? 2 : 8;

                string swapEndianness = Utilities.SwapEndianness(contentLenStr);
                int contentLen = int.Parse(swapEndianness, NumberStyles.HexNumber);

                string content = hexed.Substring(startIndex, contentLen * 2);
                startIndex += contentLen * 2;

                blocks.Add((mask, timeStr, contentLenStr, typeStr, blockParamStr, content));
            }

            return blocks.Select(b => new Block(b.mask, b.time, b.contentLen, b.type, b.blockparam, b.content)).ToList();
        }
    }
}
