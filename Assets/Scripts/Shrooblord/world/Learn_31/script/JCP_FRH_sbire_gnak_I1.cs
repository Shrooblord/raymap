using UnityEngine;

namespace CustomGame.Rayman2.Persos {
    public class JCP_FRH_sbire_gnak_I1 : PersoController {

        protected override void OnStart() {
            pos = new Vector3(-193.61f, 23.84f, 369.45f);
            rot = Quaternion.Euler(0, 0, 0);

            SetRule("Sleeping");
        }

        //animNotify sfx
        public override AnimSFX[] animSfx => new AnimSFX[] {
            //running animation
            new AnimSFX(2, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Footstep",
                volume = 0.60f,
            }, 1, 10),
        };

        //Rulzez
        void Rule_Sleeping() {
            anim.Set(48);

            if (rayman != null) {
                if (Vector3.Distance(pos, rayman.pos) < 10) {
                    SetRule("WokeUp");
                }
            }
        }

        Timer wakeUpTimer = new Timer();
        void Rule_WokeUp() {
            anim.Set(49);

            //timer for 1s
            wakeUpTimer.Start(1f, () => {
                SetRule("Surprise");
            }, false);
        }

        Timer surpriseTimer = new Timer();
        void Rule_Surprise() {
            anim.Set(6);

            //timer for 0.9s
            surpriseTimer.Start(0.9f, () => { SetRule("RunUp"); }, false);
        }

        Timer runUpTimer = new Timer();
        void Rule_RunUp() {
            //align with floor geometry
            col.StickToGround();
            col.groundDepth = 0.8f;

            //move and look at where we're headed
            Vector3 targetPos = new Vector3(-178.24f, 24.53f, 380.14f);
            //pos = Vector3.MoveTowards(pos, targetPos, 10f * dt);
            SetLookAt2D(targetPos, 180);
            moveSpeed = 10f;
            velXZ = (targetPos - pos).normalized * moveSpeed;

            runUpTimer.Start(4f, () => { SetRule("Aim"); }, false);

            //If we've arrived at the destination before the timer runs out, abort the timer and just continue.
            if (Vector3.Distance(pos, targetPos) <= 1) {
                runUpTimer.onFinishAction();
                runUpTimer.Abort();
            }
        }

        void Rule_Aim() {
            anim.Set(8);
            velXZ = Vector3.zero;
            SetRule("LookAt");
        }

        void Rule_LookAt() {
            SetLookAt2D(rayman.pos, 180);
        }
    }
}
