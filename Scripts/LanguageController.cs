using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ballistic.Localization
{
    /// <summary>
    /// Singleton class used to select languages. Interface for SettingsManager.
    /// Referenced by LocalizedText and LocalizedString to determine when to change translations,
    /// and which translation to change to.
    /// </summary>
    public class LanguageController
    {
        public static LanguageController Instance { get; private set; } = null;

        private Languages languages;

        /// <summary>
        /// Initializes the LanguageController with a given list of languages, and an initial language.
        /// </summary>
        public LanguageController(Languages languages, string curLanguage)
        {
            Instance = this;
            this.languages = languages;
            language = curLanguage;
        }

        /// <summary>
        /// Provides the language being changed to, and its translation index.
        /// </summary>
        public delegate void OnLanguageChangedEvent(Language language, int index);

        /// <summary>
        /// Invoked when the language changes. Notifies LocalizedText instances to update text.
        /// </summary>
        public event OnLanguageChangedEvent OnLanguageChanged;

        /// <summary>
        /// Gets the currently selected language.
        /// </summary>
        public Language Language {
            get {
                return Instance.languages.Get(language);
            }
        }
        private string language = "";

        /// <summary>
        /// Update the current language
        /// </summary>
        public void SetLanguage(string languageName)
        {
            language = languageName;
            OnLanguageChanged?.Invoke(Language, languages.GetLanguageIndex(language));
        }

        /// <summary>
        /// Gets all of the existing language options.
        /// </summary>
        public Language[] Languages {
            get {
                return Instance.languages.languages;
            }
        }

        /// <summary>
        /// Unique index identifier for a language.
        /// </summary>
        public int LanguageIndex {
            get {
                return Instance.languages.GetLanguageIndex(language);
            }
        }
    }

}
