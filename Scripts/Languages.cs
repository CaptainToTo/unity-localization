using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace Ballistic.Localization
{
    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    [System.Serializable]
    public struct Language
    {
        [Tooltip("Unique key for the language.")]
        public string name;
        [Tooltip("Font asset text should use when the language is selected.")]
        public TMP_FontAsset font;
    }

    [CreateAssetMenu(fileName = "Languages", menuName = "Languages")]
    public class Languages : ScriptableObject
    {
        // * Languages =================================

        // serialized list of languages, order is used for distinguishing translations
        public Language[] languages;

        public int LanguageCount {
            get { return languages.Length; }
        }
        
        // dict created at runtime from array above
        private Dictionary<string, (int index, Language lang)> langDict = null;

        /// <summary>
        /// Get a language using its name. If no language is found, returns the first language.
        /// </summary>
        public Language Get(string name)
        {
            // build language dict if it hasn't been
            if (langDict == null)
            {
                langDict = new Dictionary<string, (int, Language)>();
                for (int i = 0; i < languages.Length; i++)
                {
                    langDict[languages[i].name] = (i, languages[i]);
                }
            }

            if (!langDict.ContainsKey(name))
            {
                return languages[0];
            }

            return langDict[name].lang;
        }

        /// <summary>
        /// Get the translation index for the language with the matching name. 
        /// If no language is found, returns 0.
        /// </summary>
        public int GetLanguageIndex(string name)
        {
            if (Application.isEditor)
            {
                for (int i = 0; i < languages.Length; i++) 
                {
                    if (languages[i].name == name)
                    {
                        return i;
                    }
                }
            }

            if (langDict == null)
            {
                langDict = new Dictionary<string, (int, Language)>();
                for (int i = 0; i < languages.Length; i++)
                {
                    langDict[languages[i].name] = (i, languages[i]);
                }
            }

            if (!langDict.ContainsKey(name))
            {
                return 0;
            }

            return langDict[name].index;
        }

        // * ============================================

        // * Translations ===============================

        public List<string> translationGUIDs = new List<string>();

        public void AddTranslation(string guid)
        {
            translationGUIDs.Add(guid);
        }

        /// <summary>
        /// Gets a translation by title. ONLY USABLE IN EDITOR! DO NOT USE 
        /// IN GAME LOGIC.
        /// </summary>
        public Translation GetTranslation(string title)
        {
            #if UNITY_EDITOR
            foreach (var guid in translationGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var translation = (Translation) AssetDatabase.LoadAssetAtPath(path, typeof(Translation));
                if (translation.Title == title)
                {
                    return translation;
                }
            }
            #endif
            return null;
        }

        /// <summary>
        /// Removes a translation by title. ONLY USABLE IN EDITOR! DO NOT USE 
        /// IN GAME LOGIC.
        /// </summary>
        public void RemoveTranslation(string title)
        {
            #if UNITY_EDITOR
            for (int i = 0; i < translationGUIDs.Count; i++) 
            {
                var path = AssetDatabase.GUIDToAssetPath(translationGUIDs[i]);
                var translation = (Translation) AssetDatabase.LoadAssetAtPath(path, typeof(Translation));
                if (translation.Title == title)
                {
                    translationGUIDs.RemoveAt(i);
                    i--;
                }
            }
            #endif
        }

        /// <summary>
        /// Returns true if the translation exists. ONLY USABLE IN EDITOR! DO NOT USE 
        /// IN GAME LOGIC.
        /// </summary>
        public bool HasTranslation(string title)
        {
            #if UNITY_EDITOR
            if (title == null || title == "")
            {
                return false;
            }

            foreach (var guid in translationGUIDs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var translation = (Translation) AssetDatabase.LoadAssetAtPath(path, typeof(Translation));
                if (translation?.Title == title)
                {
                    return true;
                }
            }
            #endif
            return false;
        }

        // * ============================================
    }
}
