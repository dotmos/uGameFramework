using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class EnumFlagsAttribute : PropertyAttribute {
    public EnumFlagsAttribute() { }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer {
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
        // Workaround for multi edit: https://answers.unity.com/questions/1150282/how-to-correctly-use-editorguilayoutmaskfield-in-m.html
        // To avoid wrongly setting/changing data
        if (_property.hasMultipleDifferentValues) {
            bool guienabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.TextField(_position, _property.displayName, "Multi edit not allowed!");
            GUI.enabled = guienabled;
        } else {
            int newVal = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
            if (newVal != _property.intValue) {
                _property.intValue = newVal;
            }
        }
    }
}
#endif
