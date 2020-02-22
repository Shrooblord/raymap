﻿using System.Collections.Generic;
using UnityEngine;

namespace CustomGame.Rayman2.Persos {
    public class JCP_FRH_sbire_gnak_I1 : PersoController {
        #region Setup
        public bool jumping;
        public bool selfJump;
        public bool slideJump;
        private float jumpApex = 1;         //jump apex
        private float s2;                   //distance between jump apex and landing point

        public float groundDepth = 0.5f;
        float jumpCutoff;
        float jumpLiftOffVelY;
        float liftOffVel;
        Vector3 velXZStored = Vector3.zero; //in certain circumstances, we want to pause our horizontal movement for a little moment before returning to it (eg. jump anticipation)

        public bool strafing;

        public SHR_WaypointGraph wpGraph;
        SHR_Waypoint WPCurrent;
        SHR_Waypoint WPTarget;

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

        #region Functions
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

        void GoToNearestWaypoint() {
            if (graph != null)
                WPCurrent = WPTarget;
            WPTarget = graph.GetNearestWaypoint(pos);
        }

        WPConnection GetConnection() {
            foreach (var conn in WPCurrent.next) {
                if (conn.wp == WPTarget) {
                    return conn;
                }
            }

            return null;
        }

        void SetVelXZ() {
            Vector3 dir = (WPTarget.transform.position - pos).normalized;
            velXZ = dir * moveSpeed;
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
        
        //First half of the jump parabola happens to be the exact same calculation as the second half
        private Vector3 JumpParabola(float h_max_or_y0, float s, Vector3 dir) {
            //1) Find t
            float t = Mathf.Sqrt( 2*(h_max_or_y0) / Mathf.Abs(gravity) );
            Debug.LogError("t: " + t);
            //2) Find and return v_xz
            return s * t * dir;
        }

        void animateJump() {
            switch (anim.currAnim) {
                case 11:                //airborne loop
                    if (!jumping)
                        anim.Set(12);   //apex of jump; transitions to 13 (jump declination loop)
                    break;
                default:                //do nothing by default; we must still be in an animation transition
                    break;
            }
        }
        #endregion

        #region Rules
        //***  Rulzez  ***//
        #region Core
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
                SetRule("Land");
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
                    velXZ = JumpParabola(pos.y, s2, transform.forward);  //- because all persos have "forwards" pointing behind them...
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


        #endregion

        #region Introduction: Sleeping
        Timer snoringTimer = new Timer();
        void Rule_Sleeping() {
            anim.Set(48);

            snoringTimer.Start(3f, () => { snoreSFXPlayer.Play(); }, false);

            GoToNearestWaypoint();

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
        #endregion

        //Running over the ground to next Waypoint
        void Rule_RunAround() {
            //stick to ground, set movespeed and other ground-based rules and functionality
            Ground();

            //move and look at where we're headed
            if (!lookAtRay)
                SetLookAt2D(WPTarget.transform.position, 180);
            SetVelXZ();
            anim.Set(2);

            if (Vector3.Distance(pos, WPTarget.transform.position) <= 0.5f) {                
                WPCurrent = WPTarget;
                WPTarget = WPCurrent.getRandomNextWaypoint();

                SetVelXZ();
                WPConnection conn = GetConnection();
                if (conn.type == WPConnection.Type.JumpTo) {
                    SetRule("JumpAround", conn);
                }
            }
        }

        //Jumping between Waypoints
        void Rule_JumpAround(WPConnection conn) {
            var tr = conn.jumpCurveHandle;

            //move and look at where we're headed
            if (!lookAtRay)
                SetLookAt2D(WPTarget.transform.position, 180);

            #region Parabolic Movement Calculus Part 1
            //Parabola with start C and end T and apex H
            Vector3 C = WPCurrent.transform.position;
            Vector3 H = conn.jumpCurveHandle.position;
            Vector3 T = WPTarget.transform.position;
            jumpApex = Mathf.Sqrt( Mathf.Pow(H.y, 2) - Mathf.Pow(C.y, 2) );
            Debug.LogError("apex: " + jumpApex);
            Vector3 dir = (WPTarget.transform.position - pos).normalized;
            Debug.LogError("dir: " + dir);

            float s1 = Vector2.Distance(new Vector2(C.x, C.z), new Vector2(H.x, H.z)); //distance ||Cxz - Hxz||
            s2 = Vector2.Distance(new Vector2(H.x, H.z), new Vector2(T.x, T.z));       //distance ||Hxz - Txz||

            Debug.LogError("s1: " + s1);

            velXZ = JumpParabola(jumpApex, s1, dir);
            Debug.LogError("velXZ: " + velXZ);
            //The rest of part 2 is inside Rule_Air (as that is when vy = 0 is called)
            #endregion

            if (!jumping) {
                velXZStored = velXZ;
                SetRule("PrepareJump");
            }
        }

        //Animation and transition into becoming airborne
        void Rule_PrepareJump() {
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

        void Rule_Land() {
            switch (anim.currAnim) {
                case 13:                //jump declination loop
                    if (velY == 0f) {
                        Ground();
                        velXZ = Vector3.zero;
                        landClankSFXPlayer.Play();

                        lookAtRay = false;
                        anim.Set(15);   //landing transition
                    }
                    break;
                case 15:
                    Ground();
                    if (perso.currentFrame == 12) {
                        SetRule("RunAround");
                    }
                    break;
                default:
                    break;
            }
        }
        #endregion
    }
}
