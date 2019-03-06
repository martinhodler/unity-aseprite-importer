using System.IO;


namespace Aseprite
{
    public enum ColorDepth : ushort
    {
        RGBA = 32,
        Grayscale = 16,
        Indexed = 8
    }

    public class Header
    {

        public uint FileSize { get; private set; }
        public ushort MagicNumber { get; private set; }
        public ushort Frames { get; private set; }
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public ColorDepth ColorDepth { get; private set; }
        public uint Flags { get; private set; }
        public ushort Speed { get; private set; }

        public byte TransparentIndex { get; private set; }

        public ushort ColorCount { get; private set; }
        public byte PixelWidth { get; private set; }
        public byte PixelHeight { get; private set; }

        public Header(byte[] header)
        {
            if (header.Length != 128)
                return;

            Stream stream = new MemoryStream(header);
            BinaryReader reader = new BinaryReader(stream);

            FileSize = reader.ReadUInt32();         // File size
            MagicNumber = reader.ReadUInt16();      // Magic number (0xA5E0)
            Frames = reader.ReadUInt16();           // Frames
            Width = reader.ReadUInt16();            // Width in pixels
            Height = reader.ReadUInt16();           // Height in pixels
            ColorDepth = (ColorDepth)reader.ReadUInt16();       // Color depth (bits per pixel) [32 bpp = RGBA, 16 bpp = Grayscale, 8 bpp Indexed]
            Flags = reader.ReadUInt32();            // Flags: 1 = Layer opacity has valid value
            Speed = reader.ReadUInt16();            // Speed (milliseconds between frame, like in FLC files) DEPRECATED: You should use the frame duration field from each frame header

            reader.ReadUInt32();                    // Set be 0
            reader.ReadUInt32();                    // Set be 0

            TransparentIndex = reader.ReadByte();   // Palette entry (index) which represent transparent color in all non-background layers (only for Indexed sprites)

            reader.ReadBytes(3);                    // Ignore these bytes

            ColorCount = reader.ReadUInt16();       // Number of colors (0 means 256 for old sprites)
            PixelWidth = reader.ReadByte();         // Pixel width (pixel ratio is "pixel width/pixel height"). If pixel height field is zero, pixel ratio is 1:1
            PixelHeight = reader.ReadByte();        // Pixel height

            reader.ReadBytes(92);                   // For future
        }

    }
}
