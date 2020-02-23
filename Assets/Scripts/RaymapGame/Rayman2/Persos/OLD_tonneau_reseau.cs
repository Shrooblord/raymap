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
            gravity = -3;
            moveSpeed = 4;
        }

        void Rule_Wait() {
            if (StoodOnByPerso(rayman))
                SetRule("Navigate");
        }

        void Rule_Navigate() {
            if (NavNearestWaypointGraph())
                SetRule("Sink");
        }

        public virtual void OnSink() { }

        void Rule_Sink() {
            SetFriction(2, 5);
            if (pos.y - startPos.y < -2) {
                Reset();
                OnSink();
            }
            else ApplyGravity();
        }
    }
}