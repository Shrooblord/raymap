using UnityEngine;

//extend PersoController so that it contains a triggerVolume variable.
namespace RaymapGame {
    public partial class PersoController {
        public Shrooblord.lib.TriggerVolume triggerVolume;
    }
}

namespace Shrooblord.lib {
    public class TriggerVolume : MonoBehaviour {
        protected ShowCollider colliderVisuals;

        private void OnDrawGizmos() {
            if (transform.gameObject.GetComponent<ShowCollider>() == null) {
                colliderVisuals = transform.gameObject.AddComponent<ShowCollider>();
            }
        }
    }
}
