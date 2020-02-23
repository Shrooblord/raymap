using UnityEditor;
using UnityEngine;
using CustomGame;

[CanEditMultipleObjects]
[CustomEditor(typeof(SHR_Waypoint))]
public class SHR_WaypointEditor : Editor {
    SHR_Waypoint wp => (SHR_Waypoint)target;  //are we using the Inspector to look at a Waypoint?

    public void SnapToGround() {
        var hitResult = RayCollider.Raycast(wp.transform.position, Vector3.down, 100);

        if (hitResult.AnyGround) {
            wp.transform.position = hitResult.hit.point;
        }
    }

    private void OnSceneGUI() {
        if (wp != null) {
            Event e = Event.current;
            if (e.type == EventType.KeyDown) {
                if (e.keyCode == KeyCode.End)
                    SnapToGround();
            }
        }
    }

    WPConnection connectionToAddOutbound = new WPConnection();
    WPConnection connectionToAddInbound = new WPConnection();
    public override void OnInspectorGUI() {
        if (wp != null) {
            GUILayout.Label("Outbound Connections");

            foreach (var n in wp.next) {
                EditorGUILayout.BeginHorizontal();
                n.wp = (SHR_Waypoint)EditorGUILayout.ObjectField(n.wp, typeof(SHR_Waypoint), true);

                //Keep track of whether the user just used the enum field
                GUI.changed = false;
                n.type = (WPConnection.Type)EditorGUILayout.EnumPopup(n.type);
                if (GUI.changed) {
                    //The user just changed this value. That means we need to update its pair in the next Waypoint's prev list.
                    foreach (var p in n.wp.prev) {
                        if (p.wp == wp) {
                            p.type = n.type;
                        }
                    }
                }

                if (GUILayout.Button("-", GUILayout.Width(30))) {
                    //remove forward references
                    wp.next.Remove(n);

                    //remove backward references
                    for (int i = 0; i < n.wp.prev.Count; i++) {
                        if (n.wp.prev[i].wp == wp) {
                            n.wp.prev.Remove(n.wp.prev[i--]);
                        }
                    }

                    //destroy the children
                    if (n.type == WPConnection.Type.JumpTo) {
                        if (n.jumpCurveHandle != null) {
                            if (wp.transform.childCount != 0) {
                                //var tr = transform.Find("HDL_jumpCurve_" + name + "_" + conn.wp.name);

                                //if (tr != null) {
                                DestroyImmediate(n.jumpCurveHandle.gameObject);
                                //}

                                n.jumpCurveHandle = null;
                            }
                        }
                    } else if (n.type == WPConnection.Type.DrillTo) {
                        if (n.drillPathHandle != null) {
                            if (wp.transform.childCount != 0) {
                                DestroyImmediate(n.drillPathHandle.gameObject);
                                n.drillPathHandle = null;
                            }
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

            foreach (var p in wp.prev) {
                EditorGUILayout.BeginHorizontal();
                p.wp = (SHR_Waypoint)EditorGUILayout.ObjectField(p.wp, typeof(SHR_Waypoint), true);

                //Keep track of whether the user just used the enum field
                GUI.changed = false;
                p.type = (WPConnection.Type)EditorGUILayout.EnumPopup(p.type);
                if (GUI.changed) {
                    //The user just changed this value. That means we need to update its pair in the prev Waypoint's next list.
                    foreach (var n in p.wp.next) {
                        if (n.wp == wp) {
                            n.type = p.type;
                        }
                    }
                }

                if (GUILayout.Button("-", GUILayout.Width(30))) {
                    //remove forward references
                    wp.prev.Remove(p);

                    //remove backward references
                    for (int i = 0; i < p.wp.next.Count; i++) {
                        if (p.wp.next[i].wp == wp) {
                            p.wp.next.Remove(p.wp.next[i--]);
                            break;
                        }
                    }

                    //destroy the children
                    if (p.type == WPConnection.Type.JumpTo) {
                        if (p.jumpCurveHandle != null) {
                            if (p.wp.transform.childCount != 0) {
                                //var tr = transform.Find("HDL_jumpCurve_" + name + "_" + conn.wp.name);

                                //if (tr != null) {
                                DestroyImmediate(p.jumpCurveHandle.gameObject);
                                //}

                                p.jumpCurveHandle = null;
                            }
                        }
                    } else if (p.type == WPConnection.Type.DrillTo) {
                        if (p.drillPathHandle != null) {
                            if (wp.transform.childCount != 0) {
                                DestroyImmediate(p.drillPathHandle.gameObject);
                                p.drillPathHandle = null;
                            }
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
}
