using Aseprite;
using AsepriteImporter.Settings;
using UnityEditor;

namespace AsepriteImporter
{
    public abstract class AsepriteImporter
    {
        private const int UPDATE_LIMIT = 300;

        private int updates;
        private AseFileImporter importer;

        protected AseFileImportSettings Settings => importer.settings;
        protected AseFileTextureImportSettings TextureImportSettings => importer.textureImporterSettings;
        protected AseFileAnimationSettings[] AnimationSettings => importer.animationSettings;
        
        protected AseFile AsepriteFile { get; private set; }
        protected string AssetPath { get; private set; }
        
        public void Import(string path, AseFile file, AseFileImporter importer)
        {
            this.importer = importer;

            AsepriteFile = file;
            AssetPath = path;
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
    }
}