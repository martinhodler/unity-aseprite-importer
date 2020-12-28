using AsepriteImporter.Editors;

namespace AsepriteImporter.Importers
{
    public class ImporterVariant
    {
        public string Name { get; }
        public SpriteImporter SpriteImporter { get; }
        public SpriteImporter TileSetImporter { get; }
        public SpriteImporterEditor Editor { get; }

        public ImporterVariant(string name, SpriteImporter spriteImporter, SpriteImporter tileSetImporter, SpriteImporterEditor editor)
        {
            Name = name;
            SpriteImporter = spriteImporter;
            TileSetImporter = tileSetImporter;
            Editor = editor;
        }
    }
}