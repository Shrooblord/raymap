using Shrooblord.lib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Makes it so that collider always show in-Editor even when not selected. Useful for trigger volumes!
 * Author: Jawchewa, 28/04/2017; modified by Shrooblord at on 25/02/2020.
 * Retrieved from https://answers.unity.com/questions/1346279/view-colliders-when-not-selected.html on 25/02/2020.
 */

[System.Serializable]
public class ShowCollider : MonoBehaviour {
    void OnDrawGizmos() {
        Gizmos.color = new Color(SHR_Colours.lime.r, SHR_Colours.lime.g, SHR_Colours.lime.b, 0.2f);
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
    }
}
