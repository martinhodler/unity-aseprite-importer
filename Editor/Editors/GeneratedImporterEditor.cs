using UnityEditor;
using UnityEngine;

namespace AsepriteImporter.Editors
{
    public class GeneratedImporterEditor : SpriteImporterEditor
    {
        private readonly string[] spritePivotOptions =
        {
            "Center", "Top Left", "Top", "Top Right", "Left", "Right", "Bottom Left", "Bottom", "Bottom Right", "Custom"
        };
        
        private bool customSpritePivot;
        
        protected override void OnInspectorGUI()
        {
            var settings = "settings.";

            EditorGUILayout.LabelField("Texture Options", EditorStyles.boldLabel);
            {
                EditorGUI.indentLevel++;
                var transparencyMode = SerializedObject.FindProperty(settings + "transparencyMode");
                var transparentColor = SerializedObject.FindProperty(settings + "transparentColor");

                EditorGUILayout.PropertyField(transparencyMode);
                if (transparencyMode.intValue == (int) TransparencyMode.Mask)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(transparentColor);
                    if (GUILayout.Button("Reset"))
                    {
                        transparentColor.colorValue = Color.magenta;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.PropertyField(SerializedObject.FindProperty(settings + "pixelsPerUnit"));

                if (ImportType == AseFileImportType.Sprite)
                {
                    PivotPopup("Pivot");
                }

                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space();

            if (ImportType == AseFileImportType.Sprite)
            {
                EditorGUILayout.LabelField("Animation Options", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                var bindTypeProperty = SerializedObject.FindProperty(settings + "bindType");
                var bindType = (AseAnimationBindType) bindTypeProperty.intValue;

                EditorGUI.BeginChangeCheck();
                bindType = (AseAnimationBindType) EditorGUILayout.EnumPopup("Bind Type", bindType);

                var animTypeProperty = SerializedObject.FindProperty(settings + "animType");
                var animType = (AseAnimatorType) animTypeProperty.intValue;
                animType = (AseAnimatorType) EditorGUILayout.EnumPopup("Animator Type", animType);

                if (animType == AseAnimatorType.AnimatorOverrideController)
                {
                    EditorGUILayout.PropertyField(SerializedObject.FindProperty(settings + "baseAnimator"));
                }

                EditorGUILayout.PropertyField(SerializedObject.FindProperty(settings + "buildAtlas"));

                if (EditorGUI.EndChangeCheck())
                {
                    bindTypeProperty.intValue = (int) bindType;
                    animTypeProperty.intValue = (int) animType;
                }

                EditorGUI.indentLevel--;
            }

            if (ImportType == AseFileImportType.Tileset)
            {
                EditorGUILayout.LabelField("Tileset Options", EditorStyles.boldLabel);
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(SerializedObject.FindProperty(settings + "tileSize"));
                    PivotPopup("Tile Pivot");
                    EditorGUILayout.PropertyField(SerializedObject.FindProperty(settings + "tileEmpty"),
                        new GUIContent("Empty Tile Behaviour",
                            "Behavior for empty tiles:\nKeep - Keep empty tiles\nIndex - Remove empty tiles, but still index them\nRemove - Remove empty tiles completely"));

                    // tileNameType
                    var tileNameTypeProperty = SerializedObject.FindProperty(settings + "tileNameType");
                    var tileNameType = (TileNameType) tileNameTypeProperty.enumValueIndex;

                    EditorGUI.BeginChangeCheck();
                    tileNameType = (TileNameType) EditorGUILayout.EnumPopup("TileNameType", tileNameType);
                    if (EditorGUI.EndChangeCheck())
                    {
                        tileNameTypeProperty.enumValueIndex = (int) tileNameType;
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }
        
        private void PivotPopup(string label) {
            var alignmentProperty = SerializedObject.FindProperty("settings.spriteAlignment");
            var pivotProperty = SerializedObject.FindProperty("settings.spritePivot");
            var pivot = pivotProperty.vector2Value;
            var alignment = alignmentProperty.intValue;

            EditorGUI.BeginChangeCheck();
            alignment = EditorGUILayout.Popup(label, alignment, spritePivotOptions);
            switch (alignment) {
                case 0:
                    customSpritePivot = false;
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
                case 1:
                    customSpritePivot = false;
                    pivot = new Vector2(0f, 1f);
                    break;
                case 2:
                    customSpritePivot = false;
                    pivot = new Vector2(0.5f, 1f);
                    break;
                case 3:
                    customSpritePivot = false;
                    pivot = new Vector2(1f, 1f);
                    break;
                case 4:
                    customSpritePivot = false;
                    pivot = new Vector2(0f, 0.5f);
                    break;
                case 5:
                    customSpritePivot = false;
                    pivot = new Vector2(1f, 0.5f);
                    break;
                case 6:
                    customSpritePivot = false;
                    pivot = new Vector2(0f, 0f);
                    break;
                case 7:
                    customSpritePivot = false;
                    pivot = new Vector2(0.5f, 0f);
                    break;
                case 8:
                    customSpritePivot = false;
                    pivot = new Vector2(1f, 0f);
                    break;
                default:
                    customSpritePivot = true;
                    break;
            }

            alignmentProperty.intValue = alignment;

            if (customSpritePivot) {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(SerializedObject.FindProperty("settings.spritePivot"),
                    new GUIContent(label));
                EditorGUI.indentLevel--;
            } else if (EditorGUI.EndChangeCheck() && !customSpritePivot) {
                pivotProperty.vector2Value = pivot;
            }
        }
    }
}