using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.AssetImporters;
using UnityEditor;
using System.Linq;
using System;

namespace AsepriteImporter
{
    [CustomEditor(typeof(AseFileImporter)), CanEditMultipleObjects]
    public class AseFileImporterEditor : ScriptedImporterEditor
    {
        private const int BUTTON_RESET_WIDTH = 55;
        private const string TEXTURE_SETTINGS_PATH = "textureImporterSettings.";
        private const string AUTO_GEMERATION_SETTINGS_PATH = "autoGenerationSettings.";

        private const string FOLDOUT_TEXTURE_ADVANCED = "textureSettingsAdvanced";

        private enum TextureImportTypeIndex
        {
            Default = 0,
            Image = 1,
            NormalMap = 2,
            Bump = 3,
            GUI = 4,
            Cubemap = 5,
            Reflection = 6,
            Cookie = 7,
            Advanced = 8,
            Lightmap = 9,
            Cursor = 10,
            Sprite = 11,
            HDRI = 12,
            SingleChannel = 13
        }

        private readonly Dictionary<int, string> mappingGenerationType = new Dictionary<int, string>
        {
            { 0, "Sprite" },
            //{ 1, "Tilemap (Grid)" },
        };

        private readonly Dictionary<int, string> mappingTextureImportTypes = new Dictionary<int, string>
        {
            { (int)TextureImportTypeIndex.Default, "Default" },
            { (int)TextureImportTypeIndex.NormalMap, "Normal map" },
            { (int)TextureImportTypeIndex.GUI, "Editor GUI and Legacy GUI" },
            { (int)TextureImportTypeIndex.Sprite, "Sprite (2D and UI)" },
            { (int)TextureImportTypeIndex.Cursor, "Cursor" },
            { (int)TextureImportTypeIndex.Cookie, "Cookie" },
            { (int)TextureImportTypeIndex.Lightmap, "Lightmap" },
            { (int)TextureImportTypeIndex.SingleChannel, "Single Channel" },
        };

        private readonly Dictionary<int, string> mappingTextureShapes = new Dictionary<int, string>
        {
            { 0, "2D" },
            { 1, "Cube" },
        };

        private readonly Dictionary<int, string> mappingAlphaSource = new Dictionary<int, string>
        {
            { 0, "None" },
            { 1, "Input Texture Alpha" },
            { 2, "From Gray Scale" },
        };

        private readonly Dictionary<int, string> mappingMipMapFilter = new Dictionary<int, string>
        {
            { 0, "Box" },
            { 1, "Kaiser" },
        };

        private readonly Dictionary<int, string> mappingFilterMode = new Dictionary<int, string>
        {
            { 0, "Point (no filter)" },
            { 1, "Bilinear" },
            { 2, "Trilinear" },
        };

        private readonly int[] textureType2DFixed = { 4, 5, 6, 7, 8, 9, 10, 11, 12 };
        private readonly int[] textureTypeAnisoEnabled =
        {
            (int)TextureImportTypeIndex.Default,
            (int)TextureImportTypeIndex.NormalMap
        };

        private readonly string[] editorTabs = { "Texture", "Animation" };

        private static readonly Color asepriteEditorBackground = new Color(0.3f, 0.6f, 1f, 0.1f);
        private static readonly Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();
        
        private AseFileImporter importer;
        private int activeTab = 0;


        public override void OnEnable()
        {
            base.OnEnable();
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            importer = serializedObject.targetObject as AseFileImporter;

            if (importer.textureImporterSettings == null)
                importer.textureImporterSettings = new AsepriteTextureImportSettings();

            DrawAsepriteImporterSettings();

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

            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }



        private void GenerateSprites()
        {
            if (EditorUtility.DisplayDialog(
                        "Automatic Sprite Generation",
                        "The import setting \"Sprite Mode\" and all the custom sprites made with the \"Sprite Editor\" will be replaced!" +
                        "\nDo you want to continue?",
                        "Generate", "Cancel"))
            {
                ResetValues();
                importer.GenerateAtlasTexture(true);
                serializedObject.Update();
                serializedObject.ApplyModifiedProperties();

                ApplyAndImport();
                
                
                GUIUtility.ExitGUI();
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
                    DrawAnimationSetting(animationSettingsArray.GetArrayElementAtIndex(i), importer.animationSettings[i]);
                }
                --EditorGUI.indentLevel;
                GUI.enabled = true;
            }
        }

        private void DrawAsepriteImporterSettings()
        {
            using (var scope = new EditorGUILayout.VerticalScope())
            {
                Rect backgroundRect = new Rect(0, 0, EditorGUIUtility.currentViewWidth, scope.rect.height + 8);

                EditorGUI.DrawRect(backgroundRect, asepriteEditorBackground);

                EditorGUILayout.BeginVertical();

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Aseprite Auto Import", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Auto Import automatically sets the import settings for you and creates all the sprites you need.", MessageType.None);
                EditorGUILayout.Space();

                SerializedProperty generationType = serializedObject.FindProperty(AUTO_GEMERATION_SETTINGS_PATH + "generationType");
                CustomEnumPopup("Generation Type", generationType, mappingGenerationType);
                

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("");
                if (GUILayout.Button("Auto Generate Sprites"))
                {
                    GenerateSprites();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space();
        }


        private void DrawTextureImporterSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Texture Importer Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            SerializedProperty textureType = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "textureType");
            GUI.enabled = false;
            if (CustomEnumPopup("Texture Type", textureType, mappingTextureImportTypes))
            {
                importer.textureImporterSettings.ApplyTextureType(importer.textureImporterSettings.textureType);
            }
            GUI.enabled = true;

            SerializedProperty textureShape = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "textureShape");



            if (textureType2DFixed.Contains(textureType.enumValueIndex))
            {
                GUI.enabled = false;
                textureShape.enumValueIndex = 0;
            }
            CustomEnumPopup("Texture Shape", textureShape, mappingTextureShapes);
            GUI.enabled = true;

            EditorGUILayout.Space();

            if (textureType.enumValueIndex == (int)TextureImportTypeIndex.Sprite)
            {
                DrawSpriteSettings();
            }

            DrawAdvancedSettings();

            EditorGUILayout.Space();

            SerializedProperty wrapMode = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "wrapMode");
            EditorGUILayout.PropertyField(wrapMode);

            SerializedProperty filterMode = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "filterMode");
            CustomEnumPopup("Filter Mode", filterMode, mappingFilterMode);


            SerializedProperty aniso = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "aniso");
            if (!(Array.IndexOf(textureTypeAnisoEnabled, textureType.enumValueIndex) != -1 && filterMode.enumValueIndex != 0))
            {
                GUI.enabled = false;
            }
            aniso.intValue = (int)EditorGUILayout.Slider("Aniso Level", aniso.intValue, 0, 16);
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

            foldoutStates[FOLDOUT_TEXTURE_ADVANCED] = EditorGUILayout.Foldout(foldoutStates[FOLDOUT_TEXTURE_ADVANCED], "Advanced");
            if (foldoutStates[FOLDOUT_TEXTURE_ADVANCED])
            {
                ++EditorGUI.indentLevel;

                SerializedProperty srgbTexture = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "sRGBTexture");
                EditorGUILayout.PropertyField(srgbTexture, new GUIContent("sRGB (Color Texture)"));

                SerializedProperty alphaSource = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "alphaSource");
                CustomEnumPopup("Alpha Source", alphaSource, mappingAlphaSource);

                if (alphaSource.enumValueIndex == 0)
                    GUI.enabled = false;
                SerializedProperty alphaIsTransparency = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "alphaIsTransparency");
                EditorGUILayout.PropertyField(alphaIsTransparency);
                GUI.enabled = true;

                SerializedProperty readable = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "readable");
                EditorGUILayout.PropertyField(readable, new GUIContent("Read/Write Enabled"));

                SerializedProperty mipmapEnabled = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipmapEnabled");
                EditorGUILayout.PropertyField(mipmapEnabled, new GUIContent("Generate Mip Maps"));

                if (mipmapEnabled.boolValue)
                {
                    ++EditorGUI.indentLevel;

                    SerializedProperty borderMipmap = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "borderMipmap");
                    EditorGUILayout.PropertyField(borderMipmap, new GUIContent("Border Mip Maps"));

                    SerializedProperty mipmapFilter = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipmapFilter");
                    CustomEnumPopup("Mip Map Filtering", mipmapFilter, mappingMipMapFilter);

                    SerializedProperty mipMapsPreserveCoverage = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipMapsPreserveCoverage");
                    EditorGUILayout.PropertyField(mipMapsPreserveCoverage);

                    if (mipMapsPreserveCoverage.boolValue)
                    {
                        ++EditorGUI.indentLevel;

                        SerializedProperty mipmapBias = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipmapBias");
                        EditorGUILayout.PropertyField(mipmapBias, new GUIContent("Alpha Cutoff Value"));

                        --EditorGUI.indentLevel;
                    }


                    SerializedProperty fadeOut = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "fadeOut");
                    EditorGUILayout.PropertyField(fadeOut, new GUIContent("Fadeout Mip Maps"));

                    if (fadeOut.boolValue)
                    {
                        ++EditorGUI.indentLevel;

                        SerializedProperty mipmapFadeDistanceStart = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipmapFadeDistanceStart");
                        SerializedProperty mipmapFadeDistanceEnd = serializedObject.FindProperty(TEXTURE_SETTINGS_PATH + "mipmapFadeDistanceEnd");

                        float fadeStart = mipmapFadeDistanceStart.intValue;
                        float fadeEnd = mipmapFadeDistanceEnd.intValue;

                        EditorGUILayout.MinMaxSlider("Fade Range", ref fadeStart, ref fadeEnd, 0, 10);

                        mipmapFadeDistanceStart.intValue = (int)fadeStart;
                        mipmapFadeDistanceEnd.intValue = (int)fadeEnd;

                        --EditorGUI.indentLevel;
                    }

                    --EditorGUI.indentLevel;
                }

                --EditorGUI.indentLevel;
            }
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

        protected override void Apply()
        {
            var importer = serializedObject.targetObject as AseFileImporter;
            
            if (importer.textureImporterSettings.spriteMode != (int)SpriteImportMode.Multiple)
            {
                if (importer.SpriteImportData.Length > 1)
                {
                    serializedObject.FindProperty("spriteImportData").ClearArray();
                    importer.SetSingleSpriteImportData();
                }
            }

            base.Apply();
        }

        private void DrawTransparentMaskOptions(string textureSettings)
        {
            var transparentColorMask = serializedObject.FindProperty(textureSettings + "transparentMask");
            var transparentColor = serializedObject.FindProperty(textureSettings + "transparentColor");


            EditorGUILayout.PropertyField(transparentColorMask);
            if (transparentColorMask.boolValue)
            {
                Rect colorRect = EditorGUILayout.GetControlRect(true);
                Rect colorLabelRect = new Rect(colorRect.x, colorRect.y, EditorGUIUtility.labelWidth, colorRect.height);
                Rect colorFieldRect = new Rect(colorLabelRect.xMax - 13, colorRect.y, (colorRect.width - EditorGUIUtility.labelWidth) - BUTTON_RESET_WIDTH, colorRect.height);
                Rect colorFieldResetRect = new Rect(colorRect.xMax - BUTTON_RESET_WIDTH, colorRect.y, BUTTON_RESET_WIDTH, colorRect.height);

                EditorGUI.LabelField(colorLabelRect, "Transparent Color");
                transparentColor.colorValue = EditorGUI.ColorField(colorFieldRect, transparentColor.colorValue);

                if (GUI.Button(colorFieldResetRect, "Reset"))
                {
                    transparentColor.colorValue = Color.magenta;
                }
            }
        }



        private void DrawAnimationSetting(SerializedProperty animationSettingProperty, AseFileAnimationSettings animationSetting)
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
                    EditorGUILayout.HelpBox($"The animation '{animationName}' will not be imported.\nSome sprites are missing.", MessageType.Warning);


                EditorGUILayout.PropertyField(animationSettingProperty.FindPropertyRelative("loopTime"));
                EditorGUILayout.HelpBox(animationSettingProperty.FindPropertyRelative("about").stringValue, MessageType.None);

                SerializedProperty sprites = animationSettingProperty.FindPropertyRelative("sprites");
                SerializedProperty frameNumbers = animationSettingProperty.FindPropertyRelative("frameNumbers");

                for (int i = 0; i < sprites.arraySize; i++)
                {
                    EditorGUILayout.PropertyField(sprites.GetArrayElementAtIndex(i), new GUIContent("Frame #" + frameNumbers.GetArrayElementAtIndex(i).intValue));
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
    }
}
