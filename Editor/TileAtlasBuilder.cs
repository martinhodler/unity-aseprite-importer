using System;
using Aseprite.Utils;
using UnityEngine;

namespace AsepriteImporter
{
    public class TileAtlasBuilder
    {
        private readonly Vector2Int spriteSize = Vector2Int.zero;
        private AseFileTextureSettings textureSettings;

        public TileAtlasBuilder(AseFileTextureSettings textureSettings, int width, int height)
        {
            spriteSize = new Vector2Int(width, height);
            this.textureSettings = textureSettings;
        }

        Texture2D FlipAtlas(Texture2D original, bool upSideDown = false)
        {
            Texture2D flipped = new Texture2D(original.width, original.height);

            int xN = original.width;
            int yN = original.height;

            for (int i = 0; i < xN; i++)
            {
                for (int j = 0; j < yN; j++)
                {
                    if (upSideDown)
                    {
                        flipped.SetPixel(j, xN - i - 1, original.GetPixel(j, i));
                    }
                    else
                    {
                        flipped.SetPixel(xN - i - 1, j, original.GetPixel(i, j));
                    }
                }
            }

            return flipped;
        }

        public Texture2D GenerateAtlas(Texture2D sprite, bool baseTwo = true)
        {
            var spriteSizeW = textureSettings.tileSize.x + textureSettings.tilePadding.x * 2;
            var spriteSizeH = textureSettings.tileSize.y + textureSettings.tilePadding.y * 2;
            var cols = spriteSize.x / textureSettings.tileSize.x;
            var rows = spriteSize.y / textureSettings.tileSize.y;
            var width = cols * spriteSizeW;
            var height = rows * spriteSizeH;

            if (baseTwo)
            {
                var baseTwoValue = CalculateNextBaseTwoValue(Math.Max(width, height));
                width = baseTwoValue;
                height = baseTwoValue;
            }

            switch (textureSettings.mirror)
            {
                case MirrorOption.X:
                    sprite = FlipAtlas(sprite);
                    break;
                case MirrorOption.Y:
                    sprite = FlipAtlas(sprite, true);
                    break;
            }

            var atlas = Texture2DUtil.CreateTransparentTexture(width, height);
            var index = 0;

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++) {
                    RectInt from = new RectInt(col * textureSettings.tileSize.x,
                                               row * textureSettings.tileSize.y,
                                               textureSettings.tileSize.x,
                                               textureSettings.tileSize.y);
                    RectInt to = new RectInt(col * spriteSizeW + textureSettings.tilePadding.x, 
                                                 row * spriteSizeH + textureSettings.tilePadding.y,
                                                 textureSettings.tileSize.x, 
                                                 textureSettings.tileSize.y);
                    CopyColors(sprite, atlas, from, to);
                    atlas.Apply();
                }
            }

            return atlas;
        }

        private Color[] GetPixels(Texture2D sprite, RectInt from) {
            var res = sprite.GetPixels(from.x, from.y, from.width, from.height);
            if (textureSettings.transparencyMode == TransparencyMode.Mask)
                res = ReplaceMaskToTransparent(textureSettings.transparentColor, res);

            return res;
        }

        private Color GetPixel(Texture2D sprite, int x, int y) {
            var res = sprite.GetPixel(x, y);
            if (textureSettings.transparencyMode == TransparencyMode.Mask)
                if (textureSettings.transparentColor == res)
                    return Color.clear;

            return res;
        }

        private void CopyColors(Texture2D sprite, Texture2D atlas, RectInt from, RectInt to ) {
            atlas.SetPixels(to.x, to.y, to.width, to.height, GetPixels(sprite, from));

            for (int index = 0; index < textureSettings.tilePadding.x; index++)
            {
                RectInt lf = new RectInt(from.x, from.y, 1, from.height);
                RectInt lt = new RectInt(to.x - index - 1, to.y, 1, to.height);
                RectInt rf = new RectInt(from.xMax - 1, from.y, 1, from.height);
                RectInt rt = new RectInt(to.xMax + index, to.y, 1, to.height);
                atlas.SetPixels(lt.x, lt.y, lt.width, lt.height, GetPixels(sprite, lf));
                atlas.SetPixels(rt.x, rt.y, rt.width, rt.height, GetPixels(sprite, rf));
            }

            for (int index = 0; index < textureSettings.tilePadding.y; index++)
            {
                RectInt tf = new RectInt(from.x, from.y, from.width, 1);
                RectInt tt = new RectInt(to.x, to.y - index - 1, to.width, 1);
                RectInt bf = new RectInt(from.x, from.yMax - 1, from.width, 1);
                RectInt bt = new RectInt(to.x, to.yMax + index, to.width, 1);

                atlas.SetPixels(tt.x, tt.y, tt.width, tt.height, GetPixels(sprite, tf));
                atlas.SetPixels(bt.x, bt.y, bt.width, bt.height, GetPixels(sprite, bf));
            }

            for (int x = 0; x < textureSettings.tilePadding.x; x++)
            {
                for (int y = 0; y < textureSettings.tilePadding.y; y++)
                {
                    atlas.SetPixel(to.x - x - 1, to.y - y - 1, GetPixel(sprite, from.x, from.y));
                    atlas.SetPixel(to.xMax + x, to.y - y - 1, GetPixel(sprite, from.xMax - 1, from.y));
                    atlas.SetPixel(to.x - x - 1, to.yMax + y, GetPixel(sprite, from.x, from.yMax - 1));
                    atlas.SetPixel(to.xMax + x, to.yMax + y, GetPixel(sprite, from.xMax - 1, from.yMax - 1));
                }
            }
        }

        // step and replace all mask instances to clear
        private static Color[] ReplaceMaskToTransparent(Color mask, Color[] pallete)
        {
            for (int i = 0; i < pallete.Length; i++)
            {
                if (pallete[i] == mask)
                {
                    pallete[i] = UnityEngine.Color.clear;
                }
            }

            return pallete;
        }

        private static int CalculateNextBaseTwoValue(int value)
        {
            var exponent = 0;
            var baseTwo = 0;

            while (baseTwo < value)
            {
                baseTwo = (int) Math.Pow(2, exponent);
                exponent++;
            }

            return baseTwo;
        }
    }
}
