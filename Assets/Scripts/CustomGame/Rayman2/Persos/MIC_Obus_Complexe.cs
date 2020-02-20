//================================
//  By: Adsolution
//================================

namespace CustomGame.Rayman2.Persos {
    public class MIC_Obus_Complexe : PersoController {

        bool inAlertRadius => DistToPerso(rayman) < 15;
        Timer t_fallAsleep = new Timer();

        public void WakeUp() {
            SetRule("");
            t_fallAsleep.Abort();
            anim.Set(Anim.Shell.WakeUp);
            t_wakeUp.Start(1.5f, () => SetRule("Chasing"), false);
        }


        protected override void OnStart() {
            SetRule("Idle");
        }

        void Rule_Idle() {
            anim.Set(Anim.Shell.Idle);
            if (inAlertRadius) WakeUp();
            t_fallAsleep.Start(8, () => SetRule("Sleep"), false);
        }

        void Rule_Sleep() {
            anim.Set(Anim.Shell.Sleep);
            if (inAlertRadius) WakeUp(); 
        }

        Timer t_wakeUp = new Timer();
        
        void Rule_Chasing() {
            if (newRule)
                anim.Set(Anim.Shell.RunStart, 0);

            moveSpeed = 7;
            NavTowards(rayman.pos);
            col.StickToGround();
        }
    }
}