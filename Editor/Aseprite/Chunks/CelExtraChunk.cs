using System.IO;

namespace Aseprite.Chunks
{
    public class CelExtraChunk : Chunk
    {
        public uint Flags { get; private set; }
        public float PreciseX { get; private set; }
        public float PreciseY { get; private set; }
        public float Width { get; private set; }
        public float Height { get; private set; }

        public CelExtraChunk(uint length, BinaryReader reader) : base(length, ChunkType.CelExtra)
        {
            Flags = reader.ReadUInt32();
            PreciseX = reader.ReadSingle();
            PreciseY = reader.ReadSingle();
            Width = reader.ReadSingle();
            Height = reader.ReadSingle();

            reader.ReadBytes(16); // For Future
        }
    }
}
