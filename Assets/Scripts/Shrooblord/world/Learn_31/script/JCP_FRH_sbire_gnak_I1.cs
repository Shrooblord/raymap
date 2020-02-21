using UnityEngine;

namespace CustomGame.Rayman2.Persos {
    public class JCP_FRH_sbire_gnak_I1 : PersoController {
        #region Setup
        public bool jumping;
        public bool selfJump;
        public bool slideJump;
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

        private string previousRule = "";   //for keeping track of which rule to switch back to after an interrupt (eg. jumping)
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

            //idle
            /*
            new AnimSFX(16, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Voice/Idle",
                space = SFXPlayer.Space.Point,
                mode = SFXPlayer.Mode.Random
            }, 1),
            */
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
            rule = StdRules.Air;

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
        }



        //***  Rulzez  ***//
        //Rule_Ground and Rule_Air hijacked from YLT_RaymanModelRules.cs
        void Rule_Ground() {
            #region Rule
            col.groundDepth = groundDepth;
            col.UpdateGroundCollision();

            //if (newRule && lStick.magnitude < deadZone)
            //    velXZ = Vector3.zero;

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

            //InputMovement();
            //RotateToStick(10);
            //rot = Quaternion.Slerp(rot, Quaternion.Euler(0, rot.eulerAngles.y, 0), dt * 10);
            #endregion
            #region Animation
            /*
            if (velXZ.magnitude < 0.05f) {
                t_runStart.Start(0.033f);
                if (newRule) {
                    if (helic)
                        anim.Set(Anim.Rayman.HelicLandIdle, 1);
                    else
                        anim.Set(Anim.Rayman.LandIdle, 1);
                } else {
                    anim.Set(Anim.Rayman.Idle, 0);
                }
                if (anim.currAnim == Anim.Rayman.RunStop)
                    anim.SetSpeed(40);
                else anim.SetSpeed(25);
            } else if (velXZ.magnitude < 5) {
                if (newRule) {
                    if (helic)
                        anim.Set(Anim.Rayman.HelicLandWalk, 1);
                    else
                        anim.Set(Anim.Rayman.LandWalk, 1);
                } else
                    anim.Set(Anim.Rayman.Walk, 0);
                float spd = velXZ.magnitude * moveSpeed * 1.5f;

                if (anim.currAnim == Anim.Rayman.HelicLandWalk)
                    anim.SetSpeed(spd / 2);
                else
                    anim.SetSpeed(spd);
            } else {
                if (newRule) {
                    if (helic)
                        anim.Set(Anim.Rayman.HelicLandWalk, 1);
                    else
                        anim.Set(Anim.Rayman.LandRun, 1);
                } else {
                    if (anim.currAnim == Anim.Rayman.RunStart)
                        anim.SetSpeed(200);
                    else if (anim.IsSet(Anim.Rayman.SlideToRun))
                        anim.SetSpeed(30);
                    else
                        anim.Set(Anim.Rayman.Run, 0, velXZ.magnitude * moveSpeed * 0.5f);
                }

                if (anim.currAnim == Anim.Rayman.HelicLandWalk || anim.currAnim == Anim.Rayman.LandRun)
                    anim.SetSpeed(60);
            }

            if ((anim.currAnim == Anim.Rayman.RunStop || velXZ.magnitude < 0.05f) && lStick.magnitude >= deadZone) {
                anim.Set(Anim.Rayman.RunStart, 1);
            } else if (velXZ.magnitude > 5 && lStick.magnitude < deadZone) {
                anim.Set(Anim.Rayman.RunStop, 1);
            }
            */
            #endregion

            //helic = false;
            
            SetRule("GroundReturnHook");
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
            //move and look at where we're headed
            SetLookAt2D(targetWP.transform.position, 180);
            moveSpeed = 10f;

            velXZ = (targetWP.transform.position - pos).normalized * moveSpeed;

            //find out if target is a jump-to waypoint
            foreach (var conn in currentWP.next) {
                if (conn.wp == targetWP) {
                    if (conn.type == WPConnection.Type.JumpTo) {
                        var tr = conn.jumpCurveHandle;
                        float h = tr.position.y - currentWP.transform.position.y;
                        //float h = 6;

                        if (!jumping) {
                            runUpTimer.Abort();
                            waitHereTimer.Abort();
                            previousRule = rule;
                            velXZStored = velXZ;
                            prepareJump(h);
                        }
                    }
                }
            }

            runUpTimer.Start(Random.Range(2f, 8f), () => {
                velXZ = Vector3.zero;

                if (Random.value < 0.5f)
                    SetRule("Aim");
                else
                    SetRule("Idle", false);
            }, false);

            //If we've arrived at the destination before the timer runs out, find a new target to run at
            if (Vector3.Distance(pos, targetWP.transform.position) <= 1.5f) {
                runUpTimer.Abort();

                //if the waypoint is defined as a "wait here for X seconds" waypoint, do that first. otherwise, just go to the next waypoint
                if (targetWP.waitHere > 0f) {
                    //idle, but "loop forever" i.e. don't transition to next state; we'll do that manually from within the timer (see below)
                    SetRule("Idle", true);

                    waitHereTimer.Start(targetWP.waitHere, () => {
                        SetRule("RunAround");
                        currentWP = targetWP;
                        targetWP = currentWP.getRandomNextWaypoint();
                    }, false);
                } else {
                    currentWP = targetWP;
                    targetWP = currentWP.getRandomNextWaypoint();
                }
            }
        }

        Timer goBackToRunningTimer = new Timer();
        void Rule_Aim() {
            anim.Set(8);

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

        void prepareJump(float h) {
            velXZ = Vector3.zero;

            if (anim.currAnim == 14) {
                if (perso.currentFrame == 11) {
                    anim.Set(10); //transition to airborne anim; transitions to 11 (airborne loop)
                }
            } else if (anim.currAnim == 10) {
                velXZ = velXZStored;
                //Debug.LogError("velXZ: " + velXZ.ToString());
                Jump(h, true);
            } else {
                anim.Set(14); //anticipation
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
                        anim.Set(15);   //landing transition
                    }
                    break;
                case 15:
                    if (perso.currentFrame == 12) {
                        SetRule(StdRules.Ground);
                    }
                    break;
                default:                //do nothing by default; we must still be in an animation transition
                    break;
            }
        }

        //called when Rule_Ground returns; use to switch state machine back to where it was when it got interrupted
        void Rule_GroundReturnHook() {
            if (previousRule != "") {
                //SetRule(previousRule);
                SetRule("Idle", false);
            }
        }
    }
}
