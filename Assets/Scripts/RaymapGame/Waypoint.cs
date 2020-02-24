using UnityEngine;
using System.Collections.Generic;

namespace RaymapGame {
    public class Waypoint : MonoBehaviour {
        public WaypointGraph graph;
        public int index;
        public Waypoint next, prev;
        public Vector3 pos;

        public static List<Waypoint> all = new List<Waypoint>();
        void Awake() {
            all.Add(this);
        }

        void Start() {
            graph = GetComponentInParent<WaypointGraph>();
            index = graph.waypoints.IndexOf(this);
            pos = transform.position;

            if (index + 1 < graph.waypoints.Count)
                next = graph.waypoints[index + 1];

            if (index - 1 >= 0)
                prev = graph.waypoints[index - 1];
        }
    }
}
