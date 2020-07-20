using System;
using System.IO;
using System.Text;

namespace LeagueReplayReader.Types
{
    public class ReplayPayloadHeader
    {

        public byte[] EncryptionKey { get; }

        public long GameId { get; }

        public int GameLength { get; }

        public int KeyframeCount { get; }

        public int ChunkCount { get; }

        private int m_endStartupChunkId;
        private int m_startGameChunkId;
        private int m_keyframeInterval;
        private short m_encryptionKeyLength;

        public ReplayPayloadHeader(Stream p_stream)
        {
            using (BinaryReader r = new BinaryReader(p_stream, Encoding.UTF8, true))
            {
                this.GameId = r.ReadInt64();
                this.GameLength= r.ReadInt32();
                this.KeyframeCount = r.ReadInt32();
                this.ChunkCount = r.ReadInt32();
                m_endStartupChunkId = r.ReadInt32();
                m_startGameChunkId = r.ReadInt32();
                m_keyframeInterval = r.ReadInt32();
                m_encryptionKeyLength = r.ReadInt16();
                this.EncryptionKey = Convert.FromBase64String(Encoding.UTF8.GetString(r.ReadBytes(m_encryptionKeyLength)));
            }
        }

        public override string ToString()
        {
            return string.Format("<ReplayPayloadHeader gameId={0} gameLen={1} keyframes={2} chunks={3}>", GameId, GameLength, KeyframeCount, ChunkCount);
        }
    }
}