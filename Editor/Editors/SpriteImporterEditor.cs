using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif

namespace AsepriteImporter.Editors
{
    public class SpriteImporterEditor
    {
        protected const string SettingsPath = "settings.";
        protected const string TextureSettingsPath = "textureImporterSettings.";
        protected const string AnimationSettingsPath = "animationSettings.";
        
        private AseFileImporterEditor baseEditor;
        
        protected readonly Dictionary<string, bool> foldoutStates = new Dictionary<string, bool>();
        private AseFileImporter importer;

        public AseFileImporter Importer => importer;
        protected AseFileImportType ImportType => baseEditor.ImportType;
        protected SerializedObject SerializedObject => baseEditor.serializedObject;

        internal void Enable(AseFileImporterEditor importerEditor)
        {
            foldoutStates.Clear();
            baseEditor = importerEditor;
            
            OnEnable();
        }

        internal void Disable()
        {
            OnDisable();
        }

        internal void InspectorGUI()
        {
            importer = SerializedObject.targetObject as AseFileImporter;
            OnInspectorGUI();
        }

        protected void ApplyAndImport()
        {
            baseEditor.CallApplyAndImport();
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }
        
        protected virtual void OnInspectorGUI() 
        {
        }
        
        
        protected bool CustomEnumPopup(string label, SerializedProperty property, Dictionary<int, string> mappings)
        {
            if (!mappings.ContainsKey(property.enumValueIndex))
            {
                Debug.LogWarning("AsepriteImporterEditor: Enum Mapping is missing key");
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