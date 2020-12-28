using System;
using System.Collections.Generic;
using System.Linq;
using AsepriteImporter.EditorUtils;
using AsepriteImporter.Settings;
using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using UnityEditor;

namespace AsepriteImporter {
    [CustomEditor(typeof(AseFileImporter)), CanEditMultipleObjects]
    public class AseFileImporterEditor : ScriptedImporterEditor
    {
        private const string TEXTURE_SETTINGS_PATH = "textureImporterSettings.";
        private const string AUTO_GEMERATION_SETTINGS_PATH = "autoGenerationSettings.";

        private const string FOLDOUT_TEXTURE_ADVANCED = "textureSettingsAdvanced";
        
        private readonly string[] importTypes = {"Sprite", "Tileset (Grid)"};

        private readonly string[] spritePivotOptions =
        {
            "Center", "Top Left", "Top", "Top Right", "Left", "Right", "Bottom Left", "Bottom", "Bottom Right", "Custom"
        };

        private readonly string[] assetCreationModes = {"Generated (Subfolder)", "Texture (with Animations)"};

        private bool customSpritePivot;
        private readonly Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();

        private readonly string[] editorTabs = {"Texture", "Animation"};
        private int activeTab = 0;
        private AseFileImporter importer;

        public override void OnEnable()
        {
            base.OnEnable();
            foldoutStates.Clear();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            
            var settings = "settings.";
            var assetCreationProperty = serializedObject.FindProperty(settings + "assetCreationMode"); 
            /* Preparation for bundled importer
            assetCreationProperty.intValue = EditorGUILayout.Popup("Asset Creation Mode",
                assetCreationProperty.intValue, assetCreationModes);
            */
            EditorGUILayout.Space();
            if (assetCreationProperty.intValue == (int) AssetCreationMode.Bundled)
            {
                TextureSettings();
            }
            else
            {
                SpriteGenerationSettings();
            }

            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }

        private void TextureSettings()
        {
            importer = serializedObject.targetObject as AseFileImporter;

            if (importer.textureImporterSettings == null)
                importer.textureImporterSettings = new AseFileTextureImportSettings();

            activeTab = GUILayout.Toolbar(activeTab, editorTabs);

            switch (activeTab)
            {
                case 0:
                    DrawTextureImporterSettings();
                    break;
                case 1:
                    DrawAnimationImportSettings();
                    break;
            }
        }


        private void DrawTextureImporterSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Texture Importer Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            SerializedProperty textureType = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "textureType");
            GUI.enabled = false;
            if (CustomEnumPopup("Texture Type", textureType, TextureImporterEditorUtils.MappingTextureImportTypes))
            {
                importer.textureImporterSettings.ApplyTextureType(importer.textureImporterSettings.textureType);
            }

            GUI.enabled = true;

            SerializedProperty textureShape = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "textureShape");



            if (TextureImporterEditorUtils.textureType2DFixed.Contains(textureType.enumValueIndex))
            {
                GUI.enabled = false;
                textureShape.enumValueIndex = 0;
            }

            CustomEnumPopup("Texture Shape", textureShape, TextureImporterEditorUtils.mappingTextureShapes);
            GUI.enabled = true;

            EditorGUILayout.Space();

            if (textureType.enumValueIndex == (int) TextureImporterEditorUtils.TextureImportTypeIndex.Sprite)
            {
                DrawSpriteSettings();
            }

            DrawAdvancedSettings();

            EditorGUILayout.Space();

            SerializedProperty wrapMode = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "wrapMode");
            EditorGUILayout.PropertyField(wrapMode);

            SerializedProperty filterMode = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "filterMode");
            CustomEnumPopup("Filter Mode", filterMode, TextureImporterEditorUtils.mappingFilterMode);


            SerializedProperty aniso = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "aniso");
            if (!(Array.IndexOf(TextureImporterEditorUtils.textureTypeAnisoEnabled, textureType.enumValueIndex) != -1 &&
                  filterMode.enumValueIndex != 0))
            {
                GUI.enabled = false;
            }

            aniso.intValue = (int) EditorGUILayout.Slider("Aniso Level", aniso.intValue, 0, 16);
            GUI.enabled = true;
        }
        
        private void DrawSpriteSettings()
        {
            SerializedProperty spriteMode = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "spriteMode");
            spriteMode.intValue = EditorGUILayout.Popup("Sprite Mode", spriteMode.intValue, Enum.GetNames(typeof(SpriteImportMode)));

            ++EditorGUI.indentLevel;

            SerializedProperty pixelsPerUnit = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "spritePixelsPerUnit");
            EditorGUILayout.PropertyField(pixelsPerUnit, new GUIContent("Pixels Per Unit"));

            SerializedProperty meshType = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "spriteMeshType");
            EditorGUILayout.PropertyField(meshType, new GUIContent("Mesh Type"));

            SerializedProperty extrudeEdges = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "spriteExtrude");
            EditorGUILayout.IntSlider(extrudeEdges, 0, 32, new GUIContent("Extrude Edges"));

            SerializedProperty pivot = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "spriteAlignment");
            pivot.intValue = EditorGUILayout.Popup("Pivot", pivot.intValue, Enum.GetNames(typeof(SpriteAlignment)));

            if (pivot.intValue == (int)SpriteAlignment.Custom)
            {
                SerializedProperty spritePivot = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "spritePivot");
                EditorGUILayout.PropertyField(spritePivot, new GUIContent(" "));
            }

            SerializedProperty generatePhysics = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "spriteGenerateFallbackPhysicsShape");
            EditorGUILayout.PropertyField(generatePhysics, new GUIContent("Generate Physics Shape"));

            DrawSpriteEditorButton();

            --EditorGUI.indentLevel;
        }
        

        private void DrawAdvancedSettings()
        {
            if (!foldoutStates.ContainsKey(FOLDOUT_TEXTURE_ADVANCED))
                foldoutStates.Add(FOLDOUT_TEXTURE_ADVANCED, false);

            foldoutStates[FOLDOUT_TEXTURE_ADVANCED] =
                EditorGUILayout.Foldout(foldoutStates[FOLDOUT_TEXTURE_ADVANCED], "Advanced");
            if (foldoutStates[FOLDOUT_TEXTURE_ADVANCED])
            {
                ++EditorGUI.indentLevel;

                SerializedProperty srgbTexture = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "sRGBTexture");
                EditorGUILayout.PropertyField(srgbTexture, new GUIContent("sRGB (Color Texture)"));

                SerializedProperty alphaSource = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "alphaSource");
                CustomEnumPopup("Alpha Source", alphaSource, TextureImporterEditorUtils.mappingAlphaSource);

                if (alphaSource.enumValueIndex == 0)
                    GUI.enabled = false;
                SerializedProperty alphaIsTransparency =
                    serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "alphaIsTransparency");
                EditorGUILayout.PropertyField(alphaIsTransparency);
                GUI.enabled = true;

                SerializedProperty readable = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "readable");
                EditorGUILayout.PropertyField(readable, new GUIContent("Read/Write Enabled"));

                SerializedProperty mipmapEnabled =
                    serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipmapEnabled");
                EditorGUILayout.PropertyField(mipmapEnabled, new GUIContent("Generate Mip Maps"));

                if (mipmapEnabled.boolValue)
                {
                    ++EditorGUI.indentLevel;

                    SerializedProperty borderMipmap =
                        serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "borderMipmap");
                    EditorGUILayout.PropertyField(borderMipmap, new GUIContent("Border Mip Maps"));

                    SerializedProperty mipmapFilter =
                        serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipmapFilter");
                    CustomEnumPopup("Mip Map Filtering", mipmapFilter, TextureImporterEditorUtils.mappingMipMapFilter);

                    SerializedProperty mipMapsPreserveCoverage =
                        serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipMapsPreserveCoverage");
                    EditorGUILayout.PropertyField(mipMapsPreserveCoverage);

                    if (mipMapsPreserveCoverage.boolValue)
                    {
                        ++EditorGUI.indentLevel;

                        SerializedProperty mipmapBias =
                            serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipmapBias");
                        EditorGUILayout.PropertyField(mipmapBias, new GUIContent("Alpha Cutoff Value"));

                        --EditorGUI.indentLevel;
                    }


                    SerializedProperty fadeOut = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "fadeOut");
                    EditorGUILayout.PropertyField(fadeOut, new GUIContent("Fadeout Mip Maps"));

                    if (fadeOut.boolValue)
                    {
                        ++EditorGUI.indentLevel;

                        SerializedProperty mipmapFadeDistanceStart =
                            serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipmapFadeDistanceStart");
                        SerializedProperty mipmapFadeDistanceEnd =
                            serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipmapFadeDistanceEnd");

                        float fadeStart = mipmapFadeDistanceStart.intValue;
                        float fadeEnd = mipmapFadeDistanceEnd.intValue;

                        EditorGUILayout.MinMaxSlider("Fade Range", ref fadeStart, ref fadeEnd, 0, 10);

                        mipmapFadeDistanceStart.intValue = (int) fadeStart;
                        mipmapFadeDistanceEnd.intValue = (int) fadeEnd;

                        --EditorGUI.indentLevel;
                    }

                    --EditorGUI.indentLevel;
                }

                --EditorGUI.indentLevel;
            }
        }



        private void DrawAnimationImportSettings()
        {
            SerializedProperty animationSettingsArray = serializedObject.FindProperty("animationSettings");
            if (animationSettingsArray != null)
            {
                int arraySize = animationSettingsArray.arraySize;
                if (arraySize == 0)
                    return;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Animation Import Settings", EditorStyles.boldLabel);

                SerializedProperty generateAnimations = serializedObject.FindProperty("generateAnimations");
                EditorGUILayout.PropertyField(generateAnimations);

                SerializedProperty createAnimationAssets = serializedObject.FindProperty("createAnimationAssets");
                EditorGUILayout.PropertyField(createAnimationAssets);

                GUI.enabled = generateAnimations.boolValue;
                ++EditorGUI.indentLevel;
                for (int i = 0; i < arraySize; i++)
                {
                    DrawAnimationSetting(animationSettingsArray.GetArrayElementAtIndex(i),
                        importer.animationSettings[i]);
                }

                --EditorGUI.indentLevel;
                GUI.enabled = true;
            }
        }

        private void DrawAnimationSetting(SerializedProperty animationSettingProperty,
            AseFileAnimationSettings animationSetting)
        {
            string animationName = animationSettingProperty.FindPropertyRelative("animationName").stringValue;

            if (animationName == null)
                return;



            if (!foldoutStates.ContainsKey(animationName))
            {
                foldoutStates.Add(animationName, false);
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUI.indentLevel++;

            GUIStyle foldoutStyle = EditorStyles.foldout;
            FontStyle prevoiusFontStyle = foldoutStyle.fontStyle;
            foldoutStyle.fontStyle = FontStyle.Bold;

            var content = new GUIContent();
            content.text = animationName;

            if (animationSetting.HasInvalidSprites)
                content.image = EditorGUIUtility.IconContent("console.warnicon.sml").image;


            if (foldoutStates[animationName] = EditorGUILayout.Foldout(foldoutStates[animationName],
                content, true, foldoutStyle))
            {
                if (animationSetting.HasInvalidSprites)
                    EditorGUILayout.HelpBox(
                        $"The animation '{animationName}' will not be imported.\nSome sprites are missing.",
                        MessageType.Warning);


                EditorGUILayout.PropertyField(animationSettingProperty.FindPropertyRelative("loopTime"));
                EditorGUILayout.HelpBox(animationSettingProperty.FindPropertyRelative("about").stringValue,
                    MessageType.None);

                SerializedProperty sprites = animationSettingProperty.FindPropertyRelative("sprites");
                SerializedProperty frameNumbers = animationSettingProperty.FindPropertyRelative("frameNumbers");

                for (int i = 0; i < sprites.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(sprites.GetArrayElementAtIndex(i),
                        new GUIContent("Frame #" + frameNumbers.GetArrayElementAtIndex(i).intValue));
                }
            }

            foldoutStyle.fontStyle = prevoiusFontStyle;

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    



        private void DrawSpriteEditorButton()
         {
             Rect spriteEditorRect = EditorGUILayout.GetControlRect(false);
             Rect spriteEditorButtonRect = new Rect(spriteEditorRect.xMax - 80, spriteEditorRect.y, 80, spriteEditorRect.height);
             if (GUI.Button(spriteEditorButtonRect, "Sprite Editor"))
             {
                 if (EditorUtility.IsDirty(serializedObject.targetObject.GetInstanceID()))
                 {
                     var assetPath = (serializedObject.targetObject as ScriptedImporter).assetPath;

                     if (EditorUtility.DisplayDialog("Unapplied import settings", $"Unapplied import settings for {assetPath}.\nApply and continue to sprite editor or cancel.", "Apply", "Cancel"))
                     {
                         ApplyAndImport();
                     }
                     else
                     {
                         return;
                     }
                 }

                 EditorApplication.ExecuteMenuItem("Window/2D/Sprite Editor");

                 GUIUtility.ExitGUI();
             }

             EditorGUILayout.Space();
         }
        
        
        
        

        private void SpriteGenerationSettings()
        {
            var settings = "settings.";
            var importTypeProperty = serializedObject.FindProperty(settings + "importType");

            EditorGUILayout.LabelField("Texture Options", EditorStyles.boldLabel);
            {
                EditorGUI.indentLevel++;

                var importType = importTypeProperty.intValue;
                EditorGUI.BeginChangeCheck();
                importType = EditorGUILayout.Popup("GenerateSprites Type", importType, importTypes);
                if (EditorGUI.EndChangeCheck())
                {
                    importTypeProperty.intValue = importType;
                }

                var transparencyMode = serializedObject.FindProperty(settings + "transparencyMode");
                var transparentColor = serializedObject.FindProperty(settings + "transparentColor");

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

                EditorGUILayout.PropertyField(serializedObject.FindProperty(settings + "pixelsPerUnit"));

                if (importTypeProperty.intValue == (int) AseFileImportType.Sprite)
                {
                    PivotPopup("Pivot");
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            if (importTypeProperty.intValue == (int) AseFileImportType.Sprite)
            {
                EditorGUILayout.LabelField("Animation Options", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                var bindTypeProperty = serializedObject.FindProperty(settings + "bindType");
                var bindType = (AseAnimationBindType) bindTypeProperty.intValue;

                EditorGUI.BeginChangeCheck();
                bindType = (AseAnimationBindType) EditorGUILayout.EnumPopup("Bind Type", bindType);

                var animTypeProperty = serializedObject.FindProperty(settings + "animType");
                var animType = (AseAnimatorType) animTypeProperty.intValue;
                animType = (AseAnimatorType) EditorGUILayout.EnumPopup("Animator Type", animType);

                if (animType == AseAnimatorType.AnimatorOverrideController)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(settings + "baseAnimator"));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty(settings + "buildAtlas"));

                if (EditorGUI.EndChangeCheck())
                {
                    bindTypeProperty.intValue = (int) bindType;
                    animTypeProperty.intValue = (int) animType;
                }

                EditorGUI.indentLevel--;
            }

            if (importTypeProperty.intValue == (int) AseFileImportType.Tileset)
            {
                EditorGUILayout.LabelField("Tileset Options", EditorStyles.boldLabel);
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.PropertyField(serializedObject.FindProperty(settings + "tileSize"));
                    PivotPopup("Tile Pivot");
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(settings + "tileEmpty"),
                        new GUIContent("Empty Tile Behaviour",
                            "Behavior for empty tiles:\nKeep - Keep empty tiles\nIndex - Remove empty tiles, but still index them\nRemove - Remove empty tiles completely"));

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
        
        
        
        private bool CustomEnumPopup(string label, SerializedProperty property, Dictionary<int, string> mappings)
        {
            if (!mappings.ContainsKey(property.enumValueIndex))
            {
                Debug.LogWarning("AseFileImporterEditor: Enum Mapping is missing key");
                property.enumValueIndex = 0;
            }

            string[] names = mappings.Values.ToArray();
            int[] indices = mappings.Keys.ToArray();


            int index = Array.IndexOf(indices, property.enumValueIndex);
            EditorGUI.BeginChangeCheck();
            int indexNew = EditorGUILayout.Popup(label, index, names);
            if (EditorGUI.EndChangeCheck())
            {
                property.enumValueIndex = indices[indexNew];
                return true;
            }

            return false;
        }
    }
}
