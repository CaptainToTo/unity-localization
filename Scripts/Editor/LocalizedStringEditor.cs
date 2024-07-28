using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Ballistic.Localization;

namespace Ballistic.Localization.Editor
{
    [CustomPropertyDrawer(typeof(LocalizedString))]
    public class LocalizedStringEditor : PropertyDrawer
    {
        // caches languages
        Languages languages = null;

        bool inspectorOpen = true;

        delegate void Renderer();

        void AddIndent(int level, Renderer render)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(30 + level);
            render();
            GUILayout.EndHorizontal();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // find the languages asset
            if (languages == null)
            {
                var paths = AssetDatabase.FindAssets("t:Languages");
                if (paths.Length == 0)
                {
                    GUILayout.Label("LANGUAGES SCRIPTABLE OBJECT MUST EXIST");
                    return;
                }
                languages = (Languages) AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(paths[0]), typeof(Languages));
            }

            EditorGUI.BeginProperty(position, label, property);

            // add property name label
            // title property
            var title = property.FindPropertyRelative("_title");

            if (title.stringValue != null && title.stringValue != "")
            {
                label.text += ": " + title.stringValue;
            }
            inspectorOpen = EditorGUILayout.Foldout(inspectorOpen, label);

            if (!inspectorOpen)
            {
                EditorGUI.EndProperty();
                return;
            }

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel += 1;

            // translation GUID property
            var guid = property.FindPropertyRelative("translationGUID");
            var path = AssetDatabase.GUIDToAssetPath(guid.stringValue);
            var translation = (Translation) AssetDatabase.LoadAssetAtPath(path, typeof(Translation));

            AddIndent(0, () => {
                GUILayout.Label("String Title");
            });
            var newTitle = "";
            AddIndent(30, () => {
                if (guid.stringValue != "")
                {
                    GUILayout.Label(title.stringValue);
                    newTitle = title.stringValue;
                }
                else
                {
                    newTitle = GUILayout.TextField(title.stringValue);
                }
            });
            if (newTitle == null) return; // may be null after deselecting object
            newTitle = newTitle.Replace(" ", ""); // remove spaces from title for formatting

            // if name is unique
            if (!languages.HasTranslation(newTitle))
            {
                // if title has changed
                if (newTitle != title.stringValue)
                {
                    title.stringValue = newTitle;
                    
                    // and text has translation
                    if (translation != null)
                    {
                        // update the translation title, and file name
                        translation.Title = newTitle;
                        AssetDatabase.RenameAsset(path, newTitle);
                        EditorUtility.SetDirty(translation);
                    }
                }

                // if text doesn't have a translation yet
                if (newTitle != "" && translation == null)
                {
                    // create a new translation

                    bool addPressed = false;
                    AddIndent(0, () => {
                        addPressed = GUILayout.Button("Add Translation");
                    });
                    if (addPressed)
                    {
                        // make file
                        var tempTitle = title.stringValue;
                        path = $"Assets/Localization/Translations/{tempTitle}.asset";
                        Translation newTranslation = ScriptableObject.CreateInstance<Translation>();
                        AssetDatabase.CreateAsset(newTranslation, path);
                        if (guid == null)
                        {
                            Debug.Log("wtf");
                            property.FindPropertyRelative("translationGUID");
                        }
                        guid.stringValue = AssetDatabase.AssetPathToGUID(path);
                        var tempGUID = guid.stringValue;
                        translation = newTranslation;
                        EditorUtility.SetDirty(guid.serializedObject.targetObject);

                        // make file addressable
                        var settings = AddressableAssetSettingsDefaultObject.Settings;
                        AddressableAssetGroup group = settings.FindGroup("Translations");
                        var entry = settings.CreateOrMoveEntry(tempGUID, group);
                        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, entry, true);
                        // add to translations list
                        languages.AddTranslation(tempGUID);
                        // serialize translation assignments
                        translation.Title = tempTitle;
                        EditorUtility.SetDirty(languages);
                        EditorUtility.SetDirty(translation);

                        EditorGUI.indentLevel = indent;
                        EditorGUI.EndProperty();

                        AssetDatabase.SaveAssets();
                        return;
                    }
                }
            }
            // if name isn't unique & it isn't to this text's translation
            else if (translation != null && languages.GetTranslation(newTitle) != translation)
            {
                Debug.LogError("'" + newTitle + "' is already taken by another translation. Pick another name.");
            }
            // use an existing translation instead of creating a new one
            else if (translation == null && newTitle != "")
            {
                title.stringValue = newTitle;
                bool usePressed = false;
                AddIndent(0, () => {
                    usePressed = GUILayout.Button("Use Existing Translation");
                });
                if (usePressed)
                {
                    path = $"Assets/Localization/Translations/{newTitle}.asset";
                    guid.stringValue = AssetDatabase.AssetPathToGUID(path);
                }
            }

            // retry to fetch translation
            translation = (Translation) AssetDatabase.LoadAssetAtPath(path, typeof(Translation));

            // create text fields if translation is assigned
            if (translation)
            {
                // title
                GUILayout.Space(10f);
                AddIndent(0, () => {
                    GUILayout.Label("Translations");
                });
                GUILayout.Space(5f);

                // delete text
                bool deletePressed = false;
                AddIndent(0, () => {
                    deletePressed = GUILayout.Button("Delete Translation");
                });
                if (deletePressed)
                {
                    // remove from languages
                    languages.RemoveTranslation(title.stringValue);
                    EditorUtility.SetDirty(languages);
                    // delete asset files
                    path = AssetDatabase.GetAssetPath(translation.GetInstanceID());
                    AssetDatabase.DeleteAsset(path);
                    // remove assignment from localized string
                    guid.stringValue = "";
                    EditorGUI.EndProperty();
                    return;
                }

                // unlink text
                bool unlinkPressed = false;
                AddIndent(0, () => {
                    unlinkPressed = GUILayout.Button("Unlink Translation");
                });
                if (unlinkPressed)
                {
                    // remove assignment from localized string
                    guid.stringValue = "";
                    EditorGUI.EndProperty();
                    return;
                }

                // load translation text
                translation.SetLanguages(languages.LanguageCount);

                // fields for each language
                foreach (var language in languages.languages)
                {
                    // title / language name
                    AddIndent(0, () => {
                        GUILayout.Label(language.name);
                    });

                    // set & get text area value
                    int index = languages.GetLanguageIndex(language.name);

                    string newText = "";
                    AddIndent(30, () => {
                        newText = GUILayout.TextArea(
                            translation.GetContent(index),
                            GUILayout.Height(20)
                        );
                    });

                    // update translation if text has changed
                    if (newText != translation.GetContent(index))
                    {
                        translation.SetContent(index, newText);
                        EditorUtility.SetDirty(translation);
                    }
                    GUILayout.Space(5f);
                }
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
