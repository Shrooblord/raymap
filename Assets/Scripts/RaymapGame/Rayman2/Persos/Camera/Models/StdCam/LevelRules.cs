using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    public partial class StdCam {
        void LevelRules() {
            if (rule == "Free") return;
            Vector3 focus;

            if (IsInLevel("Learn_31") && rayman.IsWithinCyl(focus = new Vector3(-196, 0, 380), 15, 62))
                SetRule("FocusColumn", focus);

            if (IsInLevel("Nave_15") && rayman.IsWithinCyl(focus = new Vector3(18, 0, 5), 20, 70))
                SetRule("FocusColumn", focus);

            else SetRule("Follow");
        }
    }
}
