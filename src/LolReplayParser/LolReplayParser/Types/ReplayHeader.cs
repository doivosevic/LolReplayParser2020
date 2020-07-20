using System.IO;
using System.Text;

namespace LeagueReplayReader.Types
{
    public class ReplayHeader
    {
        public int PayloadOffset { get; }

        public byte[] Magic { get; }

        public byte[] Signature { get; }

        public int MetadataOffset { get; }

        public int MetadataLength { get; }

        public ReplayMetadata Metadata { get; }

        private short m_headerLength;
        private int m_fileLength;
        private int m_payloadHeaderOffset;
        private int m_payloadHeaderLength;
        private ReplayMetadata m_metadata;

        public ReplayHeader(Stream p_stream)
        {
            using (BinaryReader r = new BinaryReader(p_stream, Encoding.UTF8, true))
            {
                // the magic byte identifiers
                Magic = r.ReadBytes(6);

                // file hash
                Signature = r.ReadBytes(256);

                // various lengths and offsets
                m_headerLength = r.ReadInt16();
                m_fileLength = r.ReadInt32();
                MetadataOffset = r.ReadInt32();
                MetadataLength = r.ReadInt32();
                m_payloadHeaderOffset = r.ReadInt32();
                m_payloadHeaderLength = r.ReadInt32();
                PayloadOffset = r.ReadInt32();

                // json metadata
                m_metadata = ReplayMetadata.Deserialize(r.ReadBytes(MetadataLength));
            }
        }

        public override string ToString()
        {
            return string.Format("<ReplayHeader mo={0} ml={1} pho={2} phl={3} po={4}>", MetadataOffset, MetadataLength, m_payloadHeaderOffset, m_payloadHeaderLength, PayloadOffset);
        }
    }
}