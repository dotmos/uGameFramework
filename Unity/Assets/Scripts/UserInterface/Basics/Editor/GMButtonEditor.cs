using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UserInterface
{
    [CustomEditor(typeof(GMButton)), CanEditMultipleObjects]
    public class GMButtonEditor : UnityEditor.UI.ButtonEditor
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
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(defaultColorProp);
            EditorGUILayout.PropertyField(highlightColorProp);
            EditorGUILayout.PropertyField(pressedColorProp);
            EditorGUILayout.PropertyField(disabledColorProp);
            EditorGUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}
