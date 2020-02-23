using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//All possible connection types between Waypoints.
[System.Serializable]
public class WPConnection {
    public enum Type { WalkTo, JumpTo, DrillTo, ParachuteTo, TeleportTo, NONE };
    public SHR_Waypoint wp;
    public Type type;
    public float drillTime = 2.0f; //how long it takes for the Henchman to Drill from this location to the target Waypoint pair

    public Transform jumpCurveHandle;
    public Transform drillPathHandle;

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
        var inboundConnectionColour = graph.inboundConnectionColour;
        
        //colours used throughout...
        var ColWhite = Color.Lerp(outboundConnectionColour, inboundConnectionColour, 0.5f);
        var ColPurple = new Color(0.9f, 0.05f, 0.9f, 0.5f);

        foreach (var conn in next) {
            if (conn != null) {
                Gizmos.color = ColWhite;

                var horizontalDiff = Vector3.zero;
                float steps;
                var dir = Vector3.zero;
                var lastPos = Vector3.zero;

                Vector3 sideOffset = Vector3.zero;
                Vector3 midPos = Vector3.zero;

                switch (conn.type) {
                    #region JumpTo
                    case WPConnection.Type.JumpTo:
                        //First destroy all children that do not apply to this type of path
                        if (transform.childCount != 0) {
                            if (conn.drillPathHandle != null) {
                                DestroyImmediate(conn.drillPathHandle.gameObject);
                                conn.drillPathHandle = null;
                            }
                        }

                        midPos = (conn.wp.transform.position + transform.position) / 2;

                        //Add a new Jump Curve Handle transform
                        if (conn.jumpCurveHandle == null) {
                            conn.jumpCurveHandle = new GameObject("HDL_jumpCurve_" + name + "_" + conn.wp.name).transform;
                            conn.jumpCurveHandle.position = midPos + Vector3.up * 7;
                        }
                        //Add a reference to the same transform to the paired previous connection of our Waypoint pair, so the handle is accessible from both sides
                        foreach (var p in conn.wp.prev) {
                            if (p.wp == this) {
                                if (p.jumpCurveHandle == null) {
                                    p.jumpCurveHandle = conn.jumpCurveHandle;
                                }
                            }
                        }

                        var HDL_Jump = conn.jumpCurveHandle;

                        //handle may never be lower than the highest point out of the two waypoints
                        float minHeight = Mathf.Max(transform.position.y, conn.wp.transform.position.y) + 1;
                        if (HDL_Jump.position.y < minHeight) {
                            HDL_Jump.position = new Vector3(midPos.x, minHeight, midPos.z);
                        } else {
                            HDL_Jump.position = new Vector3(midPos.x, HDL_Jump.position.y, midPos.z);
                        }

                        //parent ourselves to it
                        if (HDL_Jump.parent != transform)
                            HDL_Jump.parent = transform;

                       //Draw connection ports and lines
                        for (int i = 0; i <= 1; i++) {
                            Vector3 wpPos = ((i == 0) ? this : conn.wp).transform.position + lineOffset;

                            float diffY_selfToHandle = HDL_Jump.position.y - wpPos.y;
                            horizontalDiff = wpPos - HDL_Jump.position;
                            horizontalDiff.y = 0;

                            dir = horizontalDiff.normalized;
                            lastPos = HDL_Jump.position;

                            //add sideways offset to the connectors if they are bi-directional
                            foreach (var c in conn.wp.next) {
                                if (c.wp == this) {
                                    sideOffset = Vector3.Cross(Vector3.up * 0.45f, dir * ((i == 1) ? 1 : -1));
                                }
                            }

                            Color portColour = (i == 0) ? outboundConnectionColour : inboundConnectionColour;

                            steps = Mathf.Ceil(horizontalDiff.magnitude);
                            for (float x = 1; x <= steps; x += horizontalDiff.magnitude / steps) {
                                var newPos = HDL_Jump.position + dir * x + Vector3.down * Mathf.Pow(x / horizontalDiff.magnitude, 2) * diffY_selfToHandle;

                                //blend between in and output colours based on x
                                if (i == 0) {
                                    Gizmos.color = Color.Lerp(Color.Lerp(inboundConnectionColour, outboundConnectionColour, 0.5f), outboundConnectionColour, x / horizontalDiff.magnitude);
                                } else {
                                    Gizmos.color = Color.Lerp(Color.Lerp(inboundConnectionColour, outboundConnectionColour, 0.5f), inboundConnectionColour, x / horizontalDiff.magnitude);
                                }

                                Gizmos.DrawLine(lastPos + sideOffset, newPos + sideOffset);
                                lastPos = newPos;
                            }

                            
                            Gizmos.color = portColour;
                            Gizmos.DrawSphere(wpPos - (lastPos - wpPos).normalized + sideOffset, .4f);
                        }

                        Gizmos.color = Color.red / 1.4f;
                        Gizmos.DrawSphere(HDL_Jump.position + sideOffset, 0.4f);
                        Gizmos.color = ColWhite;

                        break;
                    #endregion
                    #region DrillTo
                    case WPConnection.Type.DrillTo:
                        //First destroy all children that do not apply to this type of path
                        if (transform.childCount != 0) {
                            if (conn.jumpCurveHandle != null) {
                                DestroyImmediate(conn.jumpCurveHandle.gameObject);
                                conn.jumpCurveHandle = null;
                            }
                        }

                        midPos = (conn.wp.transform.position + transform.position) / 2;
                        Vector3 handlePos = midPos + Vector3.down * 4;

                        //Add a new Drill Curve Handle transform
                        if (conn.drillPathHandle == null) {
                            conn.drillPathHandle = new GameObject("HDL_drillPath_" + name + "_" + conn.wp.name).transform;
                            conn.drillPathHandle.position = handlePos;
                            conn.drillPathHandle.gameObject.AddComponent(typeof(DrillPathHandle));
                        }
                        //Add a reference to the same transform to the paired previous connection of our Waypoint pair, so the handle is accessible from both sides
                        foreach (var p in conn.wp.prev) {
                            if (p.wp == this) {
                                if (p.drillPathHandle == null) {
                                    p.drillPathHandle = conn.drillPathHandle;
                                }
                            }
                        }

                        var HDL_Drill = conn.drillPathHandle;

                        DrillPathHandle HDL_Drill_component = HDL_Drill.gameObject.GetComponent(typeof(DrillPathHandle)) as DrillPathHandle;
                        conn.drillTime = HDL_Drill_component.drillTravelTime;

                        //handle may never be higher than the lowest point out of the two waypoints
                        float maxHeight = Mathf.Min(transform.position.y, conn.wp.transform.position.y) + 1;
                        if (HDL_Drill.position.y > maxHeight) {
                            HDL_Drill.position = new Vector3(midPos.x, maxHeight, midPos.z);
                        } else {
                            HDL_Drill.position = new Vector3(midPos.x, HDL_Drill.position.y, midPos.z);
                        }

                        //parent ourselves to it
                        if (HDL_Drill.parent != transform)
                            HDL_Drill.parent = transform;

                        //Draw connection ports and lines
                        for (int i = 0; i <= 1; i++) {
                            Vector3 wpPos = ((i == 0) ? this : conn.wp).transform.position + lineOffset;

                            float diffY_selfToHandle = HDL_Drill.position.y - wpPos.y;
                            horizontalDiff = wpPos - HDL_Drill.position;
                            horizontalDiff.y = 0;

                            dir = horizontalDiff.normalized;
                            lastPos = HDL_Drill.position;

                            //add sideways offset to the connectors if they are bi-directional
                            foreach (var c in conn.wp.next) {
                                if (c.wp == this) {
                                    sideOffset = Vector3.Cross(Vector3.up * 0.45f, dir * ((i == 1) ? 1 : -1));
                                }
                            }

                            Color portColour = (i == 0) ? outboundConnectionColour : inboundConnectionColour;

                            steps = Mathf.Ceil(horizontalDiff.magnitude);
                            bool odd = false;
                            for (float x = 1; x <= steps; x += horizontalDiff.magnitude / steps) {
                                var newPos = HDL_Drill.position + dir * x + Vector3.down * Mathf.Pow(x / horizontalDiff.magnitude, 2) * diffY_selfToHandle;

                                //draw a dashed line; skip every other segment of the line
                                if (odd = !odd) {
                                    //blend between in and output colours based on x
                                    if (i == 0) {
                                        Gizmos.color = Color.Lerp(Color.Lerp(inboundConnectionColour, outboundConnectionColour, 0.5f), outboundConnectionColour, x / horizontalDiff.magnitude);
                                    } else {
                                        Gizmos.color = Color.Lerp(Color.Lerp(inboundConnectionColour, outboundConnectionColour, 0.5f), inboundConnectionColour, x / horizontalDiff.magnitude);
                                    }

                                    Gizmos.DrawLine(lastPos + sideOffset, newPos + sideOffset);
                                }

                                lastPos = newPos;
                            }


                            Gizmos.color = portColour;
                            Gizmos.DrawSphere(wpPos - (lastPos - wpPos).normalized + sideOffset, .4f);
                        }

                        Gizmos.color = ColPurple;
                        Gizmos.DrawSphere(HDL_Drill.position + sideOffset, 0.4f);
                        Gizmos.color = ColWhite;
                        break;
                    #endregion
                    #region default
                    default:
                        if (transform.childCount != 0) {
                            if (conn.jumpCurveHandle != null) {
                                DestroyImmediate(conn.jumpCurveHandle.gameObject);
                                conn.jumpCurveHandle = null;
                            }

                            if (conn.drillPathHandle != null) {
                                DestroyImmediate(conn.drillPathHandle.gameObject);
                                conn.drillPathHandle = null;
                            }
                        }

                        horizontalDiff = conn.wp.transform.position - transform.position;

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
                            Gizmos.color = Color.Lerp(outboundConnectionColour, inboundConnectionColour, x / horizontalDiff.magnitude);

                            //also add a tiny bit of sideways offset relative to the direction; connect from "my right" to "your left"
                            Gizmos.DrawLine(lastPos + lineOffset + sideOffset, newPos + lineOffset + sideOffset);
                            lastPos = newPos;
                        }


                        Gizmos.color = inboundConnectionColour;
                        Gizmos.DrawSphere(conn.wp.transform.position + (transform.position - conn.wp.transform.position).normalized + lineOffset + sideOffset, .4f);

                        Gizmos.color = outboundConnectionColour;
                        Gizmos.DrawSphere(transform.position + (conn.wp.transform.position - transform.position).normalized + lineOffset + sideOffset, .4f);
                        break;
                        #endregion
                }
            }
        }

        Gizmos.color = ColWhite;
        Gizmos.DrawSphere(transform.position + lineOffset, 1f);

    }
}
