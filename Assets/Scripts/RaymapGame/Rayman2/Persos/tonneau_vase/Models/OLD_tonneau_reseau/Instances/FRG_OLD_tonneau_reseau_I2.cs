//================================
//  By: Adsolution
//================================
using System.Collections.Generic;
using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Floating Barrel
    /// </summary>
    public partial class FRG_OLD_tonneau_reseau_I2 : OLD_tonneau_reseau {
        OLD_tonneau_reseau nextTonneau;
        protected override void OnStart() {
            base.OnStart();
            nextTonneau = (OLD_tonneau_reseau)GetPerso("FRG_OLD_tonneau_reseau_I3");
        }
        public override void OnSink() {
            //nextTonneau.SetRule("Rise");
        }
    }
}