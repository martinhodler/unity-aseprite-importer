using System.Collections.Generic;
using UnityEditor;

public static class AseSpritePostProcess {
    public static Dictionary<string, SerializedProperty> GetPhysicsShapeProperties(TextureImporter importer,
                                                                  List<SpriteMetaData> metaList) {
        SerializedObject serializedImporter = new SerializedObject(importer);
        var property = serializedImporter.FindProperty("m_SpriteSheet.m_Sprites");
        var res = new Dictionary<string, SerializedProperty>();
        var removed = new HashSet<int>();

        for (int index = 0; index < property.arraySize; index++) {
            var name = importer.spritesheet[index].name;
            if (res.ContainsKey(name)) {
                continue;
            }

            var element = property.GetArrayElementAtIndex(index);
            var physicsShape = element.FindPropertyRelative("m_PhysicsShape");

            res.Add(name, physicsShape);
            removed.Add(index);
        }

        return res;
    }

    public static void RecoverPhysicsShapeProperty(
        Dictionary<string, SerializedProperty> newProperties,
        Dictionary<string, SerializedProperty> oldProperties) {

        SerializedProperty property = null;
        foreach (var item in newProperties) {
            if (!oldProperties.TryGetValue(item.Key, out var oldItem)) {
                continue;
            }

            var newItem = item.Value;
            if (oldItem.arraySize > 0) {
                newItem.arraySize = oldItem.arraySize;

                for (int index = 0; index < newItem.arraySize; index++) {
                    var newShape = newItem.GetArrayElementAtIndex(index);
                    var oldShape = oldItem.GetArrayElementAtIndex(index);
                    newShape.arraySize = oldShape.arraySize;

                    for (int pi = 0; pi < newShape.arraySize; pi++) {
                        var newPt = newShape.GetArrayElementAtIndex(pi);
                        var oldPt = oldShape.GetArrayElementAtIndex(pi);
                        newPt.vector2Value = oldPt.vector2Value;
                    }
                }
                
                if (property == null)
                    property = newItem;
            }
        }

        property?.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
}