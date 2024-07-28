using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Ballistic.Localization
{
    /// <summary>
    /// Use instead of the normal string class. Will provide serialized 
    /// fields for inputting its values for each language. Can otherwise be 
    /// treated as a normal string in code.
    /// </summary>
    [System.Serializable]
    public class LocalizedString
    {
        [SerializeField] private string _title;

        private string text;

        private int languageIndex = -1;

        /// <summary>
        /// The language index (unique ID) this string is currently localized to.
        /// Is updated when UpdateLanguage() is called.
        /// </summary>
        public int LanguageIndex {
            get { return languageIndex; }
        }

        [SerializeField] private string translationGUID = null;

        /// <summary>
        /// Returns true if this text has been given a value. 
        /// If it does not, then accessing its text value will cause errors.
        /// </summary>
        public bool HasTranslation {
            get { return (translationGUID != null && translationGUID != "") || (IsRunTimeStr && !IsBaseStr); }
        }

        private AssetReference translationAsset;

        public LocalizedString()
        {
            if (!Application.isEditor)
            {
                translationAsset = new AssetReference(translationGUID);
                // translationAsset.LoadAssetAsync<Translation>()
                //     .Completed += OnTranslationLoaded;
            }
        }

        /// <summary>
        /// Updates the string value using its serialized translations.
        /// DO NOT CALL NORMALLY.
        /// </summary>
        public void UpdateLanguage(Language language, int index)
        {
            if (!IsRunTimeStr)
            {
                translationAsset = new AssetReference(translationGUID);
                translationAsset.LoadAssetAsync<Translation>()
                    .Completed += OnTranslationLoaded;
            }
            else if (components != null)
            {
                foreach (var component in components)
                {
                    component.UpdateLanguage(language, index);
                }
            }
            else
            {
                // this is a runtime string with no translation
                OnChanged?.Invoke(this);
            }
        }

        public delegate void OnChangedEvent(LocalizedString text);

        public event OnChangedEvent OnChanged;

        void OnTranslationLoaded(AsyncOperationHandle<Translation> handle)
        {
            if (handle.Status == AsyncOperationStatus.Succeeded && translationAsset.IsValid())
            {
                var index = LanguageController.Instance != null ? LanguageController.Instance.LanguageIndex : 0;
                text = handle.Result.GetContent(index);
                languageIndex = index;
                translationAsset.ReleaseAsset();
                OnChanged?.Invoke(this);
            }
            else if (handle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogWarning(@"Translation loading failed. Ensure translation files 
                    are marked as addressable by clicking the 'Bake Translations' button 
                    on 'Assets/Localization/Languages.asset'.");
            }
        }

        // * Run-time concat strings ==========================

        /// <summary>
        /// Returns true if this string was created during runtime, and isn't 
        /// directly attached to any translation.
        /// </summary>
        public bool IsRunTimeStr {
            get { return translationGUID == null || translationGUID == ""; }
        }

        /// <summary>
        /// Returns true if this string isn't composed of multiple other 
        /// Localized strings. This means it's either a runtime string that was converted 
        /// into a localized string for concatenation, or is a serialized translation string.
        /// </summary>
        public bool IsBaseStr {
            get { return components == null; }
        }

        [System.NonSerialized] private List<LocalizedString> components = null;

        public LocalizedString(LocalizedString x, LocalizedString y)
        {
            components = new List<LocalizedString>();
            if (x.IsBaseStr)
            {
                components.Add(x);
                x.OnChanged += UpdateString;
            }
            else
            {
                foreach (var str in x.components)
                {
                    components.Add(str);
                    str.OnChanged += UpdateString;
                }
            }
            if (y.IsBaseStr)
            {
                components.Add(y);
                y.OnChanged += UpdateString;
            }
            else
            {
                foreach (var str in y.components)
                {
                    components.Add(str);
                    str.OnChanged += UpdateString;
                }
            }
        }

        public LocalizedString(string newText)
        {
            text = newText;
        }

        bool[] updatedStrings = null;
        int updatedCount = 0;

        private void UpdateString(LocalizedString text)
        {
            if (updatedStrings == null)
            {
                updatedStrings = new bool[components.Count];
            }

            for (int i = 0; i < components.Count; i++) 
            {
                if (components[i] == text && !updatedStrings[i])
                {
                    updatedCount++;
                    updatedStrings[i] = true;
                }
            }

            if (updatedCount >= components.Count)
            {
                RebuildString();
                updatedStrings = null;
                updatedCount = 0;
            }
        }

        private void RebuildString()
        {
            text = "";
            foreach (var str in components)
            {
                text += str.ToString();
            }
            languageIndex = LanguageController.Instance != null ? LanguageController.Instance.LanguageIndex : 0;
            OnChanged?.Invoke(this);
        }

        // * ==================================================

        /// <summary>
        /// Gets the raw string value currently loaded.
        /// </summary>
        public override string ToString()
        {
            return text;
        }

        public static implicit operator string(LocalizedString x)
        {
            return x.ToString();
        }

        public static LocalizedString operator +(LocalizedString x, LocalizedString y)
        {
            if (x.IsBaseStr)
            {
                if (y.IsBaseStr)
                {
                    return new LocalizedString(x, y);
                }
                else
                {
                    y.components.Insert(0, x);
                    x.OnChanged += y.UpdateString;
                    return y;
                }
            }
            else if (y.IsBaseStr)
            {
                x.components.Add(y);
                y.OnChanged += x.UpdateString;
                return x;
            }

            // if both strings have components
            foreach (var str in y.components)
            {
                x.components.Add(str);
                str.OnChanged += x.UpdateString;
            }
            return x;
        }

        public static LocalizedString operator +(LocalizedString x, string y)
        {
            return x + new LocalizedString(y);
        }

        public static LocalizedString operator +(string x, LocalizedString y)
        {
            return new LocalizedString(x) + y;
        }

        public override bool Equals(object obj)
        {        
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            return base.Equals (obj);
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
