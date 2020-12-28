using System.Collections.Generic;

namespace AsepriteImporter.EditorUtils
{
    public class TextureImporterEditorUtils
    {
        internal enum TextureImportTypeIndex
        {
            Default = 0,
            Image = 1,
            NormalMap = 2,
            Bump = 3,
            GUI = 4,
            Cubemap = 5,
            Reflection = 6,
            Cookie = 7,
            Advanced = 8,
            Lightmap = 9,
            Cursor = 10,
            Sprite = 11,
            HDRI = 12,
            SingleChannel = 13
        }

        internal static readonly Dictionary<int, string> mappingGenerationType = new Dictionary<int, string>
        {
            { 0, "Sprite" },
        };

        internal static readonly Dictionary<int, string> MappingTextureImportTypes = new Dictionary<int, string>
        {
            { (int)TextureImportTypeIndex.Default, "Default" },
            { (int)TextureImportTypeIndex.NormalMap, "Normal map" },
            { (int)TextureImportTypeIndex.GUI, "Editor GUI and Legacy GUI" },
            { (int)TextureImportTypeIndex.Sprite, "Sprite (2D and UI)" },
            { (int)TextureImportTypeIndex.Cursor, "Cursor" },
            { (int)TextureImportTypeIndex.Cookie, "Cookie" },
            { (int)TextureImportTypeIndex.Lightmap, "Lightmap" },
            { (int)TextureImportTypeIndex.SingleChannel, "Single Channel" },
        };

        internal static readonly Dictionary<int, string> mappingTextureShapes = new Dictionary<int, string>
        {
            { 0, "2D" },
            { 1, "Cube" },
        };

        internal static readonly Dictionary<int, string> mappingAlphaSource = new Dictionary<int, string>
        {
            { 0, "None" },
            { 1, "Input Texture Alpha" },
            { 2, "From Gray Scale" },
        };

        internal static readonly Dictionary<int, string> mappingMipMapFilter = new Dictionary<int, string>
        {
            { 0, "Box" },
            { 1, "Kaiser" },
        };

        internal static readonly Dictionary<int, string> mappingFilterMode = new Dictionary<int, string>
        {
            { 0, "Point (no filter)" },
            { 1, "Bilinear" },
            { 2, "Trilinear" },
        };

        internal static readonly int[] textureType2DFixed = { 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        internal static readonly int[] textureTypeAnisoEnabled =
        {
            (int)TextureImportTypeIndex.Default,
            (int)TextureImportTypeIndex.NormalMap
        };
    }
}