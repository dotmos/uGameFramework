﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NPBehave
{
#if UNITY_EDITOR
    [CustomEditor(typeof(Debugger))]
    public class DebuggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Label("NPBehave Debugger", EditorStyles.centeredGreyMiniLabel);

            if (GUILayout.Button("Open Debugger"))
            {
                DebuggerWindow.selectedDebugger = ((Debugger)target);
                DebuggerWindow.selectedObject = DebuggerWindow.selectedDebugger.transform;
                DebuggerWindow.ShowWindow();
            }
        }
    }
#endif
}