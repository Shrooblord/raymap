//================================
//  By: Adsolution
//================================

using UnityEngine;
using OpenSpace.Collide;

namespace CustomGame.Rayman2.Persos {
    public class PHL_Actor_Model2 : PersoController {
        protected override void OnStart() {
            SetRule("CheckRayman");
        }

        Timer t_hyst = new Timer();
        void Rule_CheckRayman() {
            if (rayman == null || t_hyst.active) return;

            if (CheckCollisionZone(rayman, CollideType.ZDR)) {

                rayman.pos = pos + Vector3.up;
                rayman.velXZ = Vector3.zero;
                rayman.Jump(9, true);

                anim.Set(1);
                t_hyst.Start(0.2f);
            }
        }
    }
}