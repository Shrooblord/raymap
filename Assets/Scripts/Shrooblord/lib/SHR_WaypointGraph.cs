using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RaymapGame;
using RaymapGame.Rayman2.Persos;

//extend PersoController so that it contains a graph variable.
namespace RaymapGame {
    public partial class PersoController {
        public Shrooblord.lib.SHR_WaypointGraph graph;
    }
}

namespace Shrooblord.lib {
    [ExecuteInEditMode]
    public class SHR_WaypointGraph : MonoBehaviour {
        //colour of the in/outgoing connections nubs on waypoint nodes
        [Tooltip("Disable the highlighting of the Waypoints within this graph (useful for if you want to pick the graph colours).")]
        public bool turnOffSelectionHighlight;
        public Color outboundConnectionColour = new Color(0x90f/255, 0, 255);
        public Color inboundConnectionColour = new Color(255, 0x9Bf/255, 0x04f/255);

        //The Persos that will use this waypoint graph
        public List<string> persoNames = new List<string>();

        private void Update() {
            if (!Main.loaded) return;

            foreach (var n in persoNames) {
                var p = PersoController.GetPerso(n);
                if (p != null)
                    p.graph = this;
            }
        }

        public SHR_Waypoint GetNearestWaypoint(Vector3 pos) {
            SHR_Waypoint closest = null;
            float closeDist = 100000f;

            foreach (var wp in GetComponentsInChildren<SHR_Waypoint>()) {
                float dist = Vector3.Distance(pos, wp.transform.position);
                if (dist < closeDist) {
                    closeDist = dist;
                    closest = wp;
                }
            }

            return closest;
        }
    }
}
