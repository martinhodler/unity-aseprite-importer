using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;

namespace AseImporter {
    [CustomEditor(typeof(AseFileImporter)), CanEditMultipleObjects]
    public class AseFileImporterEditor : ScriptedImporterEditor {
        private string[] importTypes = {"Sprite", "Tileset (Grid)"};
        private string[] animTypes = {"None", "Animator Controller", "Animator Override Controller"};

        private string[] spritePivotOptions = {
            "Center", "Top Left", "Top", "Top Right", "Left", "Right", "Bottom Left", "Bottom", "Bottom Right", "Custom"
        };

        private bool customSpritePivot;
        private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        public override void OnEnable() {
            base.OnEnable();
            foldoutStates.Clear();
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            var settings = "settings.";
            var importTypeProperty = serializedObject.FindProperty(settings + "importType");

            EditorGUILayout.LabelField("Texture Options", EditorStyles.boldLabel);
            {
                EditorGUI.indentLevel++;

                var importType = importTypeProperty.intValue;
                EditorGUI.BeginChangeCheck();
                importType = EditorGUILayout.Popup("GenerateSprites Type", importType, importTypes);
                if (EditorGUI.EndChangeCheck()) {
                    importTypeProperty.intValue = importType;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(settings + "pixelsPerUnit"));

                if (importTypeProperty.intValue == (int) AseFileImportType.Sprite) {
                    PivotPopup("Pivot");
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            if (importTypeProperty.intValue == (int) AseFileImportType.Sprite) {
                EditorGUI.indentLevel++;
                var bindTypeProperty = serializedObject.FindProperty(settings + "bindType");
                var bindType = (AseEditorBindType) bindTypeProperty.intValue;

                EditorGUI.BeginChangeCheck();
                bindType = (AseEditorBindType) EditorGUILayout.EnumPopup("Bind Type", bindType);
                if (EditorGUI.EndChangeCheck()) {
                    bindTypeProperty.intValue = (int) bindType;
                }
                
                var animTypeProperty = serializedObject.FindProperty(settings + "animType");
                var animType = animTypeProperty.intValue;
                EditorGUI.BeginChangeCheck();
                animType = EditorGUILayout.Popup("Animator Type", animType, animTypes);
                if (EditorGUI.EndChangeCheck()) {
                    animTypeProperty.intValue = animType;
                }

                if (animType == (int)AseAnimatorType.AnimatorOverrideController) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(settings + "baseAnimator"));
                }

                EditorGUI.indentLevel--;
            }
            
            if (importTypeProperty.intValue == (int) AseFileImportType.Tileset) {
                EditorGUILayout.LabelField("Tileset Options", EditorStyles.boldLabel);
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(settings + "tileSize"));
                    PivotPopup("Tile Pivot");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(settings + "tileEmpty"), new GUIContent("Empty Tile Behaviour", "Behavior for empty tiles:\nKeep - Keep empty tiles\nIndex - Remove empty tiles, but still index them\nRemove - Remove empty tiles completely"));

                    // tileNameType
                    var tileNameTypeProperty = serializedObject.FindProperty(settings + "tileNameType");
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

            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }

        private void PivotPopup(string label) {
            var alignmentProperty = serializedObject.FindProperty("settings.spriteAlignment");
            var pivotProperty = serializedObject.FindProperty("settings.spritePivot");
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
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settings.spritePivot"),
                    new GUIContent(label));
                EditorGUI.indentLevel--;
            } else if (EditorGUI.EndChangeCheck() && !customSpritePivot) {
                pivotProperty.vector2Value = pivot;
            }
        }

        private int GetSpritePivotOptionIndex(Vector2 spritePivot) {
            if (customSpritePivot) {
                return spritePivotOptions.Length - 1;
            }

            if (spritePivot.x == 0.5f && spritePivot.y == 0.5f) return 0;
            if (spritePivot.x == 0f && spritePivot.y == 1f) return 1;
            if (spritePivot.x == 0.5f && spritePivot.y == 1f) return 2;
            if (spritePivot.x == 1f && spritePivot.y == 1f) return 3;
            if (spritePivot.x == 0f && spritePivot.y == 0.5f) return 4;
            if (spritePivot.x == 1f && spritePivot.y == 0.5f) return 5;
            if (spritePivot.x == 0f && spritePivot.y == 0f) return 6;
            if (spritePivot.x == 0.5f && spritePivot.y == 0f) return 7;
            if (spritePivot.x == 1f && spritePivot.y == 0f) return 8;

            return spritePivotOptions.Length - 1; // Last one = custom
        }
    }
}
