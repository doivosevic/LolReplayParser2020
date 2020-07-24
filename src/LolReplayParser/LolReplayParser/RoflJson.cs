using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LolReplayParser
{
    public static class RoflJson
    {
        /// <summary>
        /// Gets the JSON part of the replay file contents.
        /// </summary>
        public static JObject GetJSON(string replayFileContents)
        {
            int jsonStartIndex = replayFileContents.IndexOf("{\"gameLength\"");
            int jsonEndIndex = replayFileContents.IndexOf("\\\"}]\"}") + "\\\"}]\"}".Length;

            try
            {
                return JObject.Parse(replayFileContents.Substring(jsonStartIndex, (jsonEndIndex - jsonStartIndex)));
            }
            catch (JsonReaderException jre)
            {
                throw new Exception("Unable to parse replay data from this replay file.", jre);
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error has occured while trying to parse replay data.", ex);
            }
        }
    }
}
