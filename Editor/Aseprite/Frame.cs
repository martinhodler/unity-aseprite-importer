using Aseprite.Chunks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Aseprite
{
    public class Frame
    {
        public AseFile File = null;

        public uint Length { get; private set; }
        public ushort MagicNumber { get; private set; }
        
        public ushort OldChunksCount { get; private set; }
        public uint ChunksCount { get; private set; }
        public ushort FrameDuration { get; private set; }

        public List<Chunk> Chunks { get; private set; }
        
        private bool useNewChunkCount = true;
        
        public uint GetChunkCount()
        {
            if (useNewChunkCount)
                return ChunksCount;
            else
                return OldChunksCount;
        }

        public Frame(AseFile file, BinaryReader reader)
        {
            File = file;

            Length = reader.ReadUInt32();
            MagicNumber = reader.ReadUInt16();

            OldChunksCount = reader.ReadUInt16();
            FrameDuration = reader.ReadUInt16();

            reader.ReadBytes(2); // For Future
            
            ChunksCount = reader.ReadUInt32();
            if (ChunksCount == 0)
                useNewChunkCount = false;

            Chunks = new List<Chunk>();

            for (int i = 0; i < GetChunkCount(); i++)
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
