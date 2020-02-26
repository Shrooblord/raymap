//================================
//  By: Adsolution & Shrooblord
//================================
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Magic Fist Projectile
    /// </summary>
    ///     
    public partial class RayMagicFist : Alw_Projectile_Rayman_Model {
        Timer StartDieTimer = new Timer();
        protected override void OnStart() {
            //pos = rayman.pos;
            rot = rayman.rot;
            moveSpeed = 23;
            SetFriction(100, 100);

            StartDieTimer.Start(1f, () => SetRule("Weakening"), false);
            SetRule("Shoot");
        }

        Timer DieTimer = new Timer();
        protected void Rule_Shoot() {
            if (newRule) DieTimer.Start(2f, () => SetRule("Die"), false);
            NavForwards();
        }

        protected void Rule_Weakening() {
            if (newRule) anim.Set(1);
            NavForwards();
        }

        protected void Rule_Die() {
            Destroy(gameObject);
        }
    }
}

