using System;
using Aseprite;
using AsepriteImporter.Data;
using AsepriteImporter.Settings;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif


namespace AsepriteImporter
{
    public abstract class SpriteImporter
    {
        private const int UPDATE_LIMIT = 300;

        private int updates;
        private AseFileImporter importer;

        protected AseFileImportSettings Settings => importer.settings;
        protected AseFileTextureImportSettings TextureImportSettings => importer.textureImporterSettings;

        protected AseFileAnimationSettings[] AnimationSettings
        {
            get => importer.animationSettings;
            set => importer.animationSettings = value;
        } 

        protected AseFileImporter Importer => importer;

        protected AssetImportContext Context { get; private set; }
        protected AseFile AsepriteFile { get; private set; }
        protected string AssetPath { get; private set; }

        protected Texture2D Texture
        {
            get => importer.texture;
            set => importer.texture = value;
        }

        protected AseFileSpriteImportData[] SpriteImportData
        {
            get => importer.spriteImportData;
            set => importer.spriteImportData = value;
        }

        protected SpriteRect[] SpriteRects
        {
            get => importer.spriteRects;
            set => importer.spriteRects = value;
        }

        protected SpriteImporter(AseFileImporter importer)
        {
            this.importer = importer;
        }

        public virtual Sprite[] Sprites { get; }
        
        public void Import(AssetImportContext ctx, AseFile file)
        {
            Context = ctx;

            AsepriteFile = file;
            AssetPath = ctx.assetPath;
            OnImport();
            
            updates = UPDATE_LIMIT;
            EditorApplication.update += OnEditorUpdate;
        }

        public abstract void OnImport();

        private void OnEditorUpdate()
        {
            AssetDatabase.Refresh();
            var done = false;
            if (OnUpdate()) {
                done = true;
            } else {
                updates--;
                if (updates <= 0) {
                    done = true;
                }
            }

            if (done) {
                EditorApplication.update -= OnEditorUpdate;
            }
        }
        
        protected virtual bool OnUpdate()
        {
            return true;
        }
        
        public virtual void Apply()
        {
            throw new NotImplementedException();
        }

        public virtual SpriteImportMode spriteImportMode { get; }
        public virtual float pixelsPerUnit { get; }
        public virtual Object targetObject { get; }
    }
}