using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// A power (W) or an energy (Wh) of the electrical network that can be infinite (shown in the inspector as a value and
// an "∞" toggle). Value is float.PositiveInfinity when infinite: compare with float.IsPositiveInfinity before subtracting
[Serializable]
public struct EnergyValue
{
    public float value;
    public bool infinite;

    public EnergyValue(float value, bool infinite = false)
    {
        this.value = value;
        this.infinite = infinite;
    }

    public float Value => infinite ? float.PositiveInfinity : Mathf.Max(0f, value);

    public static string Format(float amount, string unit)
    {
        if (float.IsPositiveInfinity(amount)) return "∞ " + unit;

        return (amount >= 100f ? amount.ToString("0") : amount.ToString("0.#")) + " " + unit;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnergyValue))]
public class EnergyValueDrawer : PropertyDrawer
{
    private const float ToggleWidth = 36f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect field = EditorGUI.PrefixLabel(position, label);
        SerializedProperty value = property.FindPropertyRelative("value");
        SerializedProperty infinite = property.FindPropertyRelative("infinite");

        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Rect valueRect = new Rect(field.x, field.y, field.width - ToggleWidth - 4f, field.height);
        Rect toggleRect = new Rect(field.xMax - ToggleWidth, field.y, ToggleWidth, field.height);

        using (new EditorGUI.DisabledScope(infinite.boolValue))
        {
            EditorGUI.PropertyField(valueRect, value, GUIContent.none);
        }

        infinite.boolValue = EditorGUI.ToggleLeft(toggleRect, "∞", infinite.boolValue);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }
}
#endif
