using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using System.IO;
using Aseprite;
using AsepriteImporter.Importers;
using AsepriteImporter.Settings;

namespace AsepriteImporter {
    [ScriptedImporter(1, new[] {"ase", "aseprite"})]
    public class AseFileImporter : ScriptedImporter {
        [SerializeField] public AseFileImportSettings settings = new AseFileImportSettings();
        [SerializeField] public AseFileTextureImportSettings textureImporterSettings = new AseFileTextureImportSettings();
        [SerializeField] public AseFileAnimationSettings[] animationSettings = new AseFileAnimationSettings[0];

        private AseTileImporter tileImporter = new AseTileImporter();
        private AseSpriteImporter spriteImporter = new AseSpriteImporter();

        public override void OnImportAsset(AssetImportContext ctx) {
            name = GetFileName(ctx.assetPath);
            AseFile file = ReadAseFile(ctx.assetPath);

            if (settings.importType == AseFileImportType.Tileset) {
                tileImporter.Import(ctx.assetPath, file, this);
            } else {
                spriteImporter.Import(ctx.assetPath, file, this);
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
    }
}
