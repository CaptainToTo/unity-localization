using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using Ballistic.Localization;

namespace Ballistic.Localization.Editor
{
    [CustomEditor(typeof(Languages))]
    public class LanguagesEditor : UnityEditor.Editor
    {
        Languages self;

        AddressableAssetSettings settings = null;
        AddressableAssetGroup group = null;

        public override void OnInspectorGUI()
        {
            self = target as Languages;

            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("languages"));

            GUILayout.Space(20f);

            GUILayout.Label("Translations");

            if (GUILayout.Button("Bake Translations"))
            {
                settings = AddressableAssetSettingsDefaultObject.Settings;
                group = settings.FindGroup("Translations");

                for (int i = 0; i < self.translationGUIDs.Count; i++)
                {
                    var entry = settings.CreateOrMoveEntry(self.translationGUIDs[i], group);
                    settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                }

                AssetDatabase.SaveAssets();
            }

            GUILayout.Space(40);

            for (int i = 0; i < self.translationGUIDs.Count; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(self.translationGUIDs[i]);
                var translation = (Translation) AssetDatabase.LoadAssetAtPath(path, typeof(Translation));
                if (translation == null)
                {
                    self.translationGUIDs.RemoveAt(i);
                    i--;
                }
                else
                {
                    EditorGUILayout.ObjectField(translation.Title, translation, typeof(Translation), true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
