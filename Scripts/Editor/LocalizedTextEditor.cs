using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using Ballistic.Localization;

namespace Ballistic.Localization.Editor
{
    [CustomEditor(typeof(LocalizedText))]
    public class LocalizedTextEditor : UnityEditor.Editor
    {

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_text"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
