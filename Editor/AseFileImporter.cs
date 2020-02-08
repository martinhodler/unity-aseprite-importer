using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using System.IO;
using Aseprite;
using UnityEditor;
using Aseprite.Chunks;
using System.Text;
using UnityEditor.U2D.Sprites;
using System;
using AsepriteImporter.DataProviders;

namespace AsepriteImporter
{
    public enum AseFileImportType
    {
        Sprite,
        Tileset,
        LayerToSprite
    }

    [ScriptedImporter(1, new []{ "ase", "aseprite" })]
    public class AseFileImporter : ScriptedImporter, ISpriteEditorDataProvider
    {
        [SerializeField] public AsepriteTextureImportSettings textureImporterSettings;
        [SerializeField] public AseFileAnimationSettings[] animationSettings;
        [SerializeField] public Texture2D atlas;
        //[SerializeField] public AseFileImportType importType;
        [SerializeField] private bool generateAnimations;
        [SerializeField] private string animationImportPath;

        [SerializeField] private Texture2D texture;
        [SerializeField] private Texture2D thumbnail;
        
        [SerializeField] private AseFileSpriteImportData[] spriteImportData;

        [SerializeField] private int frameCount;
        [SerializeField] private int textureWidth;
        [SerializeField] private int textureHeight;

        public Texture2D Texture => texture;
        public Texture2D Thumbnail => thumbnail;

        public AseFileSpriteImportData[] SpriteImportData => spriteImportData;

        public SpriteImportMode spriteImportMode => (SpriteImportMode)textureImporterSettings.spriteMode;

        public float pixelsPerUnit => textureImporterSettings.spritePixelsPerUnit;

        public UnityEngine.Object targetObject => this;



        private AseFile _aseFile;
        private SpriteRect[] _spriteRects;
        [SerializeField] private Sprite[] sprites;


        public override void OnImportAsset(AssetImportContext ctx)
        {
            name = GetFileName(assetPath);
            _aseFile = ReadAseFile(assetPath);

            GenerateAtlasTexture();

            if (spriteImportData == null || spriteImportData.Length == 0)
            {
                SetSingleSpriteImportData();
            }

            ProcessAnimationSettings();

            GenerateTexture(ctx.assetPath);

            ApplySpritesToAnimation();

            ctx.AddObjectToAsset("Texture", texture);
            ctx.SetMainObject(texture);

            foreach (Sprite sprite in sprites)
            {
                ctx.AddObjectToAsset(sprite.name, sprite);
            }


            if (generateAnimations)
            {
                AnimationImporter animationImporter = new AnimationImporter(_aseFile);
                AnimationClip[] animations = animationImporter.GenerateAnimations(name, animationSettings);

                foreach (AnimationClip clip in animations)
                {
                    //AssetDatabase.CreateAsset(clip, GetPath(assetPath) + clip.name + ".asset");
                    ctx.AddObjectToAsset(clip.name, clip);
                }
            }
        }

        public void SetSingleSpriteImportData()
        {
            Rect spriteRect = new Rect(0, 0, textureWidth, textureHeight);
            spriteImportData = new AseFileSpriteImportData[]
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
                spriteImportData = ConvertAseFileSpriteImportDataToUnity(this.spriteImportData),
                textureImporterSettings = textureImporterSettings.ToImporterSettings(),
                enablePostProcessor = false,
                sourceTextureInformation = textureInformation,
                qualifyForSpritePacking = true,
                platformSettings = platformSettings,
                spritePackingTag = "aseprite",
                secondarySpriteTextures = new SecondarySpriteTexture[0]

            };


            TextureGenerationOutput output = TextureGenerator.GenerateTexture(settings, new Unity.Collections.NativeArray<Color32>(atlas.GetPixels32(), Unity.Collections.Allocator.Temp));

            texture = output.texture;
            thumbnail = output.thumbNail;
            sprites = output.sprites;
        }

        public void GenerateAtlasTexture(bool overwriteSprites = false)
        {
            if (atlas != null)
                return;

            if (_aseFile == null)
                _aseFile = ReadAseFile(assetPath);

            SpriteAtlasBuilder atlasBuilder = new SpriteAtlasBuilder(_aseFile.Header.Width, _aseFile.Header.Height);

            Texture2D[] frames = _aseFile.GetFrames();

            atlas = atlasBuilder.GenerateAtlas(frames, out AseFileSpriteImportData[] importData);

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

                this._spriteRects = new SpriteRect[0];
                this.spriteImportData = importData;

                if (spriteImportData.Length > 1)
                    this.textureImporterSettings.spriteMode = (int)SpriteImportMode.Multiple;


                AssetDatabase.WriteImportSettingsIfDirty(assetPath);
            }
        }

        private void ProcessAnimationSettings()
        {
            AnimationImporter animationImporter = new AnimationImporter(_aseFile);

            if (animationSettings == null || animationSettings.Length == 0)
            {
                animationSettings = animationImporter.GetAnimationImportSettings();
            }
            else
            {
                AseFileAnimationSettings[] settings = animationImporter.GetAnimationImportSettings();

                List<AseFileAnimationSettings> newSettings = new List<AseFileAnimationSettings>();
                foreach (var setting in settings)
                {
                    var currentSetting = Array.Find(animationSettings, s => s.animationName == setting.animationName);

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

                animationSettings = newSettings.ToArray();
            }
        }

        private void ApplySpritesToAnimation()
        {
            if (sprites.Length != frameCount)
                return;


            for (int i = 0; i < animationSettings.Length; i++)
            {
                var settings = animationSettings[i];

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

        





        private AsepriteTextureDataProvider textureDataProvider;
        private AsepriteOutlineDataProvider outlineDataProvider;


        public SpriteRect[] GetSpriteRects()
        {
            List<SpriteRect> spriteRects = new List<SpriteRect>();

            foreach (AseFileSpriteImportData importData in spriteImportData)
            {
                spriteRects.Add(new SpriteRect()
                {
                    spriteID = ConvertStringToGUID(importData.spriteID),
                    alignment = importData.alignment,
                    border = importData.border,
                    name = importData.name,
                    pivot = importData.pivot,
                    rect = importData.rect
                });
            }

            this._spriteRects = spriteRects.ToArray();
            return this._spriteRects;
        }

        public void SetSpriteRects(SpriteRect[] spriteRects)
        {
            this._spriteRects = spriteRects;
        }

        public void Apply()
        {
            if (_spriteRects != null && _spriteRects.Length > 0)
            {
                List<AseFileSpriteImportData> newImportData = new List<AseFileSpriteImportData>();

                foreach (SpriteRect spriteRect in _spriteRects)
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

                    AseFileSpriteImportData current = Array.Find<AseFileSpriteImportData>(spriteImportData, d => d.spriteID == spriteRect.spriteID.ToString());

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

                _spriteRects = new SpriteRect[0];

                this.spriteImportData = newImportData.ToArray();
                EditorUtility.SetDirty(this);
            }

            AssetDatabase.WriteImportSettingsIfDirty(assetPath);
            SaveAndReimport();

            AssetDatabase.Refresh();
            AssetDatabase.LoadAllAssetsAtPath(assetPath);
            EditorApplication.RepaintProjectWindow();
        }

        public void InitSpriteEditorDataProvider()
        {
            textureDataProvider = new AsepriteTextureDataProvider(this);
            outlineDataProvider = new AsepriteOutlineDataProvider(this);
        }

        public T GetDataProvider<T>() where T : class
        {
            if (typeof(T).Equals(typeof(ITextureDataProvider)))
                return textureDataProvider as T;

            if (typeof(T).Equals(typeof(ISpriteOutlineDataProvider)))
                return outlineDataProvider as T;

            if (typeof(T).Equals(typeof(ISpriteEditorDataProvider)))
                return this as T;

                Debug.Log(typeof(T).Name + " not found");
            return null;
        }

        public bool HasDataProvider(Type type)
        {

            if (type == typeof(ITextureDataProvider))
                return true;

            if (type == typeof(ISpriteOutlineDataProvider))
                return true;

            //Debug.Log("Does not support" + type.Name);
            return false;
        }

        private GUID ConvertStringToGUID(string guidString)
        {
            if (!GUID.TryParse(guidString, out GUID guid))
            {
                guid = GUID.Generate();
            }

            return guid;
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