using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//All possible connection types between Waypoints.
[System.Serializable]
public class WPConnection {
    public enum Type { WalkTo, JumpTo, DrillTo, ParachuteTo, TeleportTo };
    public SHR_Waypoint wp;
    public Type type;

    public WPConnection(SHR_Waypoint wp, Type type) {
        this.wp = wp;
        this.type = type;
    }

    public WPConnection() {}
}

[ExecuteInEditMode]
public class SHR_Waypoint : MonoBehaviour {
    private SHR_WaypointGraph graph;

    //These two variables control from which waypoint to which next Waypoint the Perso can jump
    //[Tooltip("Define this Waypoint as one that a Perso could jump from to another one. The acceptor WP needs to be connected to this one, and have the jumpToHere flag checked.")]
    //public bool jumpFromHere = false;
    //[Tooltip("Define this Waypoint as one that a Perso could jump to from another one. The instigating WP needs to have this one as one of its connections, and have the jumpFromHere flag checked.")]
    //public bool jumpToHere = false;
    public Transform jumpCurveHandle;

    public float waitHere = 0.0f;       //when > 0, it causes the Perso to stop and wait on this WP for this many seconds

    public List<WPConnection> next = new List<WPConnection>();
    public List<WPConnection> prev = new List<WPConnection>();

    private Vector3 lineOffset = new Vector3(0, 1.0f, 0);

    public SHR_Waypoint getRandomNextWaypoint() {
        return next[Random.Range(0, next.Count)].wp;
    }

    private void OnDrawGizmos() {
        if ((graph = GetComponentInParent<SHR_WaypointGraph>()) == null) return;

        var whiteCol = graph.graphColor;

        /*
         foreach (var conn in prev) {
            if (conn?.wp != null) {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position + (conn.wp.transform.position - transform.position).normalized + lineOffset, .4f);
            }
        }
        */

        foreach (var conn in next) {
            if (conn != null) {
                Gizmos.color = whiteCol;

                switch (conn.type) {
                    case WPConnection.Type.JumpTo:
                        var midPos = (conn.wp.transform.position + transform.position) / 2;

                        if (conn.wp.jumpCurveHandle == null) {
                            conn.wp.jumpCurveHandle = new GameObject("HDL_jumpCurve").transform;
                            conn.wp.jumpCurveHandle.position = midPos + Vector3.up * 5;
                        }

                        var handle = conn.wp.jumpCurveHandle;
                        
                        handle.position = new Vector3(midPos.x, handle.position.y, midPos.z);
                        handle.parent = transform;
                        

                        //var dir = conn.wp.transform.position - transform.position;
                        //handle.position = new Vector3();

                        /*
                         * attempt at making it curved
                         * 
                        float steps = 10;
                        for (float x = 0; x < 1.0f; x += (1.0f / steps)) {
                            Vector3 diff = (conn.wp.transform.position - transform.position);

                            Gizmos.DrawLine(
                                transform.position + diff * x,
                                transform.position + diff * (x * (1.0f / steps))
                            );
                        }
                        */
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(transform.position + (handle.position - transform.position).normalized + lineOffset, .4f);

                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(conn.wp.transform.position + (handle.position - conn.wp.transform.position).normalized + lineOffset, .4f);

                        Gizmos.color = whiteCol;
                        Gizmos.DrawLine(transform.position + lineOffset, handle.position + lineOffset);
                        Gizmos.DrawLine(handle.position + lineOffset, conn.wp.transform.position + lineOffset);

                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(handle.position + lineOffset, 0.4f);
                        Gizmos.color = whiteCol;

                        break;
                    default:
                        if (conn.wp.jumpCurveHandle != null) {
                            DestroyImmediate(conn.wp.jumpCurveHandle.gameObject);
                        }
                        
                        Gizmos.DrawLine(transform.position + lineOffset, conn.wp.transform.position + lineOffset);

                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(conn.wp.transform.position + (transform.position - conn.wp.transform.position).normalized + lineOffset, .4f);

                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(transform.position + (conn.wp.transform.position - transform.position).normalized + lineOffset, .4f);
                        break;
                }
            }
        }

        Gizmos.color = whiteCol;
        Gizmos.DrawSphere(transform.position + lineOffset, 1f);

    }
}
