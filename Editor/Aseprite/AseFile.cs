using Aseprite.Chunks;
using Aseprite.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Aseprite
{

    // See file specs here: https://github.com/aseprite/aseprite/blob/master/docs/ase-file-specs.md

    public class AseFile
    {
        public Header Header { get; private set; }
        public List<Frame> Frames { get; private set; }

        private Dictionary<Type, Chunk> chunkCache = new Dictionary<Type, Chunk>();

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

        public T GetChunk<T>() where T : Chunk
        {
            if (!chunkCache.ContainsKey(typeof(T)))
            {
                for (int i = 0; i < this.Frames.Count; i++)
                {
                    List<T> cs = this.Frames[i].GetChunks<T>();

                    if (cs.Count > 0)
                    {
                        chunkCache.Add(typeof(T), cs[0]);
                        break;
                    }
                }
            }

            return (T)chunkCache[typeof(T)];
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


        public Texture2D[] GetLayersAsFrames()
        {
            List<Texture2D> frames = new List<Texture2D>();
            List<LayerChunk> layers = GetChunks<LayerChunk>();

            for (int i = 0; i < layers.Count; i++)
            {
                List<Texture2D> layerFrames = GetLayerTexture(i, layers[i]);

                if (layerFrames.Count > 0)
                    frames.AddRange(layerFrames);
            }

            return frames.ToArray();
        }

        private LayerChunk GetParentLayer(LayerChunk layer)
        {
            if (layer.LayerChildLevel == 0)
                return null;

            int childLevel = layer.LayerChildLevel;

            List<LayerChunk> layers = GetChunks<LayerChunk>();
            int index = layers.IndexOf(layer);

            if (index < 0)
                return null;

            for (int i = index -1; i > 0; i--)
            {
                if (layers[i].LayerChildLevel == layer.LayerChildLevel - 1)
                    return layers[i];
            }

            return null;
        }

        public List<Texture2D> GetLayerTexture(int layerIndex, LayerChunk layer)
        {

            List<LayerChunk> layers = GetChunks<LayerChunk>();
            List<Texture2D> textures = new List<Texture2D>();

            for (int frameIndex = 0; frameIndex < Frames.Count; frameIndex++)
            {
                Frame frame = Frames[frameIndex];
                List<CelChunk> cels = frame.GetChunks<CelChunk>();

                for (int i = 0; i < cels.Count; i++)
                {
                    if (cels[i].LayerIndex != layerIndex)
                        continue;

                    LayerBlendMode blendMode = layer.BlendMode;
                    float opacity = Mathf.Min(layer.Opacity / 255f, cels[i].Opacity / 255f);

                    bool visibility = layer.Visible;

                    LayerChunk parent = GetParentLayer(layer);
                    while (parent != null)
                    {
                        visibility &= parent.Visible;
                        if (visibility == false)
                            break;

                        parent = GetParentLayer(parent);
                    }

                    if (visibility == false || layer.LayerType == LayerType.Group)
                        continue;

                    textures.Add(GetTextureFromCel(cels[i]));
                }
            }

            return textures;
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
                LayerChunk layer = layers[cels[i].LayerIndex];
                if (layer.LayerName.StartsWith("@")) //ignore metadata layer
                    continue;

                LayerBlendMode blendMode = layer.BlendMode;
                float opacity = Mathf.Min(layer.Opacity / 255f, cels[i].Opacity / 255f);

                bool visibility = layer.Visible;


                LayerChunk parent = GetParentLayer(layer);
                while (parent != null)
                {
                    visibility &= parent.Visible;
                    if (visibility == false)
                        break;

                    parent = GetParentLayer(parent);
                }

                if (visibility == false || layer.LayerType == LayerType.Group)
                    continue;

                Texture2D celTex = GetTextureFromCel(cels[i]);
                
                switch (blendMode)
                {
                    case LayerBlendMode.Normal: texture = Texture2DBlender.Normal(texture, celTex, opacity); break;
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
            int canvasWidth = Header.Width;
            int canvasHeight = Header.Height;
            
            Texture2D texture = Texture2DUtil.CreateTransparentTexture(canvasWidth, canvasHeight);
            Color[] colors = new Color[canvasWidth * canvasHeight];

            int pixelIndex = 0;
            int celXEnd = cel.Width + cel.X;
            int celYEnd = cel.Height + cel.Y;


            for (int y = cel.Y; y < celYEnd; y++)
            {
                if (y < 0 || y >= canvasHeight)
                {
                    pixelIndex += cel.Width;
                    continue;
                }

                for (int x = cel.X; x < celXEnd; x++)
                {
                    if (x >= 0 && x < canvasWidth)
                    {
                        int index = (canvasHeight - 1 - y) * canvasWidth + x;
                        colors[index] = cel.RawPixelData[pixelIndex].GetColor();
                    }

                    ++pixelIndex;
                }
            }

            texture.SetPixels(0, 0, canvasWidth, canvasHeight, colors);
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

        public MetaData[] GetMetaData(Vector2 spritePivot, int pixelsPerUnit)
        {
            Dictionary<int, MetaData> metadatas = new Dictionary<int, MetaData>();

            for (int index = 0; index < Frames.Count; index++)
            {
                List<LayerChunk> layers = GetChunks<LayerChunk>();
                List<CelChunk> cels = Frames[index].GetChunks<CelChunk>();

                cels.Sort((ca, cb) => ca.LayerIndex.CompareTo(cb.LayerIndex));

                for (int i = 0; i < cels.Count; i++)
                {
                    int layerIndex = cels[i].LayerIndex;
                    LayerChunk layer = layers[layerIndex];
                    if (!layer.LayerName.StartsWith(MetaData.MetaDataChar)) //read only metadata layer
                        continue;

                    if (!metadatas.ContainsKey(layerIndex))
                        metadatas[layerIndex] = new MetaData(layer.LayerName);
                    var metadata = metadatas[layerIndex];

                    CelChunk cel = cels[i];
                    Vector2 center = Vector2.zero;
                    int pixelCount = 0;

                    for (int y = 0; y < cel.Height; ++y)
                    {
                        for (int x = 0; x < cel.Width; ++x)
                        {
                            int texX = cel.X + x;
                            int texY = -(cel.Y + y) + Header.Height - 1;
                            var col = cel.RawPixelData[x + y * cel.Width];
                            if (col.GetColor().a > 0.1f)
                            {
                                center += new Vector2(texX, texY);
                                pixelCount++;
                            }
                        }
                    }

                    if (pixelCount > 0)
                    {
                        center /= pixelCount;
                        var pivot = Vector2.Scale(spritePivot, new Vector2(Header.Width, Header.Height));
                        var posWorld = (center - pivot) / pixelsPerUnit + Vector2.one * 0.5f / pixelsPerUnit; //center pos in middle of pixels

                        metadata.Transforms.Add(index, posWorld);
                    }
                }
            }
            return metadatas.Values.ToArray();
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
