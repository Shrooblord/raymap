using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    public partial class StdCam {
        void LevelRules() {
            if (rayman == null) return;

            var focus = new Vector3(-196, 0, 380);
            if (IsInLevel("Learn_31") && rayman.IsWithinCyl(focus, 15, 62))
                SetRule("FocusColumn", focus);

            focus = new Vector3(18, 0, 5);
            if (IsInLevel("Nave_15") && rayman.IsWithinCyl(focus, 20, 70))
                SetRule("FocusColumn", focus);


            else SetRule("Default");
        }
    }
}
