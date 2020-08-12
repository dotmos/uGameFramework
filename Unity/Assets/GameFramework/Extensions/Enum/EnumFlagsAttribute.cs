using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class EnumFlagsAttribute : PropertyAttribute {
    public Type enumType = null;
    public EnumFlagsAttribute(Type enumType = null) {
        this.enumType = enumType;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer {
    private Type enumType;
    private object targetObject;
    private GUIStyle propStyle;

    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label) {
        if (propStyle == null) {
            propStyle = new GUIStyle(EditorStyles.helpBox);
            propStyle.margin = new RectOffset();
            propStyle.padding = new RectOffset(3,3,3,3);
        }

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

        enumType = ((EnumFlagsAttribute)attribute).enumType;
        if (enumType != null) {
            _position.y += _position.height/2f;
            _position.height -= _position.height / 2f;
            if (_property.intValue > 0) {
                EditorGUI.TextField(_position, " ", "Set Values: "+Enum.ToObject(enumType, _property.intValue).ToString(), propStyle);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (enumType != null) {
            return EditorGUIUtility.singleLineHeight * 2f;
        }
        return EditorGUIUtility.singleLineHeight;
    }
}
#endif
