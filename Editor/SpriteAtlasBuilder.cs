using System;
using System.Collections.Generic;
using System.Linq;
using Aseprite.Utils;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace AsepriteImporter
{
    public class SpriteAtlasBuilder
    {
        private readonly Vector2Int spriteSize = Vector2Int.zero;
        private AseFileTextureSettings textureSettings;

        public SpriteAtlasBuilder(AseFileTextureSettings textureSettings)
        {
            spriteSize = new Vector2Int(16, 16);
            this.textureSettings = textureSettings;
        }

        public SpriteAtlasBuilder(AseFileTextureSettings textureSettings, Vector2Int spriteSize)
        {
            this.spriteSize = spriteSize;
            this.textureSettings = textureSettings;
        }

        public SpriteAtlasBuilder(AseFileTextureSettings textureSettings, int width, int height)
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

        public Texture2D GenerateAtlas(Texture2D[] sprites, out SpriteImportData[] spriteData, bool mask,
            bool baseTwo = true)
        {
            var cols = sprites.Length;
            var rows = 1;

            float spriteCount = sprites.Length;

            var divider = 2;

            var width = cols * spriteSize.x;
            var height = rows * spriteSize.y;


            while (width > height)
            {
                cols = (int) Math.Ceiling(spriteCount / divider);
                rows = (int) Math.Ceiling(spriteCount / cols);

                width = cols * spriteSize.x;
                height = rows * spriteSize.y;

                if (cols <= 1)
                {
                    break;
                }

                divider++;
            }

            if (height > width)
                divider -= 2;
            else
                divider -= 1;

            if (divider < 1)
                divider = 1;

            cols = (int) Math.Ceiling(spriteCount / divider);
            rows = (int) Math.Ceiling(spriteCount / cols);

            if (mask)
            {
                return GenerateAtlas(sprites, out spriteData, cols, rows, textureSettings.transparentColor, baseTwo);
            }

            return GenerateAtlas(sprites, out spriteData, cols, rows, baseTwo);
        }

        public Texture2D GenerateAtlas(Texture2D[] sprites, out SpriteImportData[] spriteData, int cols, int rows,
            bool baseTwo = true)
        {
            var spriteImportData = new List<SpriteImportData>();

            var width = cols * spriteSize.x;
            var height = rows * spriteSize.y;

            if (baseTwo)
            {
                var baseTwoValue = CalculateNextBaseTwoValue(Math.Max(width, height));
                width = baseTwoValue;
                height = baseTwoValue;
            }

            var atlas = Texture2DUtil.CreateTransparentTexture(width, height);
            var index = 0;

            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    Rect spriteRect = new Rect(col * spriteSize.x, atlas.height - ((row + 1) * spriteSize.y),
                        spriteSize.x, spriteSize.y);

                    switch (textureSettings.mirror)
                    {
                        case MirrorOption.X:
                            sprites[index] = FlipAtlas(sprites[index]);
                            break;
                        case MirrorOption.Y:
                            sprites[index] = FlipAtlas(sprites[index], true);
                            break;
                    }

                    atlas.SetPixels((int) spriteRect.x, (int) spriteRect.y, (int) spriteRect.width,
                        (int) spriteRect.height, sprites[index].GetPixels());

                    atlas.Apply();

                    var importData = new SpriteImportData
                    {
                        rect = spriteRect,
                        pivot = textureSettings.spritePivot,
                        border = Vector4.zero,
                        name = index.ToString()
                    };

                    spriteImportData.Add(importData);

                    index++;
                    if (index >= sprites.Length)
                        break;
                }

                if (index >= sprites.Length)
                    break;
            }

            spriteData = spriteImportData.ToArray();
            return atlas;
        }

        // replaces color to transparent
        public Texture2D GenerateAtlas(Texture2D[] sprites, out SpriteImportData[] spriteData, int cols, int rows,
            Color mask, bool baseTwo = true)
        {
            var spriteImportData = new List<SpriteImportData>();

            var width = cols * spriteSize.x;
            var height = rows * spriteSize.y;

            if (baseTwo)
            {
                var baseTwoValue = CalculateNextBaseTwoValue(Math.Max(width, height));
                width = baseTwoValue;
                height = baseTwoValue;
            }

            // blank transparent canvas
            var atlas = Texture2DUtil.CreateTransparentTexture(width, height);
            var index = 0;

            // step through each pixel
            for (var row = 0; row < rows; row++)
            {
                for (var col = 0; col < cols; col++)
                {
                    Rect spriteRect = new Rect(col * spriteSize.x, atlas.height - ((row + 1) * spriteSize.y),
                        spriteSize.x, spriteSize.y);

                    // change pixel mask to transparent
                    Color[] pixelPallete = ReplaceMaskToTransparent(mask, sprites[index].GetPixels());
                    atlas.SetPixels((int) spriteRect.x, (int) spriteRect.y, (int) spriteRect.width,
                        (int) spriteRect.height, pixelPallete);
                    atlas.Apply();

                    var importData = new SpriteImportData
                    {
                        rect = spriteRect,
                        pivot = textureSettings.spritePivot,
                        border = Vector4.zero,
                        name = index.ToString()
                    };

                    spriteImportData.Add(importData);

                    index++;
                    if (index >= sprites.Length)
                        break;
                }

                if (index >= sprites.Length)
                    break;
            }

            spriteData = spriteImportData.ToArray();
            return atlas;
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
