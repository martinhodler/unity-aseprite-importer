using System.IO;

namespace Aseprite.Chunks
{
    public class RawCelChunk : CelChunk
    {

        public RawCelChunk(uint length, ushort layerIndex, short x, short y, byte opacity, Frame frame, BinaryReader reader) : base(length, layerIndex, x, y, opacity, CelType.Raw)
        {
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();

            ReadPixelData(reader, frame);
        }
    }
}
