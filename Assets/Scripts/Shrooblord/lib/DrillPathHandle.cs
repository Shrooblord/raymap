using UnityEngine;

namespace Shrooblord.lib {
    public class DrillPathHandle : BasePathHandle {
        [Tooltip("Total amount of time the Henchman will be drilling underground for, hidden from the game, in seconds. Setting this to 0 will make the drill feel more like a teleport than a drill, and is generally not recommended.")]
        [Range(0f, 5f)] public float drillTravelTime = 1.2f;

        public override Color handleColour => SHR_Colours.purple * 1.5f;
    }
}
