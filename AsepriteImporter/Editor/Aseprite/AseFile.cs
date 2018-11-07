using Aseprite.Chunks;
using Aseprite.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Aseprite
{

    // See: https://github.com/aseprite/aseprite/blob/master/docs/ase-file-specs.md

    public class AseFile
    {
        public Header Header { get; private set; }
        public List<Frame> Frames { get; private set; }

        public AseFile(Stream stream)
        {
            BinaryReader reader = new BinaryReader(stream);
            byte[] header = reader.ReadBytes(128);

            Header = new Header(header);
            Frames = new List<Frame>();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Frames.Add(new Frame(this, reader));
            }
        }


        public List<T> GetChunks<T>() where T : Chunk
        {
            List<T> chunks = new List<T>();

            for (int i = 0; i < this.Frames.Count; i++)
            {
                List<T> cs = this.Frames[i].GetChunks<T>();

                chunks.AddRange(cs);
            }

            return chunks;
        }

        public Texture2D[] GetFrames()
        {
            List<Texture2D> frames = new List<Texture2D>();

            for (int i = 0; i < Frames.Count; i++)
            {
                frames.Add(GetFrame(i));
            }

            return frames.ToArray();
        }

        public Texture2D GetFrame(int index)
        {
            Frame frame = Frames[index];


            Texture2D texture = Texture2DUtil.CreateTransparentTexture(Header.Width, Header.Height);

            
            List<LayerChunk> layers = GetChunks<LayerChunk>();
            List<CelChunk> cels = frame.GetChunks<CelChunk>();

            cels.Sort((ca, cb) => ca.LayerIndex.CompareTo(cb.LayerIndex));

            for (int i = 0; i < cels.Count; i++)
            {
                LayerBlendMode blendMode = layers[cels[i].LayerIndex].BlendMode;
                float opacity = layers[cels[i].LayerIndex].Opacity / 255f;

                Texture2D celTex = GetTextureFromCel(cels[i]);
                
                switch (blendMode)
                {
                    case LayerBlendMode.Normal: texture = Texture2DBlender.Normal(texture, celTex); break;
                    case LayerBlendMode.Multiply: texture = Texture2DBlender.Multiply(texture, celTex, opacity); break;
                    case LayerBlendMode.Screen: texture = Texture2DBlender.Screen(texture, celTex); break;
                    case LayerBlendMode.Overlay: texture = Texture2DBlender.Overlay(texture, celTex); break;
                    case LayerBlendMode.Darken: texture = Texture2DBlender.Darken(texture, celTex); break;
                    case LayerBlendMode.Lighten: texture = Texture2DBlender.Lighten(texture, celTex); break;
                    case LayerBlendMode.ColorDodge: texture = Texture2DBlender.ColorDodge(texture, celTex); break;
                    case LayerBlendMode.ColorBurn: texture = Texture2DBlender.ColorBurn(texture, celTex); break;
                    case LayerBlendMode.HardLight: texture = Texture2DBlender.HardLight(texture, celTex); break;
                    case LayerBlendMode.SoftLight: texture = Texture2DBlender.SoftLight(texture, celTex); break;
                    case LayerBlendMode.Difference: texture = Texture2DBlender.Difference(texture, celTex); break;
                    case LayerBlendMode.Exclusion: texture = Texture2DBlender.Exclusion(texture, celTex); break;
                    case LayerBlendMode.Hue: texture = Texture2DBlender.Hue(texture, celTex); break;
                    case LayerBlendMode.Saturation: texture = Texture2DBlender.Saturation(texture, celTex); break;
                    case LayerBlendMode.Color: texture = Texture2DBlender.Color(texture, celTex); break;
                    case LayerBlendMode.Luminosity: texture = Texture2DBlender.Luminosity(texture, celTex); break;
                    case LayerBlendMode.Addition: texture = Texture2DBlender.Addition(texture, celTex); break;
                    case LayerBlendMode.Subtract: texture = Texture2DBlender.Subtract(texture, celTex); break;
                    case LayerBlendMode.Divide: texture = Texture2DBlender.Divide(texture, celTex); break;
                }
            }

            

            return texture;
        }

        public Texture2D GetTextureFromCel(CelChunk cel)
        {
            Texture2D texture = Texture2DUtil.CreateTransparentTexture(Header.Width, Header.Height);

            int i = 0;
            int x = 0;
            int y = 0;
            int index = 0;

            int width = Mathf.Min(cel.Width, Header.Width);
            int height = Mathf.Min(cel.Height, Header.Height);

            Color[] colors = new Color[width * height];

            for (y = 0; y < height; y++)
            {
                for (x = 0; x < width; x++)
                {
                    i = y * cel.Width + x;
                    index = (height - (y + 1)) * width + x;


                    colors[index] = cel.RawPixelData[i].GetColor();
                }
            }

            texture.SetPixels(cel.X, Header.Height - cel.Y - Mathf.Min(cel.Height, Header.Height), Mathf.Min(cel.Width, Header.Width), Mathf.Min(cel.Height, Header.Height), colors);
            texture.Apply();

            return texture;
        }

        public FrameTag[] GetAnimations()
        {
            List<FrameTagsChunk> tagChunks = this.GetChunks<FrameTagsChunk>();

            List<FrameTag> animations = new List<FrameTag>();

            foreach (FrameTagsChunk tagChunk in tagChunks)
            {
                foreach (FrameTag tag in tagChunk.Tags)
                {
                    animations.Add(tag);
                }
            }

            return animations.ToArray();
        }

        public Texture2D GetTextureAtlas()
        {
            Texture2D[] frames = this.GetFrames();

            Texture2D atlas = Texture2DUtil.CreateTransparentTexture(Header.Width * frames.Length, Header.Height);
            List<Rect> spriteRects = new List<Rect>();

            int col = 0;
            int row = 0;

            foreach (Texture2D frame in frames)
            {
                Rect spriteRect = new Rect(col * Header.Width, atlas.height - ((row + 1) * Header.Height), Header.Width, Header.Height);
                atlas.SetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height, frame.GetPixels());
                atlas.Apply();

                spriteRects.Add(spriteRect);

                col++;
            }

            return atlas;
        }
    }

}
