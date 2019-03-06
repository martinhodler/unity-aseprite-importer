using System.IO;

namespace Aseprite.Chunks
{
    public class CelExtraChunk : Chunk
    {
        public uint Flags { get; private set; }
        public double PreciseX { get; private set; }
        public double PreciseY { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }

        public CelExtraChunk(uint length, BinaryReader reader) : base(length, ChunkType.CelExtra)
        {
            Flags = reader.ReadUInt32();
            PreciseX = reader.ReadDouble();
            PreciseY = reader.ReadDouble();
            Width = reader.ReadDouble();
            Height = reader.ReadDouble();

            reader.ReadBytes(16); // For Future
        }
    }
}
