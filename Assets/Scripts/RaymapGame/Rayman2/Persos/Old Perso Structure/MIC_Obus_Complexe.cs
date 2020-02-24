//================================
//  By: Adsolution
//================================

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Walking Shell
    /// </summary>
    public class MIC_Obus_Complexe : PersoController {

        bool inAlertRadius => DistToPerso(rayman) < 15;
        Timer t_fallAsleep = new Timer();

        public void WakeUp() {
            SetRule("");
            t_fallAsleep.Abort();
            anim.Set(Anim.Shell.WakeUp);
            t_wakeUp.Start(1.75f, () => SetRule("Chasing"), false);
        }


        protected override void OnStart() {
            SetShadow(true);
            col.wallEnabled = true;
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
        Timer t_runStart = new Timer();

        void Rule_Chasing() {
            if (newRule) {
                anim.Set(Anim.Shell.RunStart, 0);
                t_runStart.Start(0.3f);
            }

            if (t_runStart.finished) {
                anim.SetSpeed(moveSpeed * 8);
                moveSpeed = 8;
                navRotYSpeed = 3;
                NavTowards(rayman.pos);
                col.StickToGround();
                if (col.ground.AnyGround) {
                    velY = 0;
                }
                else {
                    ApplyGravity();
                }
            }

            if (CheckCollisionZone(rayman, OpenSpace.Collide.CollideType.ZDM)) {
                Reset();
                rayman.Despawn();
            }
        }
    }
}