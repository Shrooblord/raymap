//================================
//  By: Adsolution
//================================

namespace RaymapGame.Rayman2.Persos {
    public class MOM_pirate_sbire_cine_2 : PersoController {
        protected override void OnStart() {
            SetRule("WaitForRayman");
        }

        void Rule_WaitForRayman() {
            if (rayman == null) return;

            if (DistToPerso(rayman) < 50)
                SetRule("JumpDownCine1");

            SetRotY(180);
            anim.Set(18, 0);
        }

        Timer t_delay1 = new Timer();
        void Rule_JumpDownCine1() {
            if (t_delay1.active) return;

            SetRotY(-11);
            anim.Set(4, 0);
            t_delay1.Start(2.7f, () => SetRule("JumpDownCine2"));
        }


        Timer t_delay2 = new Timer();
        void Rule_JumpDownCine2() {
            if (t_delay2.active) return;

            anim.Set(5, 0);

            t_delay2.Start(2.85f, () => {
                GetPerso("OLP_Sbire_Gnak").SetRule("TossKegs");
                SetVisibility(false);
            });
        }
    }
}
