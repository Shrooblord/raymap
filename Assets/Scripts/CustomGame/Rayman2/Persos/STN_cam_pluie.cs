//================================
//  By: Adsolution
//================================

using UnityEngine;

namespace CustomGame.Rayman2.Persos {
    public class STN_cam_pluie : PersoController {
        protected override void OnStart() {
            SetRule("Follow");
        }

        Transform camTr => Camera.main.transform;

        void Rule_Follow() {
            pos = camTr.position;
        }
    }
}