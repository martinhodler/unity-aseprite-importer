using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using Aseprite;
using AsepriteImporter.Data;
using AsepriteImporter.DataProviders;
using AsepriteImporter.Editors;
using AsepriteImporter.Importers;
using AsepriteImporter.Settings;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using Object = UnityEngine.Object;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif



namespace AsepriteImporter {
    [ScriptedImporter(1, new[] {"ase", "aseprite"})]
    public class AseFileImporter : ScriptedImporter, ISpriteEditorDataProvider {
        [SerializeField] public AseFileImportSettings settings = new AseFileImportSettings();
        [SerializeField] public AseFileTextureImportSettings textureImporterSettings = new AseFileTextureImportSettings();
        [SerializeField] public AseFileAnimationSettings[] animationSettings = new AseFileAnimationSettings[0];
        
        [SerializeField] internal Texture2D texture;
        [SerializeField] internal AseFileSpriteImportData[] spriteImportData;
        [SerializeField] internal SpriteRect[] spriteRects;
        
        [SerializeField] internal int selectedImporter;

        private List<ImporterVariant> importerVariants = new List<ImporterVariant>();

        public Texture2D Texture => texture;
        public AseFileSpriteImportData[] SpriteImportData => spriteImportData;
        
        public ImporterVariant SelectedImporter
        {
            get
            {
                if (selectedImporter >= 0 && selectedImporter < importerVariants.Count)
                    return importerVariants[selectedImporter];
                else
                {
                    selectedImporter = 0;
                    return importerVariants[selectedImporter];
                }
            }
        }

        public SpriteImporter CurrentImporter
        {
            get
            {
                switch (settings.importType)
                {
                    case AseFileImportType.Sprite:
                        return SelectedImporter.SpriteImporter;
                    case AseFileImportType.Tileset:
                        return SelectedImporter.TileSetImporter;
                }

                return SelectedImporter.SpriteImporter;
            }
        }

        public string[] ImporterNames
        {
            get
            {
                string[] names = new string[importerVariants.Count];
                for (int i = 0; i < names.Length; ++i)
                    names[i] = importerVariants[i].Name;

                return names;
            }
        }

        public AseFileImporter()
        {
            var generatedImporter = new ImporterVariant("Generated (Subfolders)", new GeneratedSpriteImporter(this), new GeneratedTileImporter(this), new GeneratedImporterEditor());
            var bundledImporter = new ImporterVariant("Bundled (preview)", new BundledSpriteImporter(this), new GeneratedTileImporter(this), new BundledImporterEditor());
            
            importerVariants.Add(generatedImporter);
            importerVariants.Add(bundledImporter);
        }

        public override void OnImportAsset(AssetImportContext ctx)
        {
            texture = null;
            
            name = GetFileName(ctx.assetPath);
            AseFile file = ReadAseFile(ctx.assetPath);

            CurrentImporter.Import(ctx, file);

            CleanUp();
        }

        private void CleanUp()
        {
            bool clearSprites = CurrentImporter.Sprites == null;
            
            foreach (var animationSetting in animationSettings)
            {
                for (int i = 0; i < animationSetting.sprites.Length; ++i)
                {
                    if (clearSprites)
                    {
                        animationSetting.sprites[i] = null;
                        continue;
                    }
                    
                    if (!CurrentImporter.Sprites.Contains(animationSetting.sprites[i]))
                        animationSetting.sprites[i] = null;
                }
            }
        }

        private string GetFileName(string assetPath) {
            var parts = assetPath.Split('/');
            var filename = parts[parts.Length - 1];
            return filename.Substring(0, filename.LastIndexOf('.'));
        }
     
        private static AseFile ReadAseFile(string assetPath) {
            var fileStream = new FileStream(assetPath, FileMode.Open, FileAccess.Read);
            var aseFile = new AseFile(fileStream);
            fileStream.Close();
            return aseFile;
        }
        
        
        
        #region ISpriteEditorDataProvider implementation

        private AsepriteTextureDataProvider textureDataProvider;
        private AsepriteOutlineDataProvider outlineDataProvider;
        
        
        public SpriteRect[] GetSpriteRects()
        {
            List<SpriteRect> spriteRects = new List<SpriteRect>();

            foreach (AseFileSpriteImportData importData in SpriteImportData)
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

            this.spriteRects = spriteRects.ToArray();
            return this.spriteRects;
        }

        public void SetSpriteRects(SpriteRect[] spriteRects)
        {
            this.spriteRects = spriteRects;
        }

        public void Apply()
        {
            CurrentImporter.Apply();
        }

        public void InitSpriteEditorDataProvider()
        {
            textureDataProvider = new AsepriteTextureDataProvider(this);
            outlineDataProvider = new AsepriteOutlineDataProvider(this);
        }

        public T GetDataProvider<T>() where T : class
        {
            if (typeof(T) == typeof(ITextureDataProvider))
                return textureDataProvider as T;

            if (typeof(T) == typeof(ISpriteOutlineDataProvider))
                return outlineDataProvider as T;

            if (typeof(T) == typeof(ISpriteEditorDataProvider))
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

        public SpriteImportMode spriteImportMode => CurrentImporter.spriteImportMode;
        public float pixelsPerUnit => CurrentImporter.pixelsPerUnit;
        public Object targetObject => CurrentImporter.targetObject;

        #endregion
        
        
        private GUID ConvertStringToGUID(string guidString)
        {
            if (!GUID.TryParse(guidString, out GUID guid))
            {
                guid = GUID.Generate();
            }

            return guid;
        }
    }
}
