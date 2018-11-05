using Aseprite.Chunks;
using System.IO;

namespace Aseprite
{
    public enum ChunkType : ushort
    {
        OldPalette = 0x0004,
        OldPalette2 = 0x0011,
        Layer = 0x2004,
        Cel = 0x2005,
        CelExtra = 0x2006,
        Mask = 0x2016, // DEPRECATED
        Path = 0x2017 , // NEVER USED
        FrameTags = 0x2018,
        Palette = 0x2019,
        UserData = 0x2020
    }

    public class Chunk
    {
        public const int HEADER_SIZE = 6;

        protected Frame Frame = null;
        public uint Length { get; private set; }
        public ChunkType ChunkType { get; private set; }

        public Chunk(uint length, ChunkType type)
        {
            Length = length;
            ChunkType = type;
        }

        public static Chunk ReadChunk(Frame frame, BinaryReader reader)
        {
            uint length = reader.ReadUInt32();
            ChunkType type = (ChunkType)reader.ReadUInt16();

            switch (type)
            {
                case ChunkType.Cel:
                    return CelChunk.ReadCelChunk(length, reader, frame);
                case ChunkType.CelExtra:
                    return new CelExtraChunk(length, reader) { Frame = frame };
                case ChunkType.Layer:
                    return new LayerChunk(length, reader) { Frame = frame };
                case ChunkType.FrameTags:
                    return new FrameTagsChunk(length, reader) { Frame = frame };
                case ChunkType.Palette:
                    return new PaletteChunk(length, reader) { Frame = frame };
            }

            reader.BaseStream.Position += length - Chunk.HEADER_SIZE;
            return null;
        }
    }
}
