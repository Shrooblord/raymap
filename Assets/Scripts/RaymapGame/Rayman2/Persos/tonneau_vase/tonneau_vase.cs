//================================
//  By: Adsolution
//================================
using System.Collections.Generic;
using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Floating Barrel
    /// </summary>
    public partial class tonneau_vase : PersoController {
        public override AnimSFX[] animSfx => new AnimSFX[] {
			
		};

		public static class State {
			public const int
				BarrelBayou = 0,
				BarrelTomb = 3;
		}
    }
}