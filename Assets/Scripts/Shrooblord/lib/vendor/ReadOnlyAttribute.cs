using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
// This script will allow you to mark -any- field as readonly with a single property attribute / property drawer. It's partially based on scottmontgomerie's answer.
//This version also dispatches the property height based on how "expanded" the property is.
//author: It3ration
//source: https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html
//eg. use: [ReadOnly] public string a;
public class ReadOnlyAttribute : PropertyAttribute {

}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label) {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif
