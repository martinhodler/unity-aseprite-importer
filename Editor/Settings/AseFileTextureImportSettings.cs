using UnityEngine;
using UnityEditor;
using System;

namespace AsepriteImporter.Settings
{

    [Serializable]
    public class AseFileTextureImportSettings
    {
        public bool seamlessCubemap;

        //     Mip map bias of the texture.
        public float mipmapBias = 0.5f;

        //     Texture coordinate wrapping mode.
        public TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        //     Texture U coordinate wrapping mode.
        public TextureWrapMode wrapModeU;

        //     Texture V coordinate wrapping mode.
        public TextureWrapMode wrapModeV;

        //     Texture W coordinate wrapping mode for Texture3D.
        public TextureWrapMode wrapModeW;

        //     If the provided alpha channel is transparency, enable this to dilate the color
        //     to avoid filtering artifacts on the edges.
        public bool alphaIsTransparency = true;

        //     Sprite texture import mode.
        public int spriteMode = (int) SpriteImportMode.Single;

        //     The number of pixels in the sprite that correspond to one unit in world space.
        public float spritePixelsPerUnit = 100;

        //     The tessellation detail to be used for generating the mesh for the associated
        //     sprite if the SpriteMode is set to Single. For Multiple sprites, use the SpriteEditor
        //     to specify the value per sprite. Valid values are in the range [0-1], with higher
        //     values generating a tighter mesh. A default of -1 will allow Unity to determine
        //     the value automatically.
        public float spriteTessellationDetail;

        //     The number of blank pixels to leave between the edge of the graphic and the mesh.
        public uint spriteExtrude = 1;

        //     SpriteMeshType defines the type of Mesh that TextureImporter generates for a
        //     Sprite.
        public SpriteMeshType spriteMeshType = SpriteMeshType.Tight;

        //     Edge-relative alignment of the sprite graphic.
        public int spriteAlignment = (int) SpriteAlignment.Center;

        //     Pivot point of the Sprite relative to its graphic's rectangle.
        public Vector2 spritePivot = Vector2.zero;

        //     Border sizes of the generated sprites.
        public Vector4 spriteBorder = Vector4.zero;

        //     Generates a default physics shape for a Sprite if a physics shape has not been
        //     set by the user.
        public bool spriteGenerateFallbackPhysicsShape = true;


        //     Anisotropic filtering level of the texture.
        public int aniso = 1;

        //     Filtering mode of the texture.
        public FilterMode filterMode = FilterMode.Point;

        //     Convolution mode.
        public TextureImporterCubemapConvolution cubemapConvolution;

        //     Which type of texture are we dealing with here.
        public TextureImporterType textureType = TextureImporterType.Sprite;

        //     Shape of imported texture.
        public TextureImporterShape textureShape = TextureImporterShape.Texture2D;

        //     Mipmap filtering mode.
        public TextureImporterMipFilter mipmapFilter;

        //     Generate mip maps for the texture?
        public bool mipmapEnabled = false;

        //     Is texture storing color data?
        public bool sRGBTexture = true;

        //     Fade out mip levels to gray color?
        public bool fadeOut = false;

        //     Enable this to avoid colors seeping out to the edge of the lower Mip levels.
        //     Used for light cookies.
        public bool borderMipmap;

        //     Enables or disables coverage-preserving alpha MIP mapping.
        public bool mipMapsPreserveCoverage;

        //     Mip level where texture begins to fade out to gray.
        public int mipmapFadeDistanceStart = 1;

        //     Returns or assigns the alpha test reference value.
        public float alphaTestReferenceValue = 1f;

        //     Convert heightmap to normal map?
        public bool convertToNormalMap;

        //     Amount of bumpyness in the heightmap.
        public float heightmapScale;

        //     Normal map filtering mode.
        public TextureImporterNormalFilter normalMapFilter;

        //     Select how the alpha of the imported texture is generated.
        public TextureImporterAlphaSource alphaSource = TextureImporterAlphaSource.FromInput;

        //     Color or Alpha component TextureImporterType|Single Channel Textures uses.
        public TextureImporterSingleChannelComponent singleChannelComponent;

        //     Is texture data readable from scripts.
        public bool readable = false;

        //     Enable mipmap streaming for this texture.
        public bool streamingMipmaps;

        //     Relative priority for this texture when reducing memory size in order to hit
        //     the memory budget.
        public int streamingMipmapsPriority;

        //     Scaling mode for non power of two textures.
        public TextureImporterNPOTScale npotScale;

        //     Cubemap generation mode.
        public TextureImporterGenerateCubemap generateCubemap;

        //     Mip level where texture is faded out to gray completely.
        public int mipmapFadeDistanceEnd = 3;




        public TextureImporterSettings ToImporterSettings()
        {
            TextureImporterSettings settings = new TextureImporterSettings()
            {
                seamlessCubemap = seamlessCubemap,
                mipmapBias = mipmapBias,
                wrapMode = wrapMode,
                wrapModeU = wrapModeU,
                wrapModeV = wrapModeV,
                wrapModeW = wrapModeW,
                alphaIsTransparency = alphaIsTransparency,
                spriteMode = spriteMode,
                spritePixelsPerUnit = spritePixelsPerUnit,
                spriteTessellationDetail = spriteTessellationDetail,
                spriteExtrude = spriteExtrude,
                spriteMeshType = spriteMeshType,
                spriteAlignment = spriteAlignment,
                spritePivot = spritePivot,
                spriteBorder = spriteBorder,
                spriteGenerateFallbackPhysicsShape = spriteGenerateFallbackPhysicsShape,
                aniso = aniso,
                filterMode = filterMode,
                cubemapConvolution = cubemapConvolution,
                textureType = textureType,
                textureShape = textureShape,
                mipmapFilter = mipmapFilter,
                mipmapEnabled = mipmapEnabled,
                sRGBTexture = sRGBTexture,
                fadeOut = fadeOut,
                borderMipmap = borderMipmap,
                mipMapsPreserveCoverage = mipMapsPreserveCoverage,
                mipmapFadeDistanceStart = mipmapFadeDistanceStart,
                alphaTestReferenceValue = alphaTestReferenceValue,
                convertToNormalMap = convertToNormalMap,
                heightmapScale = heightmapScale,
                normalMapFilter = normalMapFilter,
                alphaSource = alphaSource,
                singleChannelComponent = singleChannelComponent,
                readable = readable,
                streamingMipmaps = streamingMipmaps,
                streamingMipmapsPriority = streamingMipmapsPriority,
                npotScale = npotScale,
                generateCubemap = generateCubemap,
                mipmapFadeDistanceEnd = mipmapFadeDistanceEnd
            };

            return settings;
        }

        public void Apply(TextureImporterSettings settings)
        {
            this.seamlessCubemap = settings.seamlessCubemap;
            this.mipmapBias = settings.mipmapBias;
            this.wrapMode = settings.wrapMode;
            this.wrapModeU = settings.wrapModeU;
            this.wrapModeV = settings.wrapModeV;
            this.wrapModeW = settings.wrapModeW;
            this.alphaIsTransparency = settings.alphaIsTransparency;
            this.spriteMode = settings.spriteMode;
            this.spritePixelsPerUnit = settings.spritePixelsPerUnit;
            this.spriteTessellationDetail = settings.spriteTessellationDetail;
            this.spriteExtrude = settings.spriteExtrude;
            this.spriteMeshType = settings.spriteMeshType;
            this.spriteAlignment = settings.spriteAlignment;
            this.spritePivot = settings.spritePivot;
            this.spriteBorder = settings.spriteBorder;
            this.spriteGenerateFallbackPhysicsShape = settings.spriteGenerateFallbackPhysicsShape;
            this.aniso = settings.aniso;
            this.filterMode = settings.filterMode;
            this.cubemapConvolution = settings.cubemapConvolution;
            this.textureType = settings.textureType;
            this.textureShape = settings.textureShape;
            this.mipmapFilter = settings.mipmapFilter;
            this.mipmapEnabled = settings.mipmapEnabled;
            this.sRGBTexture = settings.sRGBTexture;
            this.fadeOut = settings.fadeOut;
            this.borderMipmap = settings.borderMipmap;
            this.mipMapsPreserveCoverage = settings.mipMapsPreserveCoverage;
            this.mipmapFadeDistanceStart = settings.mipmapFadeDistanceStart;
            this.alphaTestReferenceValue = settings.alphaTestReferenceValue;
            this.convertToNormalMap = settings.convertToNormalMap;
            this.heightmapScale = settings.heightmapScale;
            this.normalMapFilter = settings.normalMapFilter;
            this.alphaSource = settings.alphaSource;
            this.singleChannelComponent = settings.singleChannelComponent;
            this.readable = settings.readable;
            this.streamingMipmaps = settings.streamingMipmaps;
            this.streamingMipmapsPriority = settings.streamingMipmapsPriority;
            this.npotScale = settings.npotScale;
            this.generateCubemap = settings.generateCubemap;
            this.mipmapFadeDistanceEnd = settings.mipmapFadeDistanceEnd;
        }

        public void ApplyTextureType(TextureImporterType textureType)
        {
            TextureImporterSettings settings = ToImporterSettings();
            settings.ApplyTextureType(textureType);

            Apply(settings);
        }

    }
}