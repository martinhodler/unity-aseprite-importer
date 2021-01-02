using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aseprite;
using AsepriteImporter.Data;
using AsepriteImporter.DataProviders;
using AsepriteImporter.Settings;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace AsepriteImporter.Importers
{
    public class BundledSpriteImporter : SpriteImporter
    {
        [SerializeField] public Texture2D atlas;

        [SerializeField] private Texture2D thumbnail;

        [SerializeField] private int frameCount;
        [SerializeField] private int textureWidth;
        [SerializeField] private int textureHeight;

        [SerializeField] private Sprite[] sprites;
        
        public Texture2D Thumbnail => thumbnail;
        public override Sprite[] Sprites => sprites;
        

        public override SpriteImportMode spriteImportMode => (SpriteImportMode)TextureImportSettings.spriteMode;
        public override float pixelsPerUnit => TextureImportSettings.spritePixelsPerUnit;
        public override UnityEngine.Object targetObject => Importer;
        
        private string name;

        public BundledSpriteImporter(AseFileImporter importer) : base(importer)
        {
            
        }
        
        public override void OnImport()
        {
            name = GetFileName(AssetPath);
            sprites = new Sprite[0];
            
            GenerateAtlasTexture();
            
            if (SpriteImportData == null || SpriteImportData.Length == 0 || TextureImportSettings.spriteMode == (int)SpriteImportMode.Single)
            {
                SetSingleSpriteImportData();
            }
            
            ProcessAnimationSettings();

            GenerateTexture(AssetPath);

            ApplySpritesToAnimation();

            Context.AddObjectToAsset("Texture", Texture);
            Context.SetMainObject(Texture);

            foreach (Sprite sprite in sprites)
            {
                Context.AddObjectToAsset(sprite.name, sprite);
            }


            if (Settings.generateAnimations)
            {
                AnimationImporter animationImporter = new AnimationImporter(AsepriteFile);
                AnimationClip[] animations = animationImporter.GenerateAnimations(name, AnimationSettings);

                foreach (AnimationClip clip in animations)
                {
                    //AssetDatabase.CreateAsset(clip, GetPath(assetPath) + clip.name + ".asset");
                    if (clip == null)
                        continue;

                    Context.AddObjectToAsset(clip.name, clip);
                }
            }
        }
        
        public void SetSingleSpriteImportData()
        {
            Rect spriteRect = new Rect(0, atlas.height - AsepriteFile.Header.Height, AsepriteFile.Header.Width, AsepriteFile.Header.Height);
            SpriteImportData = new AseFileSpriteImportData[]
            {
                    new AseFileSpriteImportData()
                    {
                        alignment = SpriteAlignment.Center,
                        border = Vector4.zero,
                        name = name,
                        outline = SpriteAtlasBuilder.GenerateRectOutline(spriteRect),
                        pivot = new Vector2(0.5f, 0.5f),
                        rect = spriteRect,
                        spriteID = GUID.Generate().ToString(),
                        tessellationDetail = 0
                    }
            };

            AnimationSettings = null;
        }

        public AseFileSpriteImportData[] GetSingleSpriteImportData()
        {
            Rect spriteRect = new Rect(0, 0, textureWidth, textureHeight);
            return new AseFileSpriteImportData[]
            {
                    new AseFileSpriteImportData()
                    {
                        alignment = SpriteAlignment.Center,
                        border = Vector4.zero,
                        name = name,
                        outline = SpriteAtlasBuilder.GenerateRectOutline(spriteRect),
                        pivot = new Vector2(0.5f, 0.5f),
                        rect = spriteRect,
                        spriteID = GUID.Generate().ToString(),
                        tessellationDetail = 0
                    }
            };
        }

        private void GenerateTexture(string assetPath)
        {
            SourceTextureInformation textureInformation = new SourceTextureInformation()
            {
                containsAlpha = true,
                hdr = false,
                height = textureHeight,
                width = textureWidth
            };

            TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings()
            {
                overridden = false
            };

            TextureGenerationSettings settings = new TextureGenerationSettings()
            {
                assetPath = assetPath,
                spriteImportData = ConvertAseFileSpriteImportDataToUnity(SpriteImportData),
                textureImporterSettings = TextureImportSettings.ToImporterSettings(),
                enablePostProcessor = false,
                sourceTextureInformation = textureInformation,
                qualifyForSpritePacking = true,
                platformSettings = platformSettings,
                spritePackingTag = "aseprite",
                secondarySpriteTextures = new SecondarySpriteTexture[0]
            };


            TextureGenerationOutput output = TextureGenerator.GenerateTexture(settings, new Unity.Collections.NativeArray<Color32>(atlas.GetPixels32(), Unity.Collections.Allocator.Temp));

            Texture = output.texture;
            thumbnail = output.thumbNail;
            sprites = output.sprites;
        }

        public void GenerateAtlasTexture(bool overwriteSprites = false)
        {
            if (atlas != null)
                return;

            SpriteAtlasBuilder atlasBuilder = new SpriteAtlasBuilder(AsepriteFile.Header.Width, AsepriteFile.Header.Height);

            Texture2D[] frames = AsepriteFile.GetFrames();

            atlas = atlasBuilder.GenerateAtlas(frames, out var importData, false);

            textureWidth = atlas.width;
            textureHeight = atlas.height;
            frameCount = importData.Length;

            // Rename sprites

            if (overwriteSprites)
            {
                for (int i = 0; i < importData.Length; i++)
                {
                    importData[i].name = string.Format("{0}_{1}", name, importData[i].name);
                }

                SpriteRects = new SpriteRect[0];
                SpriteImportData = importData;

                if (SpriteImportData.Length > 1)
                    TextureImportSettings.spriteMode = (int)SpriteImportMode.Multiple;


                AssetDatabase.WriteImportSettingsIfDirty(AssetPath);
            }
        }

        private void ProcessAnimationSettings()
        {
            AnimationImporter animationImporter = new AnimationImporter(AsepriteFile);

            if (AnimationSettings == null || AnimationSettings.Length == 0)
            {
                AnimationSettings = animationImporter.GetAnimationImportSettings();
            }
            else
            {
                AseFileAnimationSettings[] settings = animationImporter.GetAnimationImportSettings();

                List<AseFileAnimationSettings> newSettings = new List<AseFileAnimationSettings>();
                foreach (var setting in settings)
                {
                    var currentSetting = Array.Find(AnimationSettings, s => s.animationName == setting.animationName);

                    if (currentSetting != null)
                    {
                        // Settings already exist
                        newSettings.Add(currentSetting);
                    }
                    else
                    {
                        // New Settings
                        newSettings.Add(setting);
                    }
                }

                AnimationSettings = newSettings.ToArray();
            }
        }

        private void ApplySpritesToAnimation()
        {
            if (sprites.Length != frameCount)
                return;


            for (int i = 0; i < AnimationSettings.Length; i++)
            {
                var settings = AnimationSettings[i];

                for (int n = 0; n < settings.sprites.Length; n++)
                {
                    if (settings.sprites[n] == null)
                        settings.sprites[n] = sprites[settings.frameNumbers[n]];
                }

            }
            /*
            for (int i = 0; i < animationSettings.Length; i++)
            {
                var settings = animationSettings[i];

                Dictionary<int, Sprite> animationSprites = new Dictionary<int, Sprite>();

                for (int n = 0; n < settings.sprites.Length; n++)
                {
                    if (settings.sprites[n] != null)
                        animationSprites.Add(settings.frameNumbers[n], settings.sprites[n]);
                }

            }
            */
        }



        private string GetFileName(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            string filename = parts[parts.Length - 1];

            return filename.Substring(0, filename.LastIndexOf('.'));
        }

        private string GetPath(string assetPath)
        {
            string[] parts = assetPath.Split('/');
            string filename = parts[parts.Length - 1];

            return assetPath.Replace(filename, "");
        }

        private static AseFile ReadAseFile(string assetPath)
        {
            FileStream fileStream = new FileStream(assetPath, FileMode.Open, FileAccess.Read);
            AseFile aseFile = new AseFile(fileStream);
            fileStream.Close();

            return aseFile;
        }




        public override void Apply()
        {
            if (SpriteRects != null && SpriteRects.Length > 0)
            {
                List<AseFileSpriteImportData> newImportData = new List<AseFileSpriteImportData>();

                foreach (SpriteRect spriteRect in SpriteRects)
                {
                    AseFileSpriteImportData data = new AseFileSpriteImportData()
                    {
                        alignment = spriteRect.alignment,
                        border = spriteRect.border,
                        name = spriteRect.name,
                        pivot = spriteRect.pivot,
                        rect = spriteRect.rect,
                        spriteID = spriteRect.spriteID.ToString()
                    };

                    AseFileSpriteImportData current = Array.Find<AseFileSpriteImportData>(SpriteImportData, d => d.spriteID == spriteRect.spriteID.ToString());

                    if (current != null)
                    {
                        data.outline = current.outline;
                        data.tessellationDetail = current.tessellationDetail;
                    }
                    else
                    {
                        data.outline = SpriteAtlasBuilder.GenerateRectOutline(data.rect);
                        data.tessellationDetail = 0;
                    }

                    newImportData.Add(data);
                }

                SpriteRects = new SpriteRect[0];

                SpriteImportData = newImportData.ToArray();
                EditorUtility.SetDirty(Importer);
            }

            AssetDatabase.WriteImportSettingsIfDirty(AssetPath);
            //SaveAndReimport();

            AssetDatabase.Refresh();
            AssetDatabase.LoadAllAssetsAtPath(AssetPath);
            EditorApplication.RepaintProjectWindow();
        }

        private static SpriteImportData[] ConvertAseFileSpriteImportDataToUnity(AseFileSpriteImportData[] spriteImportData)
        {
            SpriteImportData[] importData = new SpriteImportData[spriteImportData.Length];

            for (int i = 0; i < spriteImportData.Length; i++)
            {
                importData[i] = spriteImportData[i].ToSpriteImportData();
            }

            return importData;
        }
    }
}