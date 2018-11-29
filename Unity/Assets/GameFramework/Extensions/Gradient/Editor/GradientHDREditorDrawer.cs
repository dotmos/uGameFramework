/**
 * GradientHDRAttribute created by Chance Millar at Two Tails Games on 2018/05/08
 * Use freely. That's a license, right?
 * 
 * Attach attribute like so:
 *
 *  [GradientHDR]
 *  public Gradient _HDRGradient;
 *
 * Make sure this is placed in a non-Editor folder inside Assets.
 */

/**
 * GradientHDRAttribute created by Chance Millar at Two Tails Games on 2018/05/08
 * Use freely. That's a license, right?
 *
 * Be sure to place in an Editor folder inside Assets.
 */

using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

/**
 * Custom Property Drawer for GradientHDRAttribute
 */
[CustomPropertyDrawer(typeof(GradientHDRAttribute))]
public class GradientHDREditorDrawer : PropertyDrawer {
    // Cache the reflected method of EditorGUI.GradientField().
    private static MethodInfo sGradientFieldMethod = null;

    /// Cache the parms to save the GC from itself. Make sure the third param is always "true", this is your HDR enabler.
    private static object[] sInvokeParams = new object[3] { null, null, true };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        // If the attribute is assigned to a non-Gradient type, then get outta here!
        if (property.propertyType != SerializedPropertyType.Gradient) {
            base.OnGUI(position, property, label);
            Debug.LogWarning("GradientHDRAttribute assigned to non-GradientType");
            return;
        }

        // Do standard UI stuff.
        EditorGUI.BeginProperty(position, label, property);

        // Draw the label so we know the variable name.
        position = EditorGUI.PrefixLabel(position, label);

        // Draw the HDR Gradient Field.
        GradientField(position, property);

        EditorGUI.EndProperty();
    }

    /// Makeshift EditorGUI.GradientField()
    private static void GradientField(Rect position, SerializedProperty gradient) {
        // Check to make sure we have the EditorGUI.GradientField() method.
        ReflectMethod();

        // Update the params with the gradient info.
        sInvokeParams[0] = position; // Rect Param
        sInvokeParams[1] = gradient; // Serialized Property Param.
        // The third param is "bool hdr", which is already set to true in the initialization. Set that to false, and no more HDR field!

        // Call EditorGUI.GradientField() via reflection!
        // (This invoke can technically be cached, but because it's editor code, you only need to deselect the object with this field to get the ms back.)
        sGradientFieldMethod.Invoke(null, sInvokeParams);
    }

    /// Cache EditorGUI.GradientField() if it isn't already.
    private static void ReflectMethod() {
        if (sGradientFieldMethod == null) {
            // We know the function we need is under EditorGUI, so get that type.
            Type gradientEditorType = typeof(EditorGUI);

            // Now we know the field is called "GradientField", but it's an internal static type. So binding flags must be Static and NonPublic (as other GetMethod types default to public instance).
            // There's _lots_ of overloaded GradientFields in the source, but we're only interested in one particular method. (There's only 2 HDR-enabling methods). So match the params for the one we want!
            sGradientFieldMethod = gradientEditorType.GetMethod("GradientField", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Rect), typeof(SerializedProperty), typeof(bool) }, null);
        }
    }
}

/**
 * Use this [GradientHDRAttribute] Attribute on Gradient fields to enable HDR Gradient UI.
 */
public class GradientHDRAttribute : PropertyAttribute { }