using System;
using System.Collections.Generic;
using Aseprite.Utils;
using TMPro.SpriteAssetUtilities;
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



        public Texture2D GenerateAtlas(Texture2D[] sprites, out SpriteImportData[] spriteData, bool baseTwo = true)
        {
            var cols = sprites.Length;
            var rows = 1;

            float spriteCount = sprites.Length;
            var canvasSize = 0;
            
            var divider = 2;

            var width = cols * spriteSize.x;
            var height = rows * spriteSize.y;

            while (width > height)
            {
                cols = (int)Math.Ceiling(spriteCount / divider);
                rows = (int)Math.Ceiling(spriteCount / cols);

                width = cols * spriteSize.x;
                height = rows * spriteSize.y;
                
                divider++;
            }

            if (height > width)
                divider -= 2;
            else
                divider -= 1;
            
            cols = (int)Math.Ceiling(spriteCount / divider);
            rows = (int)Math.Ceiling(spriteCount / cols);
            
            return GenerateAtlas(sprites, out spriteData, cols, rows, baseTwo);
        }
        
        public Texture2D GenerateAtlas(Texture2D[] sprites, out SpriteImportData[] spriteData, int cols, int rows, bool baseTwo = true)
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
                    Rect spriteRect = new Rect(col * spriteSize.x, atlas.height - ((row + 1) * spriteSize.y), spriteSize.x, spriteSize.y);
                    atlas.SetPixels((int) spriteRect.x, (int) spriteRect.y, (int) spriteRect.width, (int) spriteRect.height, sprites[index].GetPixels());
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

        private static int CalculateNextBaseTwoValue(int value)
        {
            var exponent = 0;
            var baseTwo = 0;

            while (baseTwo < value)
            {
                baseTwo = (int)Math.Pow(2, exponent);
                exponent++;
            }

            return baseTwo;
        }
    }
}