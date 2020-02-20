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

    WPConnection connectionToAddOutbound = new WPConnection();
    WPConnection connectionToAddInbound = new WPConnection();
    public override void OnInspectorGUI() {
        GUILayout.Label("Outbound Connections");

        foreach (var n in wp.next) {
            EditorGUILayout.BeginHorizontal();
            n.wp = (SHR_Waypoint)EditorGUILayout.ObjectField(n.wp, typeof(SHR_Waypoint), true);
            n.type = (WPConnection.Type)EditorGUILayout.EnumPopup(n.type);

            if (GUILayout.Button("-", GUILayout.Width(30))) {
                //remove forward references
                wp.next.Remove(n);
                
                //remove backward references
                for (int i=0; i < n.wp.prev.Count; i++) {
                    if (n.wp.prev[i].wp == wp) {
                        n.wp.prev.Remove(n.wp.prev[i--]);
                    }
                }

                //destroy the children
                if (wp.transform.childCount != 0) {
                    var tr = wp.transform.Find("HDL_jumpCurve_" + n.wp.name);

                    if (tr != null) {
                        DestroyImmediate(tr.gameObject);
                        wp.graph.jumpCurveHandles.Remove(tr);
                    }
                }

                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+", GUILayout.Width(30)) && (connectionToAddOutbound.wp != null)) {
            //add him to our forwards array
            wp.next.Add(connectionToAddOutbound);

            //add us to his backwards array
            connectionToAddOutbound.wp.prev.Add(new WPConnection(wp, connectionToAddOutbound.type));

            //reset connection for next UI interaction
            connectionToAddOutbound = new WPConnection();
        }
        
        connectionToAddOutbound.wp = (SHR_Waypoint)EditorGUILayout.ObjectField(connectionToAddOutbound.wp, typeof(SHR_Waypoint), true);
        connectionToAddOutbound.type = (WPConnection.Type)EditorGUILayout.EnumPopup(connectionToAddOutbound.type);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space();


        GUILayout.Label("Inbound Connections");

        foreach (var n in wp.prev) {
            EditorGUILayout.BeginHorizontal();
            n.wp = (SHR_Waypoint)EditorGUILayout.ObjectField(n.wp, typeof(SHR_Waypoint), true);
            n.type = (WPConnection.Type)EditorGUILayout.EnumPopup(n.type);

            if (GUILayout.Button("-", GUILayout.Width(30))) {
                //remove forward references
                wp.prev.Remove(n);

                //remove backward references
                for (int i = 0; i < n.wp.next.Count; i++) {
                    if (n.wp.next[i].wp == wp) {
                        n.wp.next.Remove(n.wp.next[i--]);
                        break;
                    }
                }

                //destroy the children
                if (wp.transform.childCount != 0) {
                    var tr = wp.transform.Find("HDL_jumpCurve_" + n.wp.name);

                    if (tr != null) {
                        DestroyImmediate(tr.gameObject);
                        wp.graph.jumpCurveHandles.Remove(tr);
                    }
                }

                break;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+", GUILayout.Width(30)) && (connectionToAddInbound.wp != null)) {
            //add him to our backwards array
            wp.prev.Add(connectionToAddInbound);

            //add us to his forwards arary
            connectionToAddInbound.wp.next.Add(new WPConnection(wp, connectionToAddInbound.type));

            //reset connection for next UI interaction
            connectionToAddInbound = new WPConnection();
        }

        connectionToAddInbound.wp = (SHR_Waypoint)EditorGUILayout.ObjectField(connectionToAddInbound.wp, typeof(SHR_Waypoint), true);
        connectionToAddInbound.type = (WPConnection.Type)EditorGUILayout.EnumPopup(connectionToAddInbound.type);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space();




        if (GUILayout.Button("Snap to Ground", GUILayout.Width(300))) {
            SnapToGround();
        }
    }
}
