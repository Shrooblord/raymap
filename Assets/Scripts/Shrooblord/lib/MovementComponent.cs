using OpenSpace;
using OpenSpace.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shrooblord.lib {
    public class MovementComponent : MonoBehaviour {
        public bool moving = false;
        public IEnumerator moveToLerp(PersoBehaviour pb, Vector3 pointA, Vector3 pointB, float time) {
            if (!moving) {                     // Do nothing if already moving
                moving = true;                 // Set flag to true
                var t = 0f;
                while (t < 1.0f) {
                    t += Time.deltaTime / time; // Sweeps from 0 to 1 in time seconds
                    pb.transform.position = Vector3.Lerp(pointA, pointB, t); // Set position proportional to t
                    yield return null;         // Leave the routine and return here in the next frame
                }
                moving = false;             // Finished moving
            }
        }
    }
}
