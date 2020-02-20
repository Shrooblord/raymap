using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SHR_Waypoint : MonoBehaviour {
    private SHR_WaypointGraph graph;
    //These two variables control whether the Perso should attempt jumping to or from this Waypoint from their current position towards their next target.
    public bool jumpFromHere = false;
    public bool jumpToHere = false;
    public List<SHR_Waypoint> next = new List<SHR_Waypoint>();
    public List<SHR_Waypoint> prev = new List<SHR_Waypoint>();

    private Vector3 lineOffset = new Vector3(0, 1.0f, 0);

    public SHR_Waypoint getRandomNextWaypoint() {
        return next[Random.Range(0, next.Count)];
    }

    private void OnDrawGizmos() {
        if ((graph = GetComponentInParent<SHR_WaypointGraph>()) == null) return;

        var whiteCol = graph.graphColor;

        foreach (var wp in prev) {
            if (prev != null) {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position + (wp.transform.position - transform.position).normalized + lineOffset, .4f);
            }
        }

        foreach (var wp in next) {
            if (wp != null) {
                if (!wp.prev.Contains(this)) {
                    wp.prev.Add(this);
                }

                Gizmos.color = whiteCol;
                Gizmos.DrawLine(transform.position + lineOffset, wp.transform.position + lineOffset);

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(transform.position + (wp.transform.position - transform.position).normalized + lineOffset, .4f);
            }
        }

        Gizmos.color = whiteCol;
        Gizmos.DrawSphere(transform.position + lineOffset, 1f);
    }
}
