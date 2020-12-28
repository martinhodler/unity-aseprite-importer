using System;
using System.Collections.Generic;
using System.Linq;
using AsepriteImporter.Editors;
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
        private SpriteImporterEditor editor;
        private AseFileImporter importer;

        private int importType;
        
        protected readonly string[] importTypes = {"Sprite", "Tileset (Grid)"};

        internal AseFileImportType ImportType => (AseFileImportType)importType;

        private void ReloadEditor()
        {
            if (target is AseFileImporter fileImporter)
            {
                importer = fileImporter;
                editor = importer.SelectedImporter.Editor;
                editor.Enable(this);
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            ReloadEditor();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();


            string[] importers = importer.ImporterNames;
            int index = importer.selectedImporter;

            int newIndex = EditorGUILayout.Popup("Importer", index, importers);
            if (newIndex != index)
            {
                importer.selectedImporter = newIndex;
                ReloadEditor();
                EditorUtility.SetDirty(importer);
            }
            
            var settings = "settings.";
            var importTypeProperty = serializedObject.FindProperty(settings + "importType");
            importType = importTypeProperty.intValue;
            
            EditorGUI.BeginChangeCheck();
            importType = EditorGUILayout.Popup("GenerateSprites Type", importType, importTypes);
            if (EditorGUI.EndChangeCheck())
            {
                importTypeProperty.intValue = importType;
            }

            EditorGUILayout.Space();

            if (editor == null)
                ReloadEditor();
            
            if (editor != null)
                editor.InspectorGUI();

            serializedObject.ApplyModifiedProperties();
            ApplyRevertGUI();
        }

        

        internal void CallApplyAndImport()
        {
            ApplyAndImport();
        }
    }
}
