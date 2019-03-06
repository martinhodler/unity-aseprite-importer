using UnityEngine;

namespace Aseprite.PixelFormats
{
    public class GrayscalePixel : Pixel
    {
        public byte[] Color { get; private set; }

        public GrayscalePixel(Frame frame, byte[] color) : base(frame)
        {
            Color = color;
        }

        public override Color GetColor()
        {
            float value = (float)Color[0] / 255;
            float alpha = (float)Color[1] / 255;

            return new Color(value, value, value, alpha);
        }
    }
}
