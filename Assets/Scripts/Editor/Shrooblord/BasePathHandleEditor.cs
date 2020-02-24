using UnityEditor;
using UnityEngine;
using Shrooblord.lib;

[CanEditMultipleObjects]
[CustomEditor(typeof(BasePathHandle), true)]
public class BasePathHandleEditor : Editor {
    BasePathHandle bph => (BasePathHandle)target;  //are we using the Inspector to look at a Path Handle?

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        //GUILayout.Label("Connection");

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Label("Parent");
            var f = (SHR_Waypoint)EditorGUILayout.ObjectField(bph.parentWaypoint, typeof(SHR_Waypoint), true);

            foreach (var n in bph.parentWaypoint.next) {
                if (n.pathHandle == bph) {
                    GUI.changed = false;
                    //Keep track of whether the user just used the enum field
                    n.type = (WPConnection.Type)EditorGUILayout.EnumPopup(n.type);

                    if (GUI.changed) {
                        //The user just changed this value. That means we need to update its pair in the next Waypoint's prev list.
                        foreach (var p in n.wp.prev) {
                            if (p.pathHandle == bph) {
                                p.type = n.type;
                            }
                        }
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space();
    }
}
