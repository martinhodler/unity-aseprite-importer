using System.Collections.Generic;
using UnityEditor;

public static class AseSpritePostProcess {
    public static List<SerializedProperty> GetPhysicsShapeProperties(TextureImporter importer, 
                                                                     List<SpriteMetaData> metaList) {
        SerializedObject serializedImporter = new SerializedObject(importer);
        var spriteSheetSP = serializedImporter.FindProperty("m_SpriteSheet.m_Sprites");
        var defaultPhysicsShape = serializedImporter.FindProperty("m_SpriteSheet.m_PhysicsShape");
        var physicsShapeMap = new Dictionary<string, SerializedProperty>();
        
        for (int i = 0; i < spriteSheetSP.arraySize; i++) {
            var name = importer.spritesheet[i].name;
            var element = spriteSheetSP.GetArrayElementAtIndex(i);
            physicsShapeMap.Add(name, element.FindPropertyRelative("m_PhysicsShape"));
        }
        
        var properties = new List<SerializedProperty>();
        foreach (var meta in metaList) {
            physicsShapeMap.TryGetValue(meta.name, out SerializedProperty property);
            properties.Add(property);
        }

        return properties;
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