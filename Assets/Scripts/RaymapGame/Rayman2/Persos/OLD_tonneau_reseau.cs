//================================
//  By: Adsolution
//================================
using System.Collections.Generic;
using UnityEngine;
namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Floating Barrel
    /// </summary>
    public partial class OLD_tonneau_reseau : PersoController {
        public override AnimSFX[] animSfx => new AnimSFX[] { };

        protected override void OnStart() {
            SetRule("Wait");
            SetFriction(0.7f, 5);
            navRotYSpeed = 0;
            gravity = -2;
            moveSpeed = 3;
        }

        void Rule_Wait() {
            if (StoodOnByPerso(rayman))
                SetRule("Navigate");
        }

        void Rule_Navigate() {
            if (NavNearestWaypointGraph())
                SetRule("Sink");
        }

        void Rule_Sink() {
            SetFriction(1.5f, 5);
            if (pos.y - startPos.y < -2)
                Reset();
            else ApplyGravity();
        }
    }
}