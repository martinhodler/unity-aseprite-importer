using System.IO;
using System.Text;

namespace Aseprite.Chunks
{
    public enum LayerType : ushort
    {
        Normal = 0,
        Group = 1
    }

    public enum LayerBlendMode : ushort
    {
        Normal = 0,
        Multiply = 1,
        Screen = 2,
        Overlay = 3,
        Darken = 4,
        Lighten = 5,
        ColorDodge = 6,
        ColorBurn = 7,
        HardLight = 8,
        SoftLight = 9,
        Difference = 10,
        Exclusion = 11,
        Hue = 12,
        Saturation = 13,
        Color = 14,
        Luminosity = 15,
        Addition = 16,
        Subtract = 17,
        Divide = 18
    }

    public class LayerChunk : Chunk
    {
        public ushort Flags { get; private set; }
        public LayerType LayerType { get; private set; }
        public ushort LayerChildLevel { get; private set; }
        public ushort DefaultLayerWidth { get; private set; } // Ignored
        public ushort DefaultLayerHeight { get; private set; } // Ignored
        public LayerBlendMode BlendMode { get; private set; }
        public byte Opacity { get; private set; }

        public string LayerName { get; private set; }

        public bool Visible
        {
            get { return Flags % 2 == 1; }
        }

        public LayerChunk(uint length, BinaryReader reader) : base(length, ChunkType.Layer)
        {
            Flags = reader.ReadUInt16();
            LayerType = (LayerType)reader.ReadUInt16();
            LayerChildLevel = reader.ReadUInt16();

            DefaultLayerWidth = reader.ReadUInt16(); // Ignored
            DefaultLayerHeight = reader.ReadUInt16(); // Ignored

            BlendMode = (LayerBlendMode)reader.ReadUInt16();
            Opacity = reader.ReadByte();

            reader.ReadBytes(3); // For future

            ushort nameLength = reader.ReadUInt16();
            ///int nameLength = (int)(length - 18) - Chunk.HEADER_SIZE;
            LayerName = Encoding.Default.GetString(reader.ReadBytes(nameLength));
        }
    }
}
