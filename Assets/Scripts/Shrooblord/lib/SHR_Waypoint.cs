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
    private Transform jumpCurveHandle;

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

        foreach (var conn in prev) {
            if (conn?.wp != null) {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position + (conn.wp.transform.position - transform.position).normalized + lineOffset, .4f);
            }
        }

        foreach (var conn in next) {
            if (conn != null) {
                Gizmos.color = whiteCol;
                Gizmos.DrawLine(transform.position + lineOffset, conn.wp.transform.position + lineOffset);

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position + (conn.wp.transform.position - transform.position).normalized + lineOffset, .4f);
            }
        }

        Gizmos.color = whiteCol;
        Gizmos.DrawSphere(transform.position + lineOffset, 1f);
    }
}
