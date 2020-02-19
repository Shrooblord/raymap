using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using CustomGame;

[CanEditMultipleObjects]
[CustomEditor(typeof(SHR_Waypoint))]
public class SHR_WaypointEditor : Editor {
    SHR_Waypoint wp => (SHR_Waypoint)target;

    public void SnapToGround() {
        var hitResult = RayCollider.Raycast(wp.transform.position, Vector3.down, 100);

        if (hitResult.AnyGround) {
            wp.transform.position = hitResult.hit.point;
        }
    }

    private void OnSceneGUI() {
        Event e = Event.current;
        if (e.type == EventType.KeyDown) {
            if (e.keyCode == KeyCode.End)
                SnapToGround();
        }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        if (GUILayout.Button("Snap to Ground", GUILayout.Width(300))) {
            SnapToGround();
        }
    }
}
