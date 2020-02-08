using System;
using System.Collections.Generic;
using Aseprite.Utils;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;

namespace AsepriteImporter
{
    public class SpriteAtlasBuilder
    {
        private readonly Vector2Int spriteSize = Vector2Int.zero;

        
        public SpriteAtlasBuilder()
        {
            spriteSize = new Vector2Int(16, 16);
        }

        public SpriteAtlasBuilder(Vector2Int spriteSize)
        {
            this.spriteSize = spriteSize;
        }

        public SpriteAtlasBuilder(int width, int height)
        {
            spriteSize = new Vector2Int(width, height);
        }

        public Texture2D GenerateAtlas(Texture2D[] sprites, out AseFileSpriteImportData[] spriteImportData, bool baseTwo = true)
        {
            int cols, rows;

            CalculateColsRows(sprites.Length, spriteSize, out cols, out rows);

            return GenerateAtlas(sprites, cols, rows, out spriteImportData, baseTwo);
        }

        public Texture2D GenerateAtlas(Texture2D[] sprites, int cols, int rows, out AseFileSpriteImportData[] spriteImportData, bool baseTwo = true)
        {
            spriteImportData = new AseFileSpriteImportData[sprites.Length];

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
                    Color[] colors = sprites[index].GetPixels();
                    atlas.SetPixels((int)spriteRect.x, (int)spriteRect.y, (int)spriteRect.width, (int)spriteRect.height, sprites[index].GetPixels());
                    atlas.Apply();

                    List<Vector2[]> outline = GenerateRectOutline(spriteRect);
                    spriteImportData[index] = CreateSpriteImportData(index.ToString(), spriteRect, outline);

                    index++;
                    if (index >= sprites.Length)
                        break;
                }
                if (index >= sprites.Length)
                    break;
            }

            return atlas;
        }

        public static List<Vector2[]> GenerateRectOutline(Rect rect)
        {
            List<Vector2[]> outline = new List<Vector2[]>();

            outline.Add(new Vector2[] { new Vector2(rect.x, rect.y), new Vector2(rect.x, rect.yMax) });
            outline.Add(new Vector2[] { new Vector2(rect.x, rect.yMax), new Vector2(rect.xMax, rect.yMax) });
            outline.Add(new Vector2[] { new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMax, rect.y) });
            outline.Add(new Vector2[] { new Vector2(rect.xMax, rect.y), new Vector2(rect.x, rect.yMax) });

            return outline;
        }

        private AseFileSpriteImportData CreateSpriteImportData(string name, Rect rect, List<Vector2[]> outline)
        {
            return new AseFileSpriteImportData()
            {
                alignment = SpriteAlignment.Center,
                border = Vector4.zero,
                name = name,
                outline = outline,
                pivot = new Vector2(0.5f, 0.5f),
                rect = rect,
                spriteID = GUID.Generate().ToString(),
                tessellationDetail = 0
            };
        }

        private static void CalculateColsRows(int spritesCount, Vector2 spriteSize, out int cols, out int rows)
        {
            cols = spritesCount;
            rows = 1;

            float spriteCount = spritesCount;

            var divider = 2;

            var width = cols * spriteSize.x;
            var height = rows * spriteSize.y;


            while (width > height)
            {
                cols = (int)Math.Ceiling(spriteCount / divider);
                rows = (int)Math.Ceiling(spriteCount / cols);

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

            cols = (int)Math.Ceiling(spriteCount / divider);
            rows = (int)Math.Ceiling(spriteCount / cols);
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