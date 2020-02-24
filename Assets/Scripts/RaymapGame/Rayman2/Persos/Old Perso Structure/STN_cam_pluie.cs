//================================
//  By: Adsolution
//================================

using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    public class STN_cam_pluie : PersoController {
        Camera cam;

        protected override void OnStart() {
            cam = GetPersoModel<StdCam>("StdCam");
            SetRule("Follow");
        }

        void Rule_Follow() {
            pos = cam.pos;
        }
    }
}