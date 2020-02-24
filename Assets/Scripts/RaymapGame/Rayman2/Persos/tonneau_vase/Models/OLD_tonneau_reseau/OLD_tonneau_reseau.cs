//================================
//  By: Adsolution
//================================
using System.Collections.Generic;
using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Floating Barrel
    /// </summary>
    public partial class OLD_tonneau_reseau : tonneau_vase {
        protected override void OnStart() {
            moveSpeed = 4;
            gravity = -3;
            navRotYSpeed = 0;
            SetFriction(1, 5);
            SetRule("Wait");
        }

        protected void Rule_Wait() {
            if (StoodOnByPerso(rayman))
                SetRule("Navigate");
        }

        protected void Rule_Navigate() {
            if (NavNearestWaypointGraph())
                SetRule("Sink");
        }

        public virtual void OnSink() { }

        protected void Rule_Sink() {
            SetFriction(2, 5);
            if (pos.y - startPos.y < -2) {
                Reset();
                OnSink();
            }
            else ApplyGravity();
        }
    }
}