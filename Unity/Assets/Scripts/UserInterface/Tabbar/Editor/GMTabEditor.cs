using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

namespace UserInterface
{
    [CustomEditor(typeof(GMTab)), CanEditMultipleObjects]
    public class GMTabEditor : UnityEditor.UI.ToggleEditor
    {

        SerializedProperty contentProp;
        SerializedProperty borderProp;
        SerializedProperty colorizeObjectProp;
        SerializedProperty defaultColorProp;
        SerializedProperty activeColorProp;
        SerializedProperty highlightColorProp;
        SerializedProperty pressedColorProp;
        SerializedProperty selectedColorProp;
        SerializedProperty disabledColorProp;
        SerializedProperty surpressSubmitEventProp;

        protected override void OnEnable()
        {
            base.OnEnable();

            //Setup the SerializedProperties
            contentProp = serializedObject.FindProperty("content");
            colorizeObjectProp = serializedObject.FindProperty("colorizeElements");
            defaultColorProp = serializedObject.FindProperty("defaultColor");
            activeColorProp = serializedObject.FindProperty("activeColor");
            highlightColorProp = serializedObject.FindProperty("highlightColor");
            pressedColorProp = serializedObject.FindProperty("pressedColor");
            selectedColorProp = serializedObject.FindProperty("selectedColor");
            disabledColorProp = serializedObject.FindProperty("disabledColor");
            borderProp = serializedObject.FindProperty("border");
            surpressSubmitEventProp = serializedObject.FindProperty("surpressSubmitEvent");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            base.OnInspectorGUI();

            EditorGUILayout.ObjectField(contentProp);
            EditorGUILayout.PropertyField(borderProp);
            EditorGUILayout.PropertyField(colorizeObjectProp, true);
            EditorGUILayout.PropertyField(defaultColorProp);
            EditorGUILayout.PropertyField(activeColorProp);
            EditorGUILayout.PropertyField(highlightColorProp);
            EditorGUILayout.PropertyField(pressedColorProp);
            EditorGUILayout.PropertyField(selectedColorProp);
            EditorGUILayout.PropertyField(disabledColorProp);
            EditorGUILayout.PropertyField(surpressSubmitEventProp);

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
