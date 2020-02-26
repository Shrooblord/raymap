using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
/* Allows the user to create custom labels that hover over in-Editor objects and are displayed while the camera is a certain distance away.
 * Eg.
 * 
    Transform[] allChildren = GetComponentsInChildren<Transform>();
    foreach (Transform child in allChildren) {
        #if UNITY_EDITOR
            ObjDrawText.Draw(child,500,Color.red);  //Draw red Text only inside 500 units from Camera
        #endif
    }
 * 
 * 
 * Author: BlinD, 26/10/2011; modified by Shrooblord at on 23/02/2020.
 * Retrieved from https://forum.unity.com/threads/very-small-request-handle-label.107333/ on 23/02/2020.
 */

[System.Serializable]
public class ObjDrawText : MonoBehaviour {
    public static void Draw(Transform transf, string text, float cameraDistance, Color col) {
        GUIStyle style = new GUIStyle();
        Vector3 oncam = Camera.current.WorldToScreenPoint(transf.position);

        if ((oncam.x >= 0) && (oncam.x <= Camera.current.pixelWidth) && (oncam.y >= 0) && (oncam.y <= Camera.current.pixelHeight) && (oncam.z > 0) && (oncam.z < cameraDistance)) {
            style.normal.textColor = col;
            Handles.Label(transf.position, text, style);
        }
    }
}
#endif
