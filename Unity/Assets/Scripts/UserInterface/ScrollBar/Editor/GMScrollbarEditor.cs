using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface.Scrollbar {
    [CustomEditor(typeof(GMScrollbar), true)]
    [CanEditMultipleObjects]
    public class GMScrollbarEditor : SelectableEditor {
        SerializedProperty m_firstButton;
        SerializedProperty m_secondButton;
        SerializedProperty m_navButtonStep;
        SerializedProperty m_navContinousScrollDelay;
        SerializedProperty m_HandleRect;
        SerializedProperty m_Direction;
        SerializedProperty m_Value;
        SerializedProperty m_Size;
        SerializedProperty m_NumberOfSteps;
        SerializedProperty m_OnValueChanged;

        protected override void OnEnable() {
            base.OnEnable();

            m_firstButton = serializedObject.FindProperty("m_firstButton");
            m_secondButton = serializedObject.FindProperty("m_secondButton");
            m_navButtonStep = serializedObject.FindProperty("m_navButtonStep");
            m_navContinousScrollDelay = serializedObject.FindProperty("m_navContinousScrollDelay");
            m_HandleRect = serializedObject.FindProperty("m_HandleRect");
            m_Direction = serializedObject.FindProperty("m_Direction");
            m_Value = serializedObject.FindProperty("m_Value");
            m_Size = serializedObject.FindProperty("m_Size");
            m_NumberOfSteps = serializedObject.FindProperty("m_NumberOfSteps");
            m_OnValueChanged = serializedObject.FindProperty("m_OnValueChanged");
        }

        [MenuItem(NamingHelper.Scrollbar.HierachyName, false, 0)]
        static void AddPrefabToScene() {
            //Selection.activeTransform
            /*
            MonoScript ms = MonoScript.FromScriptableObject(this);
            string scriptFilePath = AssetDatabase.GetAssetPath(ms);

            FileInfo fi = new FileInfo(scriptFilePath);
            string prefabAssetPath = Path.Combine(fi.Directory.ToString(), "Prefabs", "Scrollbar.prefab");

            Debug.Log(prefabAssetPath);
            Debug.Log("test");
            */
        }
 
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();

            // EditorGUILayout.PropertyField(m_HandleRect);
            EditorGUI.BeginChangeCheck();
            RectTransform newRectTransform = EditorGUILayout.ObjectField("Handle Rect", m_HandleRect.objectReferenceValue, typeof(RectTransform), true) as RectTransform;
            if (EditorGUI.EndChangeCheck()) {
                // Handle Rect will modify its GameObject RectTransform drivenBy, so we need to Record the old and new RectTransform.
                List<Object> modifiedObjects = new List<Object>();
                modifiedObjects.Add(newRectTransform);
                foreach (var target in m_HandleRect.serializedObject.targetObjects) {
                    MonoBehaviour mb = target as MonoBehaviour;
                    if (mb == null)
                        continue;

                    modifiedObjects.Add(mb);
                    modifiedObjects.Add(mb.GetComponent<RectTransform>());
                }
                Undo.RecordObjects(modifiedObjects.ToArray(), "Change Handle Rect");
                m_HandleRect.objectReferenceValue = newRectTransform;
            }

            if (m_HandleRect.objectReferenceValue != null) {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_Direction);
                if (EditorGUI.EndChangeCheck()) {
                    Direction direction = (Direction)m_Direction.enumValueIndex;
                    foreach (var obj in serializedObject.targetObjects) {
                        GMScrollbar scrollbar = obj as GMScrollbar;
                        scrollbar.SetDirection(direction, true);
                    }
                }

                EditorGUILayout.PropertyField(m_Value);
                EditorGUILayout.PropertyField(m_Size);
                EditorGUILayout.PropertyField(m_NumberOfSteps);

                bool warning = false;
                foreach (var obj in serializedObject.targetObjects) {
                    GMScrollbar scrollbar = obj as GMScrollbar;
                    Direction dir = scrollbar.direction;
                    if (dir == Direction.RightToLeft)
                        warning = (scrollbar.navigation.mode != Navigation.Mode.Automatic && (scrollbar.FindSelectableOnLeft() != null || scrollbar.FindSelectableOnRight() != null));
                    else
                        warning = (scrollbar.navigation.mode != Navigation.Mode.Automatic && (scrollbar.FindSelectableOnDown() != null || scrollbar.FindSelectableOnUp() != null));
                }

                if (warning)
                    EditorGUILayout.HelpBox("The selected scrollbar direction conflicts with navigation. Not all navigation options may work.", MessageType.Warning);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Navigation Buttons", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.PropertyField(m_firstButton);
                EditorGUILayout.PropertyField(m_secondButton);
                EditorGUILayout.PropertyField(m_navButtonStep);
                EditorGUILayout.PropertyField(m_navContinousScrollDelay, new GUIContent("Scroll Delay (sec.)"));
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                // Draw the event notification options
                EditorGUILayout.PropertyField(m_OnValueChanged);
            }
            else {
                EditorGUILayout.HelpBox("Specify a RectTransform for the scrollbar handle. It must have a parent RectTransform that the handle can slide within.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
