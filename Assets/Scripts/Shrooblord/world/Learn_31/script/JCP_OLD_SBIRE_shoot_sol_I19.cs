using UnityEngine;

namespace CustomGame.Rayman2.Persos {
    public class JCP_OLD_SBIRE_shoot_sol_I19 : PersoController {

        public SHR_WaypointGraph wpGraph;
        SHR_Waypoint targetWP;

        protected override void OnStart() {
            pos = new Vector3(-187.8f, 25.51f, 383.39f);
            rot = Quaternion.Euler(0, -106.54f, 0);

            //Colour the Henchman. 1 = Red; 2 = Purple
            GetComponent<PersoBehaviour>().poListIndex = 1;

            SetRule("Sleeping");
        }

        //animNotify sfx
        public override AnimSFX[] animSfx => new AnimSFX[] {
            //running animation footstep plants
            new AnimSFX(2, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Footstep/Walk",
                space = SFXPlayer.Space.Point,          //make the sound originate from specifically the Henchman
                volume = 0.60f,
            }, 1, 10),

            //snoring while asleep
            new AnimSFX(48, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Voice/Snoring",
                space = SFXPlayer.Space.Point,
                mode = SFXPlayer.Mode.Consecutive,
            }, 1),

            //wake up in surprise
            //surprise sound
            new AnimSFX(49, new SFXPlayer.Info {
                path = "Rayman2/Henchman/General/pimoteur",
                space = SFXPlayer.Space.Point,
                polyphony = SFXPlayer.Polyphony.Poly,
            }, 1),
            //heavy foot plant sound
            new AnimSFX(49, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Footstep/Land/",
                space = SFXPlayer.Space.Point,
                polyphony = SFXPlayer.Polyphony.Poly,
            }, 16),

            //swivel head in surprise
            new AnimSFX(6, new SFXPlayer.Info {
                path = "Rayman2/Henchman/General/surpris",
                space = SFXPlayer.Space.Point,
                polyphony = SFXPlayer.Polyphony.Poly,
            }, 1),

            //idle
            new AnimSFX(16, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Voice/Idle",
                space = SFXPlayer.Space.Point,
                polyphony = SFXPlayer.Polyphony.Poly,
                mode = SFXPlayer.Mode.RandomNoRepeat
            }, 1),
        };

        void goToNearestWaypoint() {
            if (graph != null)
                targetWP = graph.GetNearestWaypoint(pos);
        }

        //Rulzez
        void Rule_Sleeping() {
            anim.Set(48);

            goToNearestWaypoint();

            if (rayman != null) {
                if (Vector3.Distance(pos, rayman.pos) < 6) {
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
            surpriseTimer.Start(0.9f, () => { SetRule("RunAround"); }, false);
        }

        Timer runUpTimer = new Timer();
        void Rule_RunAround() {
            //align with floor geometry
            col.StickToGround();
            col.groundDepth = 0.8f;

            //move and look at where we're headed
            SetLookAt2D(targetWP.transform.position, 180);
            moveSpeed = 10f;
            velXZ = (targetWP.transform.position - pos).normalized * moveSpeed;

            runUpTimer.Start(Random.Range(2f, 8f), () => {
                velXZ = Vector3.zero;

                if (Random.value < 0.5f)
                    SetRule("Aim");
                else
                    SetRule("Idle");
            }, false);

            //If we've arrived at the destination before the timer runs out, find a new target to run at
            if (Vector3.Distance(pos, targetWP.transform.position) <= 1) {
                targetWP = targetWP.getRandomNextWaypoint();
            }
        }

        Timer goBackToRunningTimer = new Timer();
        void Rule_Aim() {
            anim.Set(8);

            SetLookAt2D(rayman.pos, 180);

            goBackToRunningTimer.Start(Random.Range(1f, 5f), () => {
                goToNearestWaypoint();
                anim.Set(2);
                SetRule("RunAround");
            }, false);
        }

        void Rule_Idle() {
            anim.Set(0);

            goBackToRunningTimer.Start(Random.Range(4f, 8f), () => {
                goToNearestWaypoint();
                anim.Set(2);
                SetRule("RunAround");
            }, false);
        }
    }
}
