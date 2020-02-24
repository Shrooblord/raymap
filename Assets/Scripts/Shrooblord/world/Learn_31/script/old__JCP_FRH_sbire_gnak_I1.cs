using UnityEngine;
using Shrooblord.lib;

namespace CustomGame.Rayman2.Persos {
    public class old__JCP_FRH_sbire_gnak_I1 : PersoController {
        #region Setup
        public bool jumping;
        public bool selfJump;
        public bool slideJump;
        private float jumpApex = 1;                //jump apex
        public float groundDepth = 0.5f;
        float jumpCutoff;
        float jumpLiftOffVelY;
        float liftOffVel;
        Vector3 velXZStored = Vector3.zero; //in certain circumstances, we want to pause our horizontal movement for a little moment before returning to it (eg. jump anticipation)

        public bool strafing;

        public SHR_WaypointGraph wpGraph;
        SHR_Waypoint currentWP;
        SHR_Waypoint targetWP;

        private SFXPlayer snoreSFXPlayer;
        private SFXPlayer landClankSFXPlayer;
        private SFXPlayer idleSFXPlayer;

        private bool lookAtRay = false;
        private bool noIdle = true;
        #endregion

        protected override void OnStart() {
            pos = new Vector3(-193.61f, 23.84f, 369.45f);
            rot = Quaternion.Euler(0, 0, 0);

            //Colour the Henchman. 1 = Red; 2 = Purple
            GetComponent<PersoBehaviour>().poListIndex = 2;

            SetRule("Sleeping");

            #region SFX
            //snoring
            snoreSFXPlayer = SFXPlayer.CreateOn(this, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Voice/Snoring",
                space = SFXPlayer.Space.Point,
                mode = SFXPlayer.Mode.Consecutive,
            });

            //landing clank
            landClankSFXPlayer = SFXPlayer.CreateOn(this, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Footstep/Land",
                space = SFXPlayer.Space.Point
            });

            //idle voice
            idleSFXPlayer = SFXPlayer.CreateOn(this, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Voice/Idle",
                space = SFXPlayer.Space.Point,
                mode = SFXPlayer.Mode.RandomNoRepeat
            });            
            #endregion
        }

        //animNotify sfx
        #region animSFX
        public override AnimSFX[] animSfx => new AnimSFX[] {
            //running animation footstep plants
            new AnimSFX(2, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Footstep/Walk",
                space = SFXPlayer.Space.Point,          //make the sound originate from specifically the Henchman
                volume = 0.60f,
            }, 1, 10),

            //wake up in surprise
            //surprise sound
            new AnimSFX(49, new SFXPlayer.Info {
                path = "Rayman2/Henchman/General/pimoteur",
                space = SFXPlayer.Space.Point,
            }, 1),
            //heavy foot plant sound
            new AnimSFX(49, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Footstep/Land/",
                space = SFXPlayer.Space.Point,
            }, 16),

            //swivel head in surprise
            new AnimSFX(6, new SFXPlayer.Info {
                path = "Rayman2/Henchman/General/surpris",
                space = SFXPlayer.Space.Point,
            }, 1),
        };
        #endregion


        void goToNearestWaypoint() {
            if (graph != null)
                currentWP = targetWP;
            targetWP = graph.GetNearestWaypoint(pos);
        }

        public void Jump(float height, bool forceMaxHeight, bool selfJump = false, bool slideJump = false) {
            this.selfJump = selfJump;
            this.slideJump = slideJump;
            jumping = true;
            //helic = false;
            //rule = StdRules.Air;

            float am = Mathf.Sqrt((1920f / 97) * height);
            jumpLiftOffVelY = slideJump ? apprVel.y / 2 : 0;
            jumpCutoff = am * 0.65f + jumpLiftOffVelY;
            velY = am * 1.25f + jumpLiftOffVelY;

            /*
            if (velXZ.magnitude < moveSpeed / 2 || !selfJump)
                anim.Set(Anim.Rayman.JumpIdleStart, 1);
            else
                anim.Set(Anim.Rayman.JumpRunStart, 1);
            */

            SetRule(StdRules.Air);
        }



        //***  Rulzez  ***//
        //Rule_Ground and Rule_Air hijacked from YLT_RaymanModelRules.cs
        void Ground() {
            col.groundDepth = groundDepth;
            col.UpdateGroundCollision();

            slideJump = false;
            selfJump = false;

            if (col.ground.AnyGround && col.ground.hit.distance < 1.5f) {
                col.StickToGround();
            } else if (col.ground.Slide) {
                SetRule(StdRules.Sliding); return;
            } else if (col.water.Water && !col.waterIsShallow) {
                SetRule(StdRules.Swimming); return;
            } else {
                SetRule(StdRules.Air); return;
            }

            //SetFriction(30, 0);

            if (strafing) moveSpeed = 7;
            else moveSpeed = 10;
        }

        void Rule_Air() {
            #region Rule
            col.groundDepth = 0;
            col.UpdateGroundCollision();
            animateJump();

            if (newRule)
                liftOffVel = velXZ.magnitude;

            if (col.ground.AnyGround && velY <= 0) {
                velY = 0;
                //SetRule(StdRules.Ground);  <---- now handled by animateJump()
                return;
            } else if (col.ground.Slide) {
                SetRule(StdRules.Sliding);
                return;
            } else if (col.ground.Water && velY < 0 && !col.waterIsShallow) {
                SetRule(StdRules.Swimming);
                return;
            }

            if (jumping) {
                gravity = -13;
                if (velY < jumpCutoff)
                    jumping = false;
            } else {
                gravity = -25;
            }

            ApplyGravity();

            if (lookAtRay)
                SetLookAt2D(rayman.pos, 180);

            #region helic
            /*
            if (helic) {
                if (superHelicAscend)
                    superHelicRev = Mathf.Lerp(superHelicRev, 38, dt * 45);
                else superHelicRev = Mathf.Lerp(superHelicRev, 0, dt * 1);

                //GetSFXLayer(Anim.YLT_RaymanModel.HelicIdle).player.asrc.pitch = 1 + superHelicRev / 300;

                SetFriction(10, hasSuperHelic ? 2.5f : 7);
                moveSpeed = 5;
                velY += dt * superHelicRev;
                velY = Mathf.Clamp(velY, hasSuperHelic ? -25 : -5, 5);
                selfJump = false;
            } else {
                if (slideJump)
                    SetFriction(0.1f, 0);
                else SetFriction(5, 0);
                moveSpeed = 10;
            }
            */
            #endregion

            //RotateToStick(6);
            //InputMovement();


            if (pos.y < startPos.y - 1100)
                SetRule(StdRules.Falling);
            #endregion
            #region Animation
            /*
            if (helic) {
                anim.Set(Anim.Rayman.HelicEnable, 1);
            } else if (liftOffVel < 5 || !selfJump) {
                if (velY > 5 + jumpLiftOffVelY) {
                    anim.Set(Anim.Rayman.JumpIdleLoop, 0);
                } else {
                    if (newRule)
                        anim.Set(Anim.Rayman.FallIdleLoop, 0);
                    else
                        anim.Set(Anim.Rayman.FallIdleStart, 1);
                }
            } else {
                if (velY > 5 + jumpLiftOffVelY) {
                    anim.Set(Anim.Rayman.JumpRunLoop, 0);
                } else {
                    if (newRule)
                        anim.Set(Anim.Rayman.FallRunLoop, 0);
                    else
                        anim.Set(Anim.Rayman.FallRunStart, 1);
                }
            }
            */
            #endregion
        }




        Timer snoringTimer = new Timer();
        void Rule_Sleeping() {
            anim.Set(48);

            snoringTimer.Start(3f, () => { snoreSFXPlayer.Play(); }, false);

            goToNearestWaypoint();

            if (rayman != null) {
                if (Vector3.Distance(pos, rayman.pos) < 60) {  //6
                    snoringTimer.Abort();
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
        Timer waitHereTimer = new Timer();
        void Rule_RunAround() {
            //stick to ground, set movespeed and other ground-based rules and functionality
            Ground();

            //move and look at where we're headed
            if (!lookAtRay)
                SetLookAt2D(targetWP.transform.position, 180);
            Vector3 dir = (targetWP.transform.position - pos).normalized;

            velXZ = dir * moveSpeed;

            //find out if target is a jump-to waypoint
            foreach (var conn in currentWP.next) {
                if (conn.wp == targetWP) {
                    if (conn.type == WPConnection.Type.Jump) {
                        var tr = conn.pathHandle;
                        
                    //DELETE ME (TEMPORARY UNTIL CALCULUS BELOW IMPLEMENTED)
                        
                        jumpApex = 7;
                        
                    //END-DELETE ME

                        #region Parabolic Movement Calculus
                        #region // * * * OUTLINE * * * //
                        //typical parabolic motion using velocities:
                        // h(t) = gt^2  +  v.y_start * t  +  h_start
                        // h(t) = -10^2 +  v.y_start * t  +  """0"""   <--- probably more like WPend.pos.y - WPstart.pos.y

                        //axis of symmetry for parabola:
                        //  x_max = -b / 2a
                        // where a = g; b = v.y_start; x_max = t_at_h_max
                        // t_at_h_max = t_final / 2   <--- we know the value of t_final, because we decide it beforehand, eg. 4s
                        // 2 = -(v.y_start) / -20
                        // 2 * 20 = v.y_start
                        // v.y_start = 40

                        //so when you try and solve for h_maximum using this technique:
                        //  h(t) = gt^2  +  v.y_start * t  +  h_start
                        //  h(2) = -10 * (2^2)  +  40 * 2  +  0
                        //  h(2) = -40 + 80 = 40
                        //
                        //However, ***we*** *decide* how high the jump is (as that is simply the height of the handle)
                        //  h_maximum = handle.pos.y
                        //Say that it's 40
                        //  h(t) = gt^2  +  v.y_start * t  +  h_start
                        //  40 = h(2) = -10 * (2^2)  +  v.y_start * 2 + 0
                        //  40 = -40 + v.y_start * 2
                        //  80 = v.y_start * 2
                        //  40 = v.y_start

                        //we could calculate in advance at what time a jump would land given a starting velocity:
                        //  h(t) = 0
                        //  gt^2  +  v.y_start * t  +  h_start = 0
                        // -10t^2 +  40 * t + 0 = 0
                        // --> apply x = (-b (+/-) sqrt(b^2 - 4ac)) / 2a
                        //         where x = t; a = -10; b = 40; c = 0
                        // t = (-40 (+/-) sqrt(40^2 - 0)) / 2*-10
                        // t = (-40 (+/-) 40) / -20
                        // --> -40+40 = 0 so one solution is   t = 0s (that's the starting condition, so makes sense)
                        // --> -40-40 / -20 = -80 / -20 = 4 so t = 4s (and this equals the 4s we set in the initial conditions (see above), so that makes perfect sense)

                        //however, we instead will know the *time* the pirate will land (determine this beforehand; i.e. say "this jump will take 4 seconds; period."
                        //  and from this *determine* what its initial velocity in both horizontal and vertical movement components must be
                        // velY = v.y_start  <--- done! see above
                        // velHor will be determined based on the horizontal distance to point h_maximum, and the time it'll take to get from h = 0 to h = h_maximum,
                        //   and on the horizontal distance from point h_maximum to the end (it's the same!), and the time it'll take to get from h = h_maximum to h = 0

                        //also keep in mind that gravity = -13 while jumping = true, and gravity = -25 in all other cases
                        //  use this information to calculate the "two halves" (before and after the apex of the jump)
                        //  of the problem separately, though using exactly the same technique
                        //this means the velXY needs to change in mid-air to trace the parabola we expect. so write a rule for that inside the "if (velY < jumpCutoff)" of Rule_Air

                        //we can then multiply this horizontal movement by the directional vector dir (given above) to get the velocity in World Space
                        // velXZ = dir * velHor

                        #endregion
                        #region // * * * IMPLEMENTATION * * * //







                        #endregion
                        #endregion

                        if (!jumping) {
                            runUpTimer.Abort();
                            waitHereTimer.Abort();
                            velXZStored = velXZ;
                            lookAtRay = true;

                            PrepareJump();
                        }
                    }
                }
            }

            //time-out function; we didn't get to a next waypoint in time, so let's stop running around and do something else
            runUpTimer.Start(Random.Range(2f, 8f), () => {
                //do something interesting...
                //...

                //or idle behaviour
                if (!noIdle) {
                    velXZ = Vector3.zero;
                    if (Random.value < 0.5f)
                        SetRule("Aim", false);
                    else
                        SetRule("Idle", false);
                }
            }, false);

            //If we've arrived at the destination before the timer runs out, find a new target to run at
            if (Vector3.Distance(pos, targetWP.transform.position) <= 2.5f) {
                //runUpTimer.Abort();

                //if the waypoint is defined as a "wait here for X seconds" waypoint, do that first. otherwise, just go to the next waypoint
                if (targetWP.waitHere > 0f) {
                    //idle, but "loop forever" i.e. don't transition to next state; we'll do that manually from within the timer (see below)
                    SetRule("Idle", true);

                    waitHereTimer.Start(targetWP.waitHere, () => {
                        SetRule("RunAround");
                        currentWP = targetWP;
                        targetWP = currentWP.GetRandomNextWaypoint();
                    }, false);
                } else {
                    currentWP = targetWP;
                    targetWP = currentWP.GetRandomNextWaypoint();
                }
            }
        }

        Timer goBackToRunningTimer = new Timer();
        void Rule_Aim(bool fromJump) {
            if (fromJump)
                anim.Set(7);    //aiming anim
            else
                anim.Set(8);    //transition from idle to aiming anim --> transitions to 7 automatically

            SetLookAt2D(rayman.pos, 180);

            goBackToRunningTimer.Start(Random.Range(1f, 3f), () => {
                goToNearestWaypoint();
                anim.Set(2);
                SetRule("RunAround");
            }, false);
        }

        Timer idleVoice = new Timer();
        void Rule_Idle(bool loopForever) {
            anim.Set(0);

            idleVoice.Start(Random.Range(2f, 4f), () => {
                idleSFXPlayer.Play();
            }, false);

            if (!loopForever) {
                goBackToRunningTimer.Start(Random.Range(1.5f, 3f), () => {
                    idleVoice.Abort();
                    goToNearestWaypoint();
                    anim.Set(2);
                    SetRule("RunAround");
                }, false);
            }
        }

        void PrepareJump() {
            Debug.LogError("preparing jump...");
            velXZ = Vector3.zero;

            if (anim.currAnim == 14) {
                if (perso.currentFrame == 11) {
                    anim.Set(10); //transition to airborne anim; transitions to 11 (airborne loop)
                }
            } else if (anim.currAnim == 10) {
                velXZ = velXZStored;

                if (lookAtRay)
                    SetLookAt2D(rayman.pos, 180);

                Jump(jumpApex, true);
            } else {
                if (!lookAtRay)
                    anim.Set(14); //anticipation
                else
                    anim.Set(10); //straight up ignore the anticipation animation if we're supposed to be "strafe-jumping" (recall the Fairy Glade Pirate?)
            }
        }

        //called from Rule_Air when reaching apex of jump
        void animateJump() {
                switch (anim.currAnim) {
                case 11:                //airborne loop
                    if (!jumping)
                        anim.Set(12);   //apex of jump; transitions to 13 (jump declination loop)
                    break;
                case 13:                //jump declination loop
                    if (velY == 0f) {
                        velXZ = Vector3.zero;
                        landClankSFXPlayer.Play();
                        lookAtRay = false;
                        Ground();
                        anim.Set(15);   //landing transition
                    }
                    break;
                case 15:
                    Ground();
                    if (perso.currentFrame == 12) {
                        //goToNearestWaypoint();
                        //anim.Set(2);
                        //SetRule("RunAround");
                        SetRule("Test");
                    }
                    break;
                default:                //do nothing by default; we must still be in an animation transition
                    break;
            }
        }

        Timer test = new Timer();
        void Rule_Test() {
            test.Start(0.3f, () => {
                SetRule("RunAround");
            }, false);
        }
    }
}
