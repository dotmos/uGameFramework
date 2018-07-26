using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UserInterface
{
    [CustomEditor(typeof(GMToggle)), CanEditMultipleObjects]
    public class GMToggleEditor : UnityEditor.UI.ToggleEditor
    {

        SerializedProperty colorizeElementsProp;
        SerializedProperty defaultColorProp;
        SerializedProperty highlightColorProp;
        SerializedProperty pressedColorProp;
        SerializedProperty disabledColorProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            //Setup the SerializedProperties
            colorizeElementsProp = serializedObject.FindProperty("colorizeElements");
            defaultColorProp = serializedObject.FindProperty("defaultColor");
            highlightColorProp = serializedObject.FindProperty("highlightColor");
            pressedColorProp = serializedObject.FindProperty("pressedColor");
            disabledColorProp = serializedObject.FindProperty("disabledColor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();

            EditorGUILayout.PropertyField(colorizeElementsProp, true);
            EditorGUILayout.PropertyField(defaultColorProp);
            EditorGUILayout.PropertyField(highlightColorProp);
            EditorGUILayout.PropertyField(pressedColorProp);
            EditorGUILayout.PropertyField(disabledColorProp);

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
