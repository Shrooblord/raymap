//================================
//  By: Adsolution
//================================
using OpenSpace.Object;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Magic Fist
    /// </summary>
    /// 
//     public static class PersoProjectile : Perso {
//         public static Perso Func_GenerateObject(this Perso p, Type persoType, Vector3 position) {
// 
//         }
//     }
    
    public partial class Alw_Projectile_Rayman_Model : projectiles {
        protected override void OnStart() {
            SetRule("Shoot");
        }

        protected void Rule_Shoot() {
            if (newRule) {
                pos = rayman.pos;
                rot = rayman.rot;
                moveSpeed = 23;
            }
            SetFriction(100, 100);
            NavForwards();
        }
    }
}