using Aseprite.Chunks;
using System.Collections.Generic;
using System.IO;

namespace Aseprite
{
    public class Frame
    {
        public AseFile File = null;

        public uint Length { get; private set; }
        public ushort MagicNumber { get; private set; }
        public ushort ChunksCount { get; private set; }
        public ushort FrameDuration { get; private set; }

        public List<Chunk> Chunks { get; private set; }

        public Frame(AseFile file, BinaryReader reader)
        {
            File = file;

            Length = reader.ReadUInt32();
            MagicNumber = reader.ReadUInt16();

            ChunksCount = reader.ReadUInt16();
            FrameDuration = reader.ReadUInt16();

            reader.ReadBytes(6); // For Future

            Chunks = new List<Chunk>();

            for (int i = 0; i < ChunksCount; i++)
            {
                Chunk chunk = Chunk.ReadChunk(this, reader);

                if (chunk != null)
                    Chunks.Add(chunk);
            }
        }

        public T GetChunk<T>() where T : Chunk
        {
            for (int i = 0; i < Chunks.Count; i++)
            {
                if (Chunks[i] is T)
                {
                    return (T)Chunks[i];
                }
            }

            return null;
        }

        public T GetCelChunk<T>(int layerIndex) where T : CelChunk
        {
            for (int i = 0; i < Chunks.Count; i++)
            {
                if (Chunks[i] is T && (Chunks[i] as CelChunk).LayerIndex == layerIndex)
                {
                    return (T)Chunks[i];
                }
            }

            return null;
        }

        public List<T> GetChunks<T>() where T : Chunk
        {
            List<T> chunks = new List<T>();

            for (int i = 0; i < Chunks.Count; i++)
            {
                if (Chunks[i] is T)
                {
                    chunks.Add((T)Chunks[i]);
                }
            }

            return chunks;
        }
    }
}
