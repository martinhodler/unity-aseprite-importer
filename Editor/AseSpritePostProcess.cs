using System.Collections.Generic;
using UnityEditor;

public static class AseSpritePostProcess {
    public static List<SerializedProperty> GetPhysicsShapeProperties(TextureImporter importer, 
                                                                     List<SpriteMetaData> metaList) {
        var properties = new List<SerializedProperty>();
        foreach (var meta in metaList) {
            var property = GetPhysicsShapeProperty(importer, meta.name);
            if (property != null) {
                properties.Add(property);
            }
        }

        return properties;
    }
    
    public static SerializedProperty GetPhysicsShapeProperty(TextureImporter importer, string spriteName) {
        SerializedObject serializedImporter = new SerializedObject(importer);
 
        if (importer.spriteImportMode == SpriteImportMode.Multiple) {
            var spriteSheetSP = serializedImporter.FindProperty("m_SpriteSheet.m_Sprites");
 
            for (int i = 0; i < spriteSheetSP.arraySize; i++) {
                if (importer.spritesheet[i].name == spriteName) {
                    var element = spriteSheetSP.GetArrayElementAtIndex(i);
                    return element.FindPropertyRelative("m_PhysicsShape");
                }
            }
 
        }
 
        return serializedImporter.FindProperty("m_SpriteSheet.m_PhysicsShape");
    }

    public static void RecoverPhysicsShapeProperty(List<SerializedProperty> properties) {
        foreach (var property in properties) {
            // make it dirty
            property.InsertArrayElementAtIndex(0);
            property.DeleteArrayElementAtIndex(0);
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}