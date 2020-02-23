using UnityEngine;

public class DrillPathHandle : MonoBehaviour {
    [Tooltip("Total amount of time the Henchman will be drilling underground for, hidden from the game, in seconds. Setting this to 0 will make the drill feel more like a teleport than a drill, and is generally not recommended.")]
    [Range(0f, 5f)] public float drillTravelTime = 2.0f;
}
