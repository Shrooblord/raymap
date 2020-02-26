using System.Collections.Generic;
using UnityEngine;

namespace Shrooblord.lib {
    //All possible connection types between Waypoints.
    [System.Serializable]
    public class WPConnection {
        public enum Type { Walk, Jump, Drill, Parachute, MoonJump, Teleport, NONE };
        public SHR_Waypoint wp;
        public Type type;
        public float drillTime; //how long it takes for the Henchman to Drill from this location to the target Waypoint pair

        //Connection settings transforms. All configurable variables relevant to the Connection are stored in its transform handle in a child component.
        public BasePathHandle pathHandle;
        public WPConnection.Type previousConnectionType;

        public WPConnection(SHR_Waypoint wp, Type type) {
            this.wp = wp;
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
        private Color outboundConnectionColourSelected;
        private Color outboundConnectionLineColour;

        private Color inboundConnectionColour;
        private Color inboundConnectionColourSelected;
        private Color inboundConnectionLineColour;

        private Color colourWaypoint;
        private Color colourWaypointSelected;
        private Color colourGizmo;

        public WPConnection GetRandomConnectionInList(List<WPConnection> connections, bool allowNext = true, bool allowPrevious = true) {
            List<WPConnection> valid = new List<WPConnection>();

            //only check the subset of connections that is within our own connections
            foreach (var c in connections) {
                if (allowNext) {
                    foreach (var n in next) {
                        if (c == n) valid.Add(c);
                    }
                }

                if (allowPrevious) {
                    foreach (var p in prev) {
                        if (c == p) valid.Add(c);
                    }
                }
            }

            return valid[Random.Range(0, valid.Count)];
        }
        public WPConnection GetRandomNextConnection() => GetRandomConnectionInList(next, true, false);
        public WPConnection GetRandomPreviousConnection() => GetRandomConnectionInList(prev, false, true);

        public bool ConnectionInNext(WPConnection conn) => next.Contains(conn);
        public bool ConnectionInPrev(WPConnection conn) => prev.Contains(conn);

        private void Update() {
            if ((graph = GetComponentInParent<SHR_WaypointGraph>()) == null) return;

            //colour of the in/outgoing connections nubs on waypoint nodes
            outboundConnectionColour = graph.outboundConnectionColour;
            inboundConnectionColour = graph.inboundConnectionColour;

            colourWaypoint = Color.Lerp(outboundConnectionColour, inboundConnectionColour, 0.5f);

            //finding selection colour
            colourWaypointSelected = SHR_Colours.Invert(colourWaypoint);
            outboundConnectionColourSelected = SHR_Colours.Invert(outboundConnectionColour);
            inboundConnectionColourSelected = SHR_Colours.Invert(inboundConnectionColour);
        }

        private void colourSelection() {
            if (!graph.turnOffSelectionHighlight) {
                colourGizmo = colourWaypointSelected;
                outboundConnectionLineColour = outboundConnectionColourSelected;
                inboundConnectionLineColour = inboundConnectionColourSelected;
            }
        }

        private void OnDrawGizmosSelected() {
                colourSelection();
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

                    #region Shared Functions
                    //functions shared by all Waypoint Connections
                    void createNewPathHandle() {
                        midPos = (conn.wp.transform.position + transform.position) / 2;

                        //Add a new Path Handle transform
                        if (conn.pathHandle == null) {
                            var GO = new GameObject("HDL_" + conn.type + "Path_" + name + "_" + conn.wp.name);

                            //check which type of Path Handle we have
                            switch (conn.type) {
                                case WPConnection.Type.Jump:
                                    conn.pathHandle = GO.AddComponent<JumpPathHandle>();
                                    conn.pathHandle.transform.position = midPos + Vector3.up * 7;
                                    break;

                                case WPConnection.Type.Drill:
                                    conn.pathHandle = GO.AddComponent<DrillPathHandle>();
                                    conn.pathHandle.transform.position = midPos + Vector3.down * 4;
                                    break;

                                default:
                                    conn.pathHandle = GO.AddComponent<WalkPathHandle>();
                                    break;
                            }
                        }
                        //Add a reference to the same transform to the paired previous connection of our Waypoint pair, so the handle is accessible from both sides
                        foreach (var p in conn.wp.prev) {
                            if (p.pathHandle == null) {
                                if (p.wp == this) {
                                    p.pathHandle = conn.pathHandle;
                                }
                            }
                        }
                    }

                    void initPathHandle() {
                        createNewPathHandle();

                        conn.pathHandle.parentWaypoint = this;
                        conn.pathHandle.colourGizmo = colourGizmo;

                        if (conn.pathHandle is JumpPathHandle HDLJump) {
                            JumpPathHandle HDL = HDLJump;

                            //handle may never be lower than the highest point out of the two waypoints
                            float minHeight = Mathf.Max(transform.position.y, conn.wp.transform.position.y) + 1;
                            if (HDL.transform.position.y < minHeight) {
                                HDL.transform.position = new Vector3(midPos.x, minHeight, midPos.z);
                            } else {
                                HDL.transform.position = new Vector3(midPos.x, HDL.transform.position.y, midPos.z);
                            }

                        } else if (conn.pathHandle is DrillPathHandle HDLDrill) {
                            DrillPathHandle HDL = HDLDrill;

                            //handle is never allowed to be higher than the lowest point out of the two waypoints
                            float maxHeight = Mathf.Min(transform.position.y, conn.wp.transform.position.y) + 1;
                            if (HDL.transform.position.y > maxHeight) {
                                HDL.transform.position = new Vector3(midPos.x, maxHeight, midPos.z);
                            } else {
                                HDL.transform.position = new Vector3(midPos.x, HDL.transform.position.y, midPos.z);
                            }

                            conn.drillTime = HDL.drillTravelTime;

                        } else if (conn.pathHandle is WalkPathHandle HDLWalk) {
                            HDLWalk.transform.position = midPos + lineOffset;
                        }


                        //parent ourselves to it
                        if (conn.pathHandle.transform.parent != transform)
                            conn.pathHandle.transform.parent = transform;


                        //If the Path Handle is currently selected, draw the parent waypoint, our lines and connector nubs as if they're also selected
                        if (conn.pathHandle.isSelected)
                            colourSelection();

                    }
                    #endregion

                    switch (conn.type) {
                        #region Jump
                        case WPConnection.Type.Jump:
                            initPathHandle();
                            JumpPathHandle HDL_Jump = (JumpPathHandle)conn.pathHandle;

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

                                Color portColour = (i == 0) ? outboundConnectionLineColour : inboundConnectionLineColour;

                                steps = Mathf.Ceil(horizontalDiff.magnitude);
                                for (float x = 1; x <= steps; x += horizontalDiff.magnitude / steps) {
                                    var newPos = HDL_Jump.transform.position + dir * x + Vector3.down * Mathf.Pow(x / horizontalDiff.magnitude, 2) * diffY_selfToHandle;

                                    //blend between in and output colours based on x
                                    if (i == 0) {
                                        Gizmos.color = Color.Lerp(Color.Lerp(inboundConnectionLineColour, outboundConnectionLineColour, 0.5f), outboundConnectionLineColour, x / horizontalDiff.magnitude);
                                    } else {
                                        Gizmos.color = Color.Lerp(Color.Lerp(inboundConnectionLineColour, outboundConnectionLineColour, 0.5f), inboundConnectionLineColour, x / horizontalDiff.magnitude);
                                    }

                                    Gizmos.DrawLine(lastPos + sideOffset, newPos + sideOffset);
                                    lastPos = newPos;
                                }

                                Gizmos.color = portColour;
                                Gizmos.DrawSphere(wpPos - (lastPos - wpPos).normalized + sideOffset, .4f);
                            }
                            break;
                        #endregion
                        #region Drill
                        case WPConnection.Type.Drill:
                            initPathHandle();
                            DrillPathHandle HDL_Drill = (DrillPathHandle)conn.pathHandle;

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

                                Color portColour = (i == 0) ? outboundConnectionLineColour : inboundConnectionLineColour;

                                steps = Mathf.Ceil(horizontalDiff.magnitude);
                                bool odd = false;
                                for (float x = 1; x <= steps; x += horizontalDiff.magnitude / steps) {
                                    var newPos = HDL_Drill.transform.position + dir * x + Vector3.down * Mathf.Pow(x / horizontalDiff.magnitude, 2) * diffY_selfToHandle;

                                    //draw a dashed line; skip every other segment of the line
                                    if (odd = !odd) {
                                        //blend between in and output colours based on x
                                        if (i == 0) {
                                            Gizmos.color = Color.Lerp(Color.Lerp(inboundConnectionLineColour, outboundConnectionLineColour, 0.5f), outboundConnectionLineColour, x / horizontalDiff.magnitude) * 1.5f;
                                        } else {
                                            Gizmos.color = Color.Lerp(Color.Lerp(inboundConnectionLineColour, outboundConnectionLineColour, 0.5f), inboundConnectionLineColour, x / horizontalDiff.magnitude) * 1.5f;
                                        }

                                        Gizmos.DrawLine(lastPos + sideOffset, newPos + sideOffset);
                                    }

                                    lastPos = newPos;
                                }


                                Gizmos.color = portColour;
                                Gizmos.DrawSphere(wpPos - (lastPos - wpPos).normalized + sideOffset, .4f);
                            }
                            break;
                        #endregion
                        #region default (Walk)
                        default:
                            initPathHandle();
                            WalkPathHandle HDL_Walk = (WalkPathHandle)conn.pathHandle;

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
                                Gizmos.color = Color.Lerp(outboundConnectionLineColour, inboundConnectionLineColour, x / horizontalDiff.magnitude);

                                //also add a tiny bit of sideways offset relative to the direction; connect from "my right" to "your left"
                                Gizmos.DrawLine(lastPos + lineOffset + sideOffset, newPos + lineOffset + sideOffset);
                                lastPos = newPos;
                            }

                            Gizmos.color = inboundConnectionLineColour;
                            Gizmos.DrawSphere(conn.wp.transform.position + (transform.position - conn.wp.transform.position).normalized + lineOffset + sideOffset, .4f);

                            Gizmos.color = outboundConnectionLineColour;
                            Gizmos.DrawSphere(transform.position + (conn.wp.transform.position - transform.position).normalized + lineOffset + sideOffset, .4f);
                            break;
                            #endregion
                    }
                }

                conn.previousConnectionType = conn.type;
            }

            Gizmos.color = colourGizmo;
            Gizmos.DrawSphere(transform.position + lineOffset, 1f);

            //reset "selection colour" to normal
            colourGizmo = colourWaypoint;
            outboundConnectionLineColour = outboundConnectionColour;
            inboundConnectionLineColour = inboundConnectionColour;
        }
    }
}
