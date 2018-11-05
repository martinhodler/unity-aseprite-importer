using Aseprite.PixelFormats;
using System.IO;


namespace Aseprite.Chunks
{
    public enum CelType : ushort
    {
        Raw = 0,
        Linked = 1,
        Compressed = 2
    }

    public class CelChunk : Chunk
    {
        public ushort LayerIndex { get; private set; }
        public short X { get; private set; }
        public short Y { get; private set; }
        public virtual ushort Width { get; protected set; }
        public virtual ushort Height { get; protected set; }
        public byte Opacity { get; set; }
        public CelType CelType { get; set; }

        public virtual Pixel[] RawPixelData { get; protected set; }


        public CelChunk(uint length, ushort layerIndex, short x, short y, byte opacity, CelType type) : base(length, ChunkType.Cel)
        {
            LayerIndex = layerIndex;
            X = x;
            Y = y;
            Opacity = opacity;
            CelType = type;
        }

        protected void ReadPixelData(BinaryReader reader, Frame frame)
        {
            int size = Width * Height;
            RawPixelData = new Pixel[size];

            switch (frame.File.Header.ColorDepth)
            {
                case ColorDepth.RGBA:
                    for (int i = 0; i < size; i++)
                    {
                        byte[] color = reader.ReadBytes(4);

                        RawPixelData[i] = new RGBAPixel(frame, color);
                    }
                    break;
                case ColorDepth.Grayscale:
                    for (int i = 0; i < size; i++)
                    {
                        byte[] color = reader.ReadBytes(2);

                        RawPixelData[i] = new GrayscalePixel(frame, color);
                    }
                    break;
                case ColorDepth.Indexed:
                    for (int i = 0; i < size; i++)
                    {
                        byte color = reader.ReadByte();

                        RawPixelData[i] = new IndexedPixel(frame, color);
                    }
                    break;
            }
        }



        public static CelChunk ReadCelChunk(uint length, BinaryReader reader, Frame frame)
        {
            ushort layerIndex = reader.ReadUInt16();
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();
            byte opacity = reader.ReadByte();
            CelType type = (CelType)reader.ReadUInt16();

            reader.ReadBytes(7); // For Future


            switch (type)
            {
                case CelType.Raw:
                    return new RawCelChunk(length, layerIndex, x, y, opacity, frame, reader);
                case CelType.Linked:
                    return new LinkedCelChunk(length, layerIndex, x, y, opacity, frame, reader);
                case CelType.Compressed:
                    return new CompressedCelChunk(length, layerIndex, x, y, opacity, frame, reader);
            }


            return null;
        }

    }
}
