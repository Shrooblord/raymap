//================================
//  By: Adsolution
//================================
using System.Collections.Generic;
using UnityEngine;
namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Planks
    /// </summary>
    public partial class STN_planches : PersoController {
        public override AnimSFX[] animSfx => new AnimSFX[] { };

        protected override void OnStart() {
            SetRule("Default");
        }

        void Rule_Default() {
            //if (rayman != null)
                //if (StoodOnByPerso(rayman))
                    //Destroy(gameObject);
        }
    }
}