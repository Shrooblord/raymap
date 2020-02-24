using UnityEngine;

namespace Shrooblord.lib {
    public class BasePathHandle : MonoBehaviour {
        [HideInInspector] public Color colourGizmo;

        [HideInInspector] public SHR_Waypoint parentWaypoint;
        [HideInInspector] public Vector3 sideOffset;

        public virtual Color handleColour => Color.cyan / 1.4f;
        public virtual Color handleColourSelected => Color.yellow;

        private Color gizmoHandleColour;

        [HideInInspector] public bool isSelected;

        private void OnDrawGizmosSelected() {
            gizmoHandleColour = handleColourSelected;
            isSelected = true;
        }

        protected void OnDrawGizmos() {
            transform.position += sideOffset;
            Gizmos.color = gizmoHandleColour;
            Gizmos.DrawSphere(transform.position, 0.4f);
            Gizmos.color = colourGizmo;
            gizmoHandleColour = handleColour;

            isSelected = false;
        }
    }
}
