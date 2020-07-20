using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace LeagueReplayReader.Types
{
    public enum ReplayPayloadEntryType
    {
        Chunk,
        Keyframe,
        Unknown
    }

    public class ReplayPayloadEntry
    {
        public byte[] Data { get; }

        public int ID { get; }

        public ReplayPayloadEntryType Type
        {
            get
            {
                if (m_type == 1)
                {
                    return ReplayPayloadEntryType.Chunk;
                }
                else if (m_type == 2)
                {
                    return ReplayPayloadEntryType.Keyframe;
                }

                return ReplayPayloadEntryType.Unknown;
            }
        }

        public int Length { get; }

        public int Offset { get; }

        public int NextChunkID { get; }
        private int m_nextChunkId;
        private byte m_type;

        public ReplayPayloadEntry(Replay p_replay, Stream p_stream, int p_payloadDataStartOffset)
        {
            using (BinaryReader r = new BinaryReader(p_stream, Encoding.UTF8, true))
            {
                this.ID = r.ReadInt32();
                m_type = r.ReadByte();
                this.Length = r.ReadInt32();
                m_nextChunkId = r.ReadInt32();
                this.Offset = r.ReadInt32();
            }

            // seek to the entry's data location
            p_stream.Seek(p_payloadDataStartOffset + Offset, SeekOrigin.Begin);

            // init the byte array to appropriate length
            this.Data = new byte[Length];

            // the entry data chunk
            p_stream.Read(Data, 0, Length);

            // store the decrypted data
            Data = GetDecryptedData(p_replay, Data);
        }

        private byte[] GetDecryptedData(Replay p_replay, byte[] p_data)
        {
            // string represenation of the game id
            string gameId = Convert.ToString(p_replay.PayloadHeader.GameId);

            // obtaining the chunk encryption key
            byte[] chunkEncryptionKey = DepadBytes(DecryptBytes(Encoding.UTF8.GetBytes(gameId), p_replay.PayloadHeader.EncryptionKey));

            // obtaining the decrypted chunk
            byte[] decryptedChunk = DepadBytes(DecryptBytes(chunkEncryptionKey, p_data));

            return DecompressBytes(decryptedChunk);
        }

        /// <summary>
        /// http://tools.ietf.org/html/rfc2898
        /// </summary>
        private byte[] DepadBytes(byte[] p_data)
        {
            int paddingLength = Convert.ToInt32(p_data[p_data.Length - 1]);

            return p_data.Take(p_data.Length - paddingLength).ToArray();
        }

        private byte[] DecryptBytes(byte[] p_key, byte[] p_data)
        {
            BufferedBlockCipher cipher = new BufferedBlockCipher(new BlowfishEngine());

            cipher.Init(false, new KeyParameter(p_key));

            return cipher.ProcessBytes(p_data);
        }

        private byte[] DecompressBytes(byte[] p_data)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(p_data), CompressionMode.Decompress))
            {
                const int size = 4096;
                byte[] buffer = new byte[size];

                using (MemoryStream memory = new MemoryStream())
                {
                    int count = 0;

                    do
                    {
                        count = stream.Read(buffer, 0, size);

                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);

                    return memory.ToArray();
                }
            }
        }

        public override string ToString()
        {
            return string.Format("<ReplayPayloadEntry id={0} type={1} len={2} next={3}>", ID, Type, Length, m_nextChunkId);
        }
    }
}