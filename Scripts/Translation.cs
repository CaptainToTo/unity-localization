using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballistic.Localization
{
    [System.Serializable]
    public class Translation : ScriptableObject
    {
        [SerializeField] private string title;

        public string Title { 
            get { return title; } 
            set { title = value; }
        }

        [SerializeField] private List<string> translations;

        public void SetLanguages(int size)
        {
            if (translations == null)
            {
                translations = new List<string>();
            }

            while (translations.Count < size)
            {
                translations.Add("");
            }
        }

        public void SetContent(int langIndex, string content)
        {
            translations[Mathf.Clamp(langIndex, 0, translations.Count)] = content;
        }

        public string GetContent(int langIndex)
        {
            if (translations == null || Mathf.Clamp(langIndex, 0, translations.Count) != langIndex)
            {
                Debug.LogError($"Translation for '{Title}' failed to fetch value. Don't worry about it just tell Tony.");
                return "";
            }
            return translations[Mathf.Clamp(langIndex, 0, translations.Count)];
        }
    }
}

