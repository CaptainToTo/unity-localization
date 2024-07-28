using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Ballistic.Localization
{
    /// <summary>
    /// Wrapper around TMPro text component.
    /// Will update the text's font when the language changes.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class LocalizedText : MonoBehaviour
    {
        /// <summary>
        /// The text mesh pro Text component this class wraps.
        /// </summary>
        public TextMeshProUGUI TMP {
            get 
            { 
                if (tmp == null)
                {
                    tmp = GetComponent<TextMeshProUGUI>();
                }
                return tmp; 
            }
        }
        private TextMeshProUGUI tmp = null;

        /// <summary>
        /// Use to set and get the current text displayed.
        /// </summary>
        public LocalizedString text {
            get { 
                return _text; 
            }
            set { 
                _text.OnChanged -= UpdateText;
                _text = value;
                if (LanguageController.Instance == null || _text.LanguageIndex != LanguageController.Instance.LanguageIndex)
                {
                    if (LanguageController.Instance != null)
                    {
                        _text.UpdateLanguage(LanguageController.Instance.Language, LanguageController.Instance.LanguageIndex);
                    }
                    else
                    {
                        _text.UpdateLanguage(new Language(), 0);
                    }
                }
                _text.OnChanged += UpdateText;
                if (tmp)
                {
                    tmp.text = value.ToString();

                    if (IsUpperCase && tmp.text != null)
                    {
                        tmp.text = tmp.text.ToUpper();
                    }
                }
            }
        }
        [SerializeField] private LocalizedString _text = null;

        public bool IsUpperCase = false;

        void Start()
        {
            tmp = GetComponent<TextMeshProUGUI>();
            LanguageController.Instance.OnLanguageChanged -= UpdateLanguage;
            LanguageController.Instance.OnLanguageChanged += UpdateLanguage;
            _text.OnChanged += UpdateText;
            if (LanguageController.Instance != null)
            {
                UpdateLanguage(LanguageController.Instance.Language, LanguageController.Instance.LanguageIndex);
            }

            if (tempStr != null)
            {
                tmp.text = tempStr.ToString();
                tempStr = null;
            }
        }

        /// <summary>
        /// Change the text, and its assigned string to match the given language.
        /// ONLY USED BY SETTINGS MANAGER AND OTHER LOCALIZATION CLASSES.
        /// </summary>
        public void UpdateLanguage(Language language, int index)
        {
            tmp.font = language.font;
            _text.UpdateLanguage(language, index);
        }

        /// <summary>
        /// Procedurally sets the text displayed by this component.
        /// ONLY USED BY OTHER LOCALIZATION CLASSES.
        /// </summary>
        void UpdateText(LocalizedString text)
        {
            if (tmp == null)
            {
                tempStr = text;
                return;
            }
            tmp.text = text?.ToString() ?? "";

            if (IsUpperCase)
            {
                tmp.text = tmp.text.ToUpper();
            }
        }

        LocalizedString tempStr = null;

        void OnDestroy()
        {
            if (LanguageController.Instance != null)
            {
                LanguageController.Instance.OnLanguageChanged -= UpdateLanguage;
            }
        }
    }
}
