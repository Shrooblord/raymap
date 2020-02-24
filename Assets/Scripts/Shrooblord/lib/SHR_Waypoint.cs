using System.Collections.Generic;
using UnityEngine;

namespace Shrooblord.lib {
    //All possible connection types between Waypoints.
    [System.Serializable]
    public class WPConnection {
        public enum Type { WalkTo, JumpTo, DrillTo, ParachuteTo, TeleportTo, NONE };
        public SHR_Waypoint wp;
        public Type type;
        public float drillTime; //how long it takes for the Henchman to Drill from this location to the target Waypoint pair

        //Connection settings transforms. All configurable variables relevant to the Connection are stored in its transform handle in a child component.
        public BasePathHandle pathHandle;
        public WPConnection.Type previousConnectionType;

        public WPConnection(SHR_Waypoint wpTo, Type type) {
            this.wp = wpTo;
            this.type = type;
        }

        public WPConnection() { }
    }

    [ExecuteInEditMode]
    public class SHR_Waypoint : MonoBehaviour {
        public SHR_WaypointGraph graph;

        //public Transform jumpCurveHandle;
        public float waitHere = 0.0f;       //when > 0, it causes the Perso to stop and wait on this WP for this many seconds

        public List<WPConnection> next = new List<WPConnection>();
        public List<WPConnection> prev = new List<WPConnection>();

        private Vector3 lineOffset = new Vector3(0, 1.0f, 0);
        private Color outboundConnectionColour;
        private Color inboundConnectionColour;

        private Color colourWaypoint;
        private Color colourWaypointSelected;
        private Color colourGizmo;

        public SHR_Waypoint GetRandomNextWaypoint() {
            return next[Random.Range(0, next.Count)].wp;
        }

        private void Update() {
            if ((graph = GetComponentInParent<SHR_WaypointGraph>()) == null) return;

            //colour of the in/outgoing connections nubs on waypoint nodes
            outboundConnectionColour = graph.outboundConnectionColour;
            inboundConnectionColour = graph.inboundConnectionColour;

            colourWaypoint = Color.Lerp(outboundConnectionColour, inboundConnectionColour, 0.5f);
            
            //finding selection colour (invert colourWaypoint)
            float hue, sat, val;
            Color.RGBToHSV(colourWaypoint, out hue, out sat, out val);
            hue = (hue + 0.5f) % 1f; //find colour on opposite of colour wheel

            colourWaypointSelected = Color.HSVToRGB(hue, sat, val);
        }

        private void OnDrawGizmosSelected() {
            colourGizmo = colourWaypointSelected;
        }

        private void OnDrawGizmos() {
            if (graph == null) return;

            foreach (var conn in next) {
                if (conn != null) {
                    Gizmos.color = colourGizmo;

                    var horizontalDiff = Vector3.zero;
                    float steps;
                    var dir = Vector3.zero;
                    var lastPos = Vector3.zero;

                    Vector3 sideOffset = Vector3.zero;
                    Vector3 midPos = Vector3.zero;

                    //First destroy the old Path Handle
                    if (conn.type != conn.previousConnectionType) {
                        if (conn.pathHandle != null) {
                            DestroyImmediate(conn.pathHandle.gameObject);
                            conn.pathHandle = null;
                        }
                    }

                    switch (conn.type) {
                        #region JumpTo
                        case WPConnection.Type.JumpTo:
                            midPos = (conn.wp.transform.position + transform.position) / 2;

                            JumpPathHandle HDL_Jump;

                            //Add a new Path Handle transform
                            if (conn.pathHandle == null) {
                                var GO = new GameObject("HDL_jumpPath_" + name + "_" + conn.wp.name);
                                conn.pathHandle = GO.AddComponent<JumpPathHandle>();
                                conn.pathHandle.transform.position = midPos + Vector3.up * 7;
                            }
                            //Add a reference to the same transform to the paired previous connection of our Waypoint pair, so the handle is accessible from both sides
                            foreach (var p in conn.wp.prev) {
                                if (p.pathHandle == null) {
                                    if (p.wp == this) {
                                        p.pathHandle = conn.pathHandle;
                                    }
                                }
                            }

                            if (conn.pathHandle is JumpPathHandle JumpHDL) {
                                HDL_Jump = JumpHDL;

                                HDL_Jump.parentWaypoint = this;
                                HDL_Jump.colourGizmo = colourGizmo;

                                //handle may never be lower than the highest point out of the two waypoints
                                float minHeight = Mathf.Max(transform.position.y, conn.wp.transform.position.y) + 1;
                                if (HDL_Jump.transform.position.y < minHeight) {
                                    HDL_Jump.transform.position = new Vector3(midPos.x, minHeight, midPos.z);
                                } else {
                                    HDL_Jump.transform.position = new Vector3(midPos.x, HDL_Jump.transform.position.y, midPos.z);
                                }

                                //parent ourselves to it
                                if (HDL_Jump.transform.parent != transform)
                                    HDL_Jump.transform.parent = transform;

                                //HDL_Jump.transform.position += sideOffset;

                                //Draw connection ports and lines
                                for (int i = 0; i <= 1; i++) {
                                    Vector3 wpPos = ((i == 0) ? this : conn.wp).transform.position + lineOffset;

                                    float diffY_selfToHandle = HDL_Jump.transform.position.y - wpPos.y;
                                    horizontalDiff = wpPos - HDL_Jump.transform.position;
                                    horizontalDiff.y = 0;

                                    dir = horizontalDiff.normalized;
                                    lastPos = HDL_Jump.transform.position;

                                    //add sideways offset to the connectors if they are bi-directional
                                    foreach (var c in conn.wp.next) {
                                        if (c.wp == this) {
                                            sideOffset = Vector3.Cross(Vector3.up * 0.45f, dir * ((i == 1) ? 1 : -1));
                                            HDL_Jump.sideOffset = sideOffset;
                                        }
                                    }

                                    Color portColour = (i == 0) ? outboundConnectionColour : inboundConnectionColour;

                                    steps = Mathf.Ceil(horizontalDiff.magnitude);
                                    for (float x = 1; x <= steps; x += horizontalDiff.magnitude / steps) {
                                        var newPos = HDL_Jump.transform.position + dir * x + Vector3.down * Mathf.Pow(x / horizontalDiff.magnitude, 2) * diffY_selfToHandle;

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
                            }
                            break;
                        #endregion
                        #region DrillTo
                        case WPConnection.Type.DrillTo:
                            midPos = (conn.wp.transform.position + transform.position) / 2;
                            Vector3 handlePos = midPos + Vector3.down * 4;

                            DrillPathHandle HDL_Drill;

                            //Add a new Path Handle transform
                            if (conn.pathHandle == null) {
                                var GO = new GameObject("HDL_drillPath_" + name + "_" + conn.wp.name);
                                conn.pathHandle = GO.AddComponent<DrillPathHandle>();
                                conn.pathHandle.transform.position = handlePos;
                            }
                            //Add a reference to the same transform to the paired previous connection of our Waypoint pair, so the handle is accessible from both sides
                            foreach (var p in conn.wp.prev) {
                                if (p.pathHandle == null) {
                                    if (p.wp == this) {
                                        p.pathHandle = conn.pathHandle;
                                    }
                                }
                            }

                            if (conn.pathHandle is DrillPathHandle drillHDL) {
                                HDL_Drill = drillHDL;

                                //handle is never allowed to be higher than the lowest point out of the two waypoints
                                float maxHeight = Mathf.Min(transform.position.y, conn.wp.transform.position.y) + 1;
                                if (HDL_Drill.transform.position.y > maxHeight) {
                                    HDL_Drill.transform.position = new Vector3(midPos.x, maxHeight, midPos.z);
                                } else {
                                    HDL_Drill.transform.position = new Vector3(midPos.x, HDL_Drill.transform.position.y, midPos.z);
                                }

                                HDL_Drill.parentWaypoint = this;
                                conn.drillTime = HDL_Drill.drillTravelTime;
                                HDL_Drill.colourGizmo = colourGizmo;

                                //parent ourselves to it
                                if (HDL_Drill.transform.parent != transform)
                                    HDL_Drill.transform.parent = transform;


                                //Draw connection ports and lines
                                for (int i = 0; i <= 1; i++) {
                                    Vector3 wpPos = ((i == 0) ? this : conn.wp).transform.position + lineOffset;

                                    float diffY_selfToHandle = HDL_Drill.transform.position.y - wpPos.y;
                                    horizontalDiff = wpPos - HDL_Drill.transform.position;
                                    horizontalDiff.y = 0;

                                    dir = horizontalDiff.normalized;
                                    lastPos = HDL_Drill.transform.position;

                                    //add sideways offset to the connectors if they are bi-directional
                                    foreach (var c in conn.wp.next) {
                                        if (c.wp == this) {
                                            sideOffset = Vector3.Cross(Vector3.up * 0.45f, dir * ((i == 1) ? 1 : -1));
                                            HDL_Drill.sideOffset = sideOffset;
                                        }
                                    }

                                    Color portColour = (i == 0) ? outboundConnectionColour : inboundConnectionColour;

                                    steps = Mathf.Ceil(horizontalDiff.magnitude);
                                    bool odd = false;
                                    for (float x = 1; x <= steps; x += horizontalDiff.magnitude / steps) {
                                        var newPos = HDL_Drill.transform.position + dir * x + Vector3.down * Mathf.Pow(x / horizontalDiff.magnitude, 2) * diffY_selfToHandle;

                                        //draw a dashed line; skip every other segment of the line
                                        if (odd = !odd) {
                                            //blend between in and output colours based on x
                                            if (i == 0) {
                                                Gizmos.color = Color.Lerp(Color.Lerp(inboundConnectionColour, outboundConnectionColour, 0.5f), outboundConnectionColour, x / horizontalDiff.magnitude) * 1.5f;
                                            } else {
                                                Gizmos.color = Color.Lerp(Color.Lerp(inboundConnectionColour, outboundConnectionColour, 0.5f), inboundConnectionColour, x / horizontalDiff.magnitude) * 1.5f;
                                            }

                                            Gizmos.DrawLine(lastPos + sideOffset, newPos + sideOffset);
                                        }

                                        lastPos = newPos;
                                    }


                                    Gizmos.color = portColour;
                                    Gizmos.DrawSphere(wpPos - (lastPos - wpPos).normalized + sideOffset, .4f);
                                }
                            }
                            break;
                        #endregion
                        #region default (WalkTo)
                        default:
                            midPos = (conn.wp.transform.position + transform.position) / 2 + lineOffset;
                            WalkPathHandle HDL_Walk;

                            //Add a new Path Handle transform
                            if (conn.pathHandle == null) {
                                var GO = new GameObject("HDL_walkPath_" + name + "_" + conn.wp.name);
                                conn.pathHandle = GO.AddComponent<WalkPathHandle>();
                            }
                            //Add a reference to the same transform to the paired previous connection of our Waypoint pair, so the handle is accessible from both sides                        
                            foreach (var p in conn.wp.prev) {
                                if (p.pathHandle == null) {
                                    if (p.wp == this) {
                                        p.pathHandle = conn.pathHandle;
                                    }
                                }
                            }

                            if (conn.pathHandle is WalkPathHandle WalkHDL) {
                                HDL_Walk = WalkHDL;

                                HDL_Walk.parentWaypoint = this;
                                HDL_Walk.colourGizmo = colourGizmo;

                                HDL_Walk.transform.position = midPos;

                                //parent ourselves to it
                                if (HDL_Walk.transform.parent != transform)
                                    HDL_Walk.transform.parent = transform;


                                //path drawing and connectors
                                horizontalDiff = conn.wp.transform.position - transform.position;
                                dir = horizontalDiff.normalized;
                                lastPos = transform.position;

                                //add sideways offset to the connectors if they are bi-directional
                                foreach (var c in conn.wp.next) {
                                    if (c.wp == this) {
                                        sideOffset = Vector3.Cross(Vector3.up * 0.45f, dir);
                                        HDL_Walk.sideOffset = sideOffset;
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
                            }
                            break;
                            #endregion
                    }
                }

                conn.previousConnectionType = conn.type;
            }

            Gizmos.color = colourGizmo;
            Gizmos.DrawSphere(transform.position + lineOffset, 1f);

            colourGizmo = colourWaypoint;
        }
    }
}
