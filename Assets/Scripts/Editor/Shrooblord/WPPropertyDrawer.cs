using UnityEditor;
using UnityEngine;

namespace Shrooblord.lib {
    [CustomPropertyDrawer(typeof(WPConnection))]
    public class WPConnectionDrawer : PropertyDrawer {
        public override void OnGUI(Rect pos, SerializedProperty property, GUIContent label) {
            EditorGUILayout.PropertyField(property.FindPropertyRelative("wp"));
            EditorGUILayout.PropertyField(property.FindPropertyRelative("type"));
        }
    }
}
