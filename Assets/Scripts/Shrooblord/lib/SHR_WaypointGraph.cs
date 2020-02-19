using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomGame;
using CustomGame.Rayman2.Persos;

//extend PersoController so that it contains a graph variable.
namespace CustomGame {
    public partial class PersoController {
        public SHR_WaypointGraph graph;
    }
}

[ExecuteInEditMode]
public class SHR_WaypointGraph : MonoBehaviour
{
    public Color graphColor;

    //The Persos that will use this waypoint graph
    public List<string> persoNames = new List<string>();

    private void Update() {
        if (!Main.loaded) return;

        foreach (var n in persoNames) {
            var p = PersoController.GetPersoName(n);
            //var p = FindObjectOfType(System.Type.GetType($"CustomGame.Rayman2.Persos.{n}")) as PersoController;
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

    private void OnDrawGizmos() {
        graphColor = new Color(graphColor.r, graphColor.g, graphColor.b, .7f);
    }
}
