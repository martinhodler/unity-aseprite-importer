using System.IO;

namespace Aseprite.Chunks
{
    public class LinkedCelChunk : CelChunk
    {
        private AseFile file = null;
        private CelChunk linkedCelChunk = null;


        public CelChunk LinkedCel
        {
            get
            {
                if (linkedCelChunk == null)
                {
                    linkedCelChunk = file.Frames[FramePosition].GetCelChunk<CelChunk>(LayerIndex);
                }

                return linkedCelChunk;
            }
        }

        public ushort FramePosition { get; private set; }



        public override ushort Width { get { return LinkedCel.Width; } }
        public override ushort Height { get { return LinkedCel.Height; } }
        public override Pixel[] RawPixelData { get { return LinkedCel.RawPixelData; } }


        public LinkedCelChunk(uint length, ushort layerIndex, short x, short y, byte opacity, Frame frame, BinaryReader reader) : base(length, layerIndex, x, y, opacity, CelType.Linked)
        {
            file = frame.File;

            FramePosition = reader.ReadUInt16();
        }
    }
}
