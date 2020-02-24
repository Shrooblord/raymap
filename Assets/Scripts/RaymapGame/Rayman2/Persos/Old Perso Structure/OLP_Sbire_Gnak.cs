//================================
//  By: Adsolution
//================================

namespace RaymapGame.Rayman2.Persos {
    public partial class OLP_Sbire_Gnak : PersoController {
        protected override void OnStart() {
            SetVisibility(false);
        }

        void Rule_TossKegs() {
            SetVisibility(true);

            anim.Set(Anim.pirate_sbire.BarrelJuggle);
            LookAt2D(rayman.pos);
        }
    }
}