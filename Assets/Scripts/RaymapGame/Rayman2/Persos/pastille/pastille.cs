//================================
//  By: Adsolution
//================================
using System.Collections.Generic;
using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Spiral Door
    /// </summary>
    public partial class pastille : PersoController {
        protected override void OnStart() {
            SetRule("Default");
        }

        protected void Rule_Default() {
            float dist = DistToPerso(rayman);
            if (dist < 15)
                RotateY(1300f / Mathf.Clamp(dist, 3, 15), 1);
        }
    }
}