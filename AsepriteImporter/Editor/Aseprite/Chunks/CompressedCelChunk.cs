using System.IO;
using System.IO.Compression;

namespace Aseprite.Chunks
{
    public class CompressedCelChunk : CelChunk
    {

        public byte[] CompressedRawCell { get; private set; }

        public CompressedCelChunk(uint length, ushort layerIndex, short x, short y, byte opacity, Frame frame, BinaryReader reader) : base(length, layerIndex, x, y, opacity, CelType.Compressed)
        {

            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();

            reader.ReadBytes(2);
            int compressedDataSize = (int)(length - 22) - Chunk.HEADER_SIZE;

            CompressedRawCell = reader.ReadBytes(compressedDataSize);

            byte[] buffer = new byte[1024];

            MemoryStream uncompressed = new MemoryStream();

            using (MemoryStream s = new MemoryStream(CompressedRawCell))
            {
                using (DeflateStream gzip = new DeflateStream(s, CompressionMode.Decompress))
                {

                    int len = 0;
                    do
                    {
                        len = gzip.Read(buffer, 0, buffer.Length);

                        if (len > 0)
                        {
                            uncompressed.Write(buffer, 0, len);
                        }

                    } while (len > 0);

                }
            }


            uncompressed.Position = 0;
            BinaryReader ureader = new BinaryReader(uncompressed);

            ReadPixelData(ureader, frame);
        }
    }
}
