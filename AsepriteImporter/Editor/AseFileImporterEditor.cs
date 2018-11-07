using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;

namespace AsepriteImporter
{
    [CustomEditor(typeof(AseFileImporter))]
    public class AseFileImporterEditor : ScriptedImporterEditor
    {

        private string[] importTypes = new string[] {"Sprite", "Tileset (Grid)"};

        private string[] spritePivotOptions = new string[]
        {
            "Center", "Top Left", "Top", "Top Right", "Left", "Right", "Bottom Left", "Bottom", "Bottom Right", "Custom"
        };

        private bool customSpritePivot = false;
        private Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        public override void OnEnable()
        {
            base.OnEnable();
            foldoutStates.Clear();
        }


        public override void OnInspectorGUI()
        {
            var importer = serializedObject.targetObject as AseFileImporter;
            var textureSettings = "textureSettings.";

            var importTypeProperty = serializedObject.FindProperty("importType");

            EditorGUILayout.LabelField("Texture Options", EditorStyles.boldLabel);
            {
                EditorGUI.indentLevel++;

                var importType = importTypeProperty.intValue;
                EditorGUI.BeginChangeCheck();
                importType = EditorGUILayout.Popup("Import Type", importType, importTypes);
                if (EditorGUI.EndChangeCheck())
                {
                    importTypeProperty.intValue = importType;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(textureSettings + "pixelsPerUnit"));

                var meshTypeProperty = serializedObject.FindProperty(textureSettings + "meshType");
                var meshType = (SpriteMeshType) meshTypeProperty.intValue;

                EditorGUI.BeginChangeCheck();
                meshType = (SpriteMeshType) EditorGUILayout.EnumPopup("Mesh Type", meshType);
                if (EditorGUI.EndChangeCheck())
                {
                    meshTypeProperty.intValue = (int) meshType;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(textureSettings + "extrudeEdges"));

                if (importTypeProperty.intValue == (int) AseFileImportType.Sprite)
                {
                    PivotPopup("Pivot");
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(textureSettings + "generatePhysics"));


                EditorGUILayout.Space();

                importer.textureSettings.wrapMode =
                    (TextureWrapMode) EditorGUILayout.EnumPopup("Wrap Mode", importer.textureSettings.wrapMode);
                importer.textureSettings.filterMode =
                    (FilterMode) EditorGUILayout.EnumPopup("Filter Mode", importer.textureSettings.filterMode);

                EditorGUI.indentLevel--;
            }



            EditorGUILayout.Space();

            if (importer.animationSettings.Length > 0)
            {
                EditorGUILayout.LabelField("Animation Options", EditorStyles.boldLabel);
                {
                    if (importer.animationSettings != null)
                    {
                        foreach (var animationSetting in importer.animationSettings)
                        {
                            DrawAnimationSetting(importer, animationSetting);
                        }
                    }
                }
            }

            if (importTypeProperty.intValue == (int) AseFileImportType.Tileset)
            {
                EditorGUILayout.LabelField("Tileset Options", EditorStyles.boldLabel);
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(textureSettings + "tileSize"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(textureSettings + "tilePadding"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(textureSettings + "tileOffset"));
                    PivotPopup("Tile Pivot");

                    EditorGUI.indentLevel--;
                }
            }

            base.ApplyRevertGUI();
        }


        private void PivotPopup(string label)
        {
            var pivotProperty = serializedObject.FindProperty("textureSettings.spritePivot");
            var pivot = pivotProperty.vector2Value;

            EditorGUI.BeginChangeCheck();
            switch (EditorGUILayout.Popup(label, GetSpritePivotOptionIndex(pivot), spritePivotOptions))
            {
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

            if (customSpritePivot)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("textureSettings.spritePivot"),
                    new GUIContent(label));
                EditorGUI.indentLevel--;
            }
            else if (EditorGUI.EndChangeCheck() && !customSpritePivot)
            {
                pivotProperty.vector2Value = pivot;
            }
        }


        private void DrawAnimationSetting(AseFileImporter importer, AseFileAnimationSettings setting)
        {
            if (setting.animationName == null)
                return;

            if (!foldoutStates.ContainsKey(setting.animationName))
            {
                foldoutStates.Add(setting.animationName, false);
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            GUIStyle foldoutStyle = EditorStyles.foldout;
            FontStyle prevoiusFontStyle = foldoutStyle.fontStyle;
            foldoutStyle.fontStyle = FontStyle.Bold;

            if (foldoutStates[setting.animationName] = EditorGUILayout.Foldout(foldoutStates[setting.animationName],
                setting.animationName, true, foldoutStyle))
            {
                setting.loopTime = EditorGUILayout.Toggle("Loop", setting.loopTime);
                EditorGUILayout.HelpBox(setting.about, MessageType.None);

            }

            foldoutStyle.fontStyle = prevoiusFontStyle;

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private int GetSpritePivotOptionIndex(Vector2 spritePivot)
        {
            if (customSpritePivot)
                return spritePivotOptions.Length - 1;

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
