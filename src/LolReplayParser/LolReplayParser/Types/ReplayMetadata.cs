using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace LeagueReplayReader.Types
{
    [DataContract]
    public class ReplayMetadata
    {
        [DataMember]
        public int gameId { get; set; }

        public override string ToString()
        {
            return string.Format("<ReplayMetadata gid={0}>", gameId);
        }

        public static ReplayMetadata Deserialize(byte[] p_json)
        {
            using (MemoryStream m = new MemoryStream(p_json))
            {
                DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(ReplayMetadata));
                return (ReplayMetadata)s.ReadObject(m);
            }
        }
    }
}