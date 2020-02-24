//================================
//  By: Adsolution
//================================
using System.Collections.Generic;
using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Magic Fist
    /// </summary>
    public partial class Alw_Projectile_Rayman_Model : projectiles {
        protected void Rule_Shoot() {
            if (newRule) {
                pos = rayman.pos;
                //rot = rayman.rot;
                //moveSpeed = 23;
            }
            //SetFriction(100, 100);
            //NavForwards();
        }
    }
}