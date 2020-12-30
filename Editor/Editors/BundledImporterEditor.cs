using System;
using System.Linq;
using AsepriteImporter.EditorUtils;
using AsepriteImporter.Settings;
using UnityEditor;
using UnityEngine;

namespace AsepriteImporter.Editors
{
    public class BundledImporterEditor : SpriteImporterEditor
    {
        private const string FoldoutTextureAdvanced = "textureSettingsAdvanced";
        
        private readonly string[] editorTabs = {"Texture", "Animation"};
        private int activeTab = 0;
        
        protected override void OnInspectorGUI()
        {
            if (Importer.textureImporterSettings == null)
                Importer.textureImporterSettings = new AseFileTextureImportSettings();

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

            SerializedProperty textureType = SerializedObject.FindProperty(TextureSettingsPath + "textureType");
            GUI.enabled = false;
            if (CustomEnumPopup("Texture Type", textureType, TextureImporterEditorUtils.MappingTextureImportTypes))
            {
                Importer.textureImporterSettings.ApplyTextureType(Importer.textureImporterSettings.textureType);
            }

            GUI.enabled = true;

            SerializedProperty textureShape = SerializedObject.FindProperty(TextureSettingsPath + "textureShape");



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

            SerializedProperty wrapMode = SerializedObject.FindProperty(TextureSettingsPath + "wrapMode");
            EditorGUILayout.PropertyField(wrapMode);

            SerializedProperty filterMode = SerializedObject.FindProperty(TextureSettingsPath + "filterMode");
            CustomEnumPopup("Filter Mode", filterMode, TextureImporterEditorUtils.mappingFilterMode);


            SerializedProperty aniso = SerializedObject.FindProperty(TextureSettingsPath + "aniso");
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
            SerializedProperty spriteMode = SerializedObject.FindProperty(TextureSettingsPath + "spriteMode");
            spriteMode.intValue = EditorGUILayout.Popup("Sprite Mode", spriteMode.intValue, Enum.GetNames(typeof(SpriteImportMode)));

            ++EditorGUI.indentLevel;

            SerializedProperty pixelsPerUnit = SerializedObject.FindProperty(TextureSettingsPath + "spritePixelsPerUnit");
            EditorGUILayout.PropertyField(pixelsPerUnit, new GUIContent("Pixels Per Unit"));

            SerializedProperty meshType = SerializedObject.FindProperty(TextureSettingsPath + "spriteMeshType");
            EditorGUILayout.PropertyField(meshType, new GUIContent("Mesh Type"));

            SerializedProperty extrudeEdges = SerializedObject.FindProperty(TextureSettingsPath + "spriteExtrude");
            EditorGUILayout.IntSlider(extrudeEdges, 0, 32, new GUIContent("Extrude Edges"));

            SerializedProperty pivot = SerializedObject.FindProperty(TextureSettingsPath + "spriteAlignment");
            pivot.intValue = EditorGUILayout.Popup("Pivot", pivot.intValue, Enum.GetNames(typeof(SpriteAlignment)));

            if (pivot.intValue == (int)SpriteAlignment.Custom)
            {
                SerializedProperty spritePivot = SerializedObject.FindProperty(TextureSettingsPath + "spritePivot");
                EditorGUILayout.PropertyField(spritePivot, new GUIContent(" "));
            }

            SerializedProperty generatePhysics = SerializedObject.FindProperty(TextureSettingsPath + "spriteGenerateFallbackPhysicsShape");
            EditorGUILayout.PropertyField(generatePhysics, new GUIContent("Generate Physics Shape"));

            DrawSpriteEditorButton();

            --EditorGUI.indentLevel;
        }
        

        private void DrawAdvancedSettings()
        {
            if (!foldoutStates.ContainsKey(FoldoutTextureAdvanced))
                foldoutStates.Add(FoldoutTextureAdvanced, false);

            foldoutStates[FoldoutTextureAdvanced] =
                EditorGUILayout.Foldout(foldoutStates[FoldoutTextureAdvanced], "Advanced");
            if (foldoutStates[FoldoutTextureAdvanced])
            {
                ++EditorGUI.indentLevel;

                SerializedProperty srgbTexture = SerializedObject.FindProperty(TextureSettingsPath + "sRGBTexture");
                EditorGUILayout.PropertyField(srgbTexture, new GUIContent("sRGB (Color Texture)"));

                SerializedProperty alphaSource = SerializedObject.FindProperty(TextureSettingsPath + "alphaSource");
                CustomEnumPopup("Alpha Source", alphaSource, TextureImporterEditorUtils.mappingAlphaSource);

                if (alphaSource.enumValueIndex == 0)
                    GUI.enabled = false;
                SerializedProperty alphaIsTransparency =
                    SerializedObject.FindProperty(TextureSettingsPath + "alphaIsTransparency");
                EditorGUILayout.PropertyField(alphaIsTransparency);
                GUI.enabled = true;

                SerializedProperty readable = SerializedObject.FindProperty(TextureSettingsPath + "readable");
                EditorGUILayout.PropertyField(readable, new GUIContent("Read/Write Enabled"));

                SerializedProperty mipmapEnabled =
                    SerializedObject.FindProperty(TextureSettingsPath + "mipmapEnabled");
                EditorGUILayout.PropertyField(mipmapEnabled, new GUIContent("Generate Mip Maps"));

                if (mipmapEnabled.boolValue)
                {
                    ++EditorGUI.indentLevel;

                    SerializedProperty borderMipmap =
                        SerializedObject.FindProperty(TextureSettingsPath + "borderMipmap");
                    EditorGUILayout.PropertyField(borderMipmap, new GUIContent("Border Mip Maps"));

                    SerializedProperty mipmapFilter =
                        SerializedObject.FindProperty(TextureSettingsPath + "mipmapFilter");
                    CustomEnumPopup("Mip Map Filtering", mipmapFilter, TextureImporterEditorUtils.mappingMipMapFilter);

                    SerializedProperty mipMapsPreserveCoverage =
                        SerializedObject.FindProperty(TextureSettingsPath + "mipMapsPreserveCoverage");
                    EditorGUILayout.PropertyField(mipMapsPreserveCoverage);

                    if (mipMapsPreserveCoverage.boolValue)
                    {
                        ++EditorGUI.indentLevel;

                        SerializedProperty mipmapBias =
                            SerializedObject.FindProperty(TextureSettingsPath + "mipmapBias");
                        EditorGUILayout.PropertyField(mipmapBias, new GUIContent("Alpha Cutoff Value"));

                        --EditorGUI.indentLevel;
                    }


                    SerializedProperty fadeOut = SerializedObject.FindProperty(TextureSettingsPath + "fadeOut");
                    EditorGUILayout.PropertyField(fadeOut, new GUIContent("Fadeout Mip Maps"));

                    if (fadeOut.boolValue)
                    {
                        ++EditorGUI.indentLevel;

                        SerializedProperty mipmapFadeDistanceStart =
                            SerializedObject.FindProperty(TextureSettingsPath + "mipmapFadeDistanceStart");
                        SerializedProperty mipmapFadeDistanceEnd =
                            SerializedObject.FindProperty(TextureSettingsPath + "mipmapFadeDistanceEnd");

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
            SerializedProperty animationSettingsArray = SerializedObject.FindProperty("animationSettings");
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Import Settings", EditorStyles.boldLabel);
            
            if (animationSettingsArray != null)
            {
                int arraySize = animationSettingsArray.arraySize;
                if (arraySize == 0)
                {
                    EditorGUILayout.HelpBox($"\"{Importer.name}\" does not contain any animation", MessageType.None);
                    return;
                }

                SerializedProperty generateAnimations = SerializedObject.FindProperty("settings.generateAnimations");
                EditorGUILayout.PropertyField(generateAnimations);

                /* Not implemented yet
                SerializedProperty createAnimationAssets = SerializedObject.FindProperty("settings.createAnimationAssets");
                EditorGUILayout.PropertyField(createAnimationAssets);
                */
                
                GUI.enabled = generateAnimations.boolValue;
                ++EditorGUI.indentLevel;
                for (int i = 0; i < arraySize; i++)
                {
                    DrawAnimationSetting(animationSettingsArray.GetArrayElementAtIndex(i),
                        Importer.animationSettings[i]);
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
                 if (EditorUtility.IsDirty(SerializedObject.targetObject.GetInstanceID()))
                 {
                     var assetPath = (SerializedObject.targetObject as AseFileImporter).assetPath;

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