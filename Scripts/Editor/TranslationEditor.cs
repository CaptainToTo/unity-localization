using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Ballistic.Localization;

namespace Ballistic.Localization.Editor
{
    [CustomEditor(typeof(Translation))]
    public class TranslationEditor : UnityEditor.Editor
    {
        Translation self;

        Languages languages = null;

        public override void OnInspectorGUI()
        {
            self = target as Translation;

            serializedObject.Update();

            GUILayout.Label(self.Title);
            GUILayout.Space(10f);

            if (languages == null)
            {
                var paths = AssetDatabase.FindAssets("t:Languages");
                if (paths.Length == 0)
                {
                    return;
                }
                languages = (Languages) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(paths[0]), typeof(Languages));
            }

            self.SetLanguages(languages.LanguageCount);

            foreach (var language in languages.languages)
            {
                GUILayout.Label(language.name);
                int index = languages.GetLanguageIndex(language.name);
                var newText = GUILayout.TextArea(
                    self.GetContent(index),
                    GUILayout.Height(50)
                );
                if (newText != self.GetContent(index))
                {
                    self.SetContent(index, newText);
                    EditorUtility.SetDirty(self);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
