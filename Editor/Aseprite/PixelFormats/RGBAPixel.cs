using UnityEngine;

namespace Aseprite.PixelFormats
{
    public class RGBAPixel : Pixel
    {
        public byte[] Color { get; private set; }

        public RGBAPixel(Frame frame, byte[] color) : base(frame)
        {
            Color = color;
        }

        public override Color GetColor()
        {
            if (Color.Length == 4)
            {
                float red = (float)Color[0] / 255f;
                float green = (float)Color[1] / 255f;
                float blue = (float)Color[2] / 255f;
                float alpha = (float)Color[3] / 255f;

                return new Color(red, green, blue, alpha);
            }
            else
            {
                return UnityEngine.Color.magenta;
            }
        }
    }
}
