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

    public Transform jumpCurveHandle;

    public WPConnection(SHR_Waypoint wp, Type type) {
        this.wp = wp;
        this.type = type;
    }

    public WPConnection() {}
}

[ExecuteInEditMode]
public class SHR_Waypoint : MonoBehaviour {
    public SHR_WaypointGraph graph;

    //public Transform jumpCurveHandle;
    public float waitHere = 0.0f;       //when > 0, it causes the Perso to stop and wait on this WP for this many seconds

    public List<WPConnection> next = new List<WPConnection>();
    public List<WPConnection> prev = new List<WPConnection>();

    private Vector3 lineOffset = new Vector3(0, 1.0f, 0);

    public SHR_Waypoint getRandomNextWaypoint() {
        return next[Random.Range(0, next.Count)].wp;
    }

    private void OnDrawGizmos() {
        if ((graph = GetComponentInParent<SHR_WaypointGraph>()) == null) return;

        //colour of the in/outgoing connections nubs on waypoint nodes
        var outboundConnectionColour = graph.outboundConnectionColour;
        var inbouncConnectionColour = graph.inboundConnectionColour;
        var whiteCol = Color.Lerp(outboundConnectionColour, inbouncConnectionColour, 0.5f);

        foreach (var conn in next) {
            if (conn != null) {
                Gizmos.color = whiteCol;

                var horizontalDiff = Vector3.zero;
                float steps;
                var dir = Vector3.zero;
                var lastPos = Vector3.zero;

                Vector3 sideOffset = Vector3.zero;

                switch (conn.type) {
                    case WPConnection.Type.JumpTo:
                        var midPos = (conn.wp.transform.position + transform.position) / 2;

                        if (conn.jumpCurveHandle == null) {
                            conn.jumpCurveHandle = new GameObject("HDL_jumpCurve_" + name + "_" + conn.wp.name).transform;
                            conn.jumpCurveHandle.position = midPos + Vector3.up * 5;
                        }

                        var handle = conn.jumpCurveHandle;
                        
                        handle.position = new Vector3(midPos.x, handle.position.y, midPos.z);

                        if (handle.parent != transform)
                            handle.parent = transform;

                        for (int i = 0; i <= 1; i++) {
                            Vector3 wpPos = ((i == 0) ? this : conn.wp).transform.position + lineOffset;

                            float diffY_selfToHandle = handle.position.y - wpPos.y;
                            horizontalDiff = wpPos - handle.position;
                            horizontalDiff.y = 0;

                            dir = horizontalDiff.normalized;
                            lastPos = handle.position;

                            //add sideways offset to the connectors if they are bi-directional
                            foreach (var c in conn.wp.next) {
                                if (c.wp == this) {
                                    sideOffset = Vector3.Cross(Vector3.up * 0.45f, dir * ((i == 1) ? 1 : -1));
                                }
                            }

                            Color portColour = (i == 0) ? outboundConnectionColour : inbouncConnectionColour;

                            steps = Mathf.Ceil(horizontalDiff.magnitude);
                            for (float x = 1; x <= steps; x += horizontalDiff.magnitude / steps) {
                                var newPos = handle.position + dir * x + Vector3.down * Mathf.Pow(x / horizontalDiff.magnitude, 2) * diffY_selfToHandle;

                                //blend between in and output colours based on x
                                if (i == 0) {
                                    Gizmos.color = Color.Lerp(Color.Lerp(inbouncConnectionColour, outboundConnectionColour, 0.5f), outboundConnectionColour, x / horizontalDiff.magnitude);
                                } else {
                                    Gizmos.color = Color.Lerp(Color.Lerp(inbouncConnectionColour, outboundConnectionColour, 0.5f), inbouncConnectionColour, x / horizontalDiff.magnitude);
                                }

                                Gizmos.DrawLine(lastPos + sideOffset, newPos + sideOffset);
                                lastPos = newPos;
                            }

                            
                            Gizmos.color = portColour;
                            Gizmos.DrawSphere(wpPos - (lastPos - wpPos).normalized + sideOffset, .4f);
                        }

                        Gizmos.color = Color.red / 1.4f;
                        Gizmos.DrawSphere(handle.position + sideOffset, 0.4f);
                        Gizmos.color = whiteCol;

                        break;
                    default:
                        if (transform.childCount != 0) {
                            if (conn.jumpCurveHandle != null) {
                                //var tr = transform.Find("HDL_jumpCurve_" + name + "_" + conn.wp.name);

                                //if (tr != null) {
                                    DestroyImmediate(conn.jumpCurveHandle.gameObject);
                                //}

                                conn.jumpCurveHandle = null;
                            }
                        }

                        horizontalDiff = conn.wp.transform.position - transform.position;
                        //horizontalDiff.y = 0;

                        dir = horizontalDiff.normalized;
                        lastPos = transform.position;

                        //add sideways offset to the connectors if they are bi-directional
                        foreach (var c in conn.wp.next) {
                            if (c.wp == this) {
                                sideOffset = Vector3.Cross(Vector3.up * 0.45f, dir);
                            }
                        }

                        steps = Mathf.Ceil(horizontalDiff.magnitude);
                        for (float x = 1; x <= steps; x += horizontalDiff.magnitude / steps) {
                            var newPos = transform.position + x * dir;

                            //blend between in and output colours based on x
                            Gizmos.color = Color.Lerp(outboundConnectionColour, inbouncConnectionColour, x / horizontalDiff.magnitude);

                            //also add a tiny bit of sideways offset relative to the direction; connect from "my right" to "your left"
                            Gizmos.DrawLine(lastPos + lineOffset + sideOffset, newPos + lineOffset + sideOffset);
                            lastPos = newPos;
                        }


                        Gizmos.color = inbouncConnectionColour;
                        Gizmos.DrawSphere(conn.wp.transform.position + (transform.position - conn.wp.transform.position).normalized + lineOffset + sideOffset, .4f);

                        Gizmos.color = outboundConnectionColour;
                        Gizmos.DrawSphere(transform.position + (conn.wp.transform.position - transform.position).normalized + lineOffset + sideOffset, .4f);
                        break;
                }
            }
        }

        Gizmos.color = whiteCol;
        Gizmos.DrawSphere(transform.position + lineOffset, 1f);

    }
}
