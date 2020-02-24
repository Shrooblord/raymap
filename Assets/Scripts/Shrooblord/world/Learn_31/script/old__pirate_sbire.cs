using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Shrooblord.lib;

namespace CustomGame.Rayman2.Persos {
    public class old__pirate_sbire : PersoController {
        #region Setup
        //jumping
        public bool jumping;
        public bool selfJump;
        public bool slideJump;
        //private float jumpCutoff;
        private float vy_0;                 //instantaneous vertical velocity (used for jumps)
        private Vector3 jumpDir = Vector3.zero;

        //ground-based Physics
        public float groundDepth = 0.5f;
        Vector3 velXZStored = Vector3.zero; //in certain circumstances, we want to pause our horizontal movement for a little moment before returning to it (eg. jump anticipation)

        public bool strafing;

        //senses
        public float visionRadius = 10f;
        public float visionAngle = 200f;

        //waypoints
        public SHR_WaypointGraph wpGraph;
        SHR_Waypoint WPCurrent;
        SHR_Waypoint WPTarget;

        //waypoint history
        int MaxRememberedConnections = 3;
        List<WPConnection> ConnectionHistory;   //keeps track of the last MaxRememberedConnections paths visited and attempts to avoid going back to those soon

        //SFX
        private SFXPlayer snoreSFXPlayer;
        private SFXPlayer landClankSFXPlayer;
        private SFXPlayer idleSFXPlayer;

        //general behaviour flags
        private bool lookAtRay = false;
        private bool noIdle = true;

        //debug
        private Vector3 debugTarget = Vector3.zero;
        private enum DebugType {
            FindWaypointTarget,         //Searching for next Waypoint
            WalkToWaypoint,             //Actively pathing to next Waypoint
            JumpToWaypoint,             //Performing a Jump to next Waypoint

            VisionCone,                 //Vision Cone sense -- within it, the Pirate will see Rayman
            SeeRayman,                  //Rayman was spotted! Tracking his movement...
            SearchingForRayman,         //I'm sure we saw him just a moment ago... where did he go? I guess I could give up after 5 more seconds of this...

            AimAtRayman,                //Enemy sighted! Lining up...
            ShootAtRayman,              //DIE, POTATO-NOSED SCUM!
            TossKegAtRayman,            //BOOM, BABY!!
            HookRayman,                 //Hayaaa!!

            Idle                        //Nuthin' much to do here. Let's just chill.
        };
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

        #region Animations
        /*
            IDLE
            0: idle
            21: 0 dupe
            34: 0 dupe
            58: 0 dupe
            66: 0 dupe

            16: look left
            60: 16 dupe
            19: hold looking left
            62: 19 dupe
            17: 16 --> 0
            64: 17 dupe

            18: look right
            67: 18 dupe
            22: hold look right
            69: 22 dupe
            20: 18 --> 0
            63: 20 dupe
            


            RUNNING
            2: running
            72: 2 dupe



            SLEEPING
            48: zzz
            49: WWHHHUHH WTF WHO'S THERE --> 0 (idle)
            61: 49 dupe
            74: 49 dupe



            SURPRISE
            6: surprise --> 2 (running)
            73: 6 dupe



            AIMING
            1: aim --> 0 (idle)
            4: 1 dupe
            70: 1 dupe
            71: 1 dupe
            85: 1 dupe

            5: take aim
            82: 5 dupe
            8: 5 but faster

            7: aim loop



            SHOOT:
            3: bang bang bang!!
            84: 3 but faster



            HOOK
            42: begin hook swing --> 43
            43: jiggle with hook --> 44
            44: hook swing wind-down

            78: plant hook in ground



            BARREL
            35: grab barrel --> 38
            38: juggle barrel --> 41 ***OR*** 40

            41: hold barrel (freeze frame) --> 37
            36: cock barrel --> 41
            40: lean back cocking barrel and transition into throw --> 37

            37: toss barrel
            
            HIT
            9: oof!

            45: big hit from idle --> transitions into 33 ***OR*** dies
            75: 45 dupe
            81: 45 dupe

            31: hit mid-air --> transitions back into 32 (identical to 13 (jump declination loop))
            56: big hit falling (freeze frame)
            76: 56 dupe
            33: crash-land on the ground after being hit in mid-air
            65: 33 dupe



            JUMPING
            14: anticipation --> 10
            80: 14 dupe
            10: transition to airborne loop --> 11

            11: airborne loop
            32: 11 dupe
            83: 11 dupe
            12: apex of jump --> 13

            13: jump declination loop
            77: 13 dupe
            15: landing transition --> 7 (aim loop)
            57: 15 dupe



            DRILLING
            39: submerging --> 27
            27: invisible state
            47: 27 dupe
            79: 27 dupe
            23: emerging --> 0 (idle)



            PARACHUTE
            25: parachuting down (loop)
            24: parachute fold in --> 26
            26: 24 --> starfish1 fall (but DIFFERENT from 53) --> 29
            29: starfish1 fall --> 30
            30: starfish1 landing --> 0 (idle)
            68: 30 dupe



            STARFISH2 JUMP
            52: crunched up in a ball --> starfish         // could use the first frame of this as a "jumping with salto" animation, paired with rotating him around his centre axis; or a Samus Power Ball? xD
            53: starfish2 fall
            55: starfish2 landing --> transitions into 7 (aim)



            MISCELLANEOUS
            28: hanging onto ledge with hook and aiming

            46: big landing

            50: hanging onto something
            51: hanging onto something and pulling hard

            54: landing from holding cannon arm straight up in air (? --> looks like it should have been a parachute but doesn't match animations)
            59: 54 dupe
        */
        #endregion

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

            //Drilling
            //Submerging
            new AnimSFX(39, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Weapon/Drill/ELEC6",
                space = SFXPlayer.Space.Point,
            }, 17),

            //Emerging
            //drill
            new AnimSFX(23, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Weapon/Drill/ELEC5",
                space = SFXPlayer.Space.Point,
            }, 1),

            //heavy foot plant sound
            new AnimSFX(23, new SFXPlayer.Info {
                path = "Rayman2/Henchman/Footstep/Land/",
                space = SFXPlayer.Space.Point,
            }, 21),
        };
        #endregion

        #region Functions

        #region DEBUG
        Color FindDebugDrawColour(DebugType type) {
            switch (type) {
                case DebugType.FindWaypointTarget:
                    return Color.blue;

                case DebugType.WalkToWaypoint:
                case DebugType.JumpToWaypoint:
                case DebugType.SeeRayman:
                    return Color.green;

                case DebugType.VisionCone:
                    return Color.magenta;

                case DebugType.AimAtRayman:
                    return Color.cyan;

                case DebugType.ShootAtRayman:
                case DebugType.TossKegAtRayman:
                    return Color.red;

                case DebugType.HookRayman:
                    return Color.black;

                default:
                    return Color.white;
            }
        }

        void DrawVisionCone(float angle, float radius) {
            Color col = FindDebugDrawColour(DebugType.VisionCone);

            Vector3 left = pos + radius * angle / 2 * transform.forward;
            Vector3 right = pos + radius * -angle / 2 * transform.forward;

            Debug.DrawLine(pos, left, col);
            Debug.DrawLine(pos, right, col);
            Debug.DrawLine(left, right, col);
        }

        //All the things the Henchman is thinking right now
        void DrawMind() {
            //DrawVisionCone(visionAngle, visionRadius);

            //need to keep track of what my "target(s)" is/are at any time, i.e. is it a Waypoint, or Rayman, or both? ---> prob a List of Vec3s then
            //draw line to target and a ball on the target(, and offset these in Y so they're not in the ground(?))
            //...



            //draw the LastWaypoints as numbers floating above those Waypoints
            if (ConnectionHistory != null) {
                for (var i = 0; i < ConnectionHistory.Count; i++) {
                    WPConnection conn = ConnectionHistory[i];

                    if (conn != null && conn.wp != null && conn.wp != null) {
                        Color col = Color.Lerp(Color.magenta, Color.cyan / 1.8f, ConnectionHistory.Count / (i + 1)); //fade from bright magenta to faded cyan based on which index in the history the node is
                        ObjDrawText.Draw(conn.pathHandle.transform, "[" + i.ToString() + "]", 500f, col);
                    }
                }
            }
        }
        #endregion

        //every tick
        protected override void OnUpdate() {
        }

        private void OnDrawGizmos() {
            DrawMind();
        }

        //Core
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

        //Waypoints
        void GetNearestWaypoint() {
            WPCurrent = WPTarget;
            if (graph != null)
                WPTarget = graph.GetNearestWaypoint(pos);
        }

        WPConnection GetConnection() {
            if (WPCurrent != null) {
                foreach (var conn in WPCurrent.next) {
                    if (conn.wp == WPTarget) {
                        return conn;
                    }
                }
            }

            return null;
        }

        void TrackLastWaypoints() {
            if (ConnectionHistory.Count == MaxRememberedConnections) {
                ConnectionHistory.RemoveAt(1);    //delete first item in the list and shove the rest up, or in other words, delete the "oldest" Waypoint
            }
            ConnectionHistory.Add(GetConnection()); //shove the current Waypoint onto the end of the list
        }

        void GetNextTargetWaypoint() {
            TrackLastWaypoints();
            WPCurrent = WPTarget;

            if (WPCurrent != null) {
                int attempt = 10;
                bool success = false;
                while ((attempt > 0) && (success == false)) {
                    WPTarget = WPCurrent.GetRandomNextWaypoint();

                    success = !ConnectionHistory.Contains(GetConnection());     //did we successfully find a unique Connection to travel through?

                    attempt--;
                }

                if ((attempt == 10) && (!success)) {
                    Debug.LogError(perso.name + ": Couldn't find new unique Connection to visit considering recent History. Ignoring request for unique Connection at position " + transform.position.ToString() + ".");
                }
            }
        }

        //Movement
        void SetVelXZ() {
            Vector3 dir = (WPTarget.transform.position - pos).normalized;
            velXZ = dir * moveSpeed;
        }

        public void Jump() {
            velY = vy_0;
            //jumpCutoff = vy_0 * 0.01f;
            jumping = true;
            SetRule(StdRules.Air);
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

        //Main logic loop. Most of these functions are determined by flags listed on the Waypoints themselves
        protected void Rule_Decide() {
            if (newRule) {
                anim.Set(0);    //always default to idle animation as the base state. other Rules will change this immediately in the same frame, so no need to worry about flickering
                GetNextTargetWaypoint(); //also updates the Waypoint history
            }

            WPConnection conn = GetConnection();

            //We are currently off-grid; relocate to the nearest Waypoint.
            if (conn == null) {
                if (WPTarget == null) {
                    GetNearestWaypoint();   //find a target to run to
                } else {
                    SetRule("RunAround");   //run to the target (and, once there, we'll have a new connection to examine)
                }
                return;
            }

            //Keep moving
            //if (I want to keep moving) {
            switch (conn.type) {
                case WPConnection.Type.Jump:
                    SetRule("JumpAround");
                    break;

                case WPConnection.Type.Drill:
                    SetRule("PrepareDrill");
                    break;

                default:
                    SetRule("RunAround");
                    break;
            }
            //}


            //Go off-grid and approach Ray


            //Attack Ray (Shoot, Hook, Barrel)


            //Idle
        }

        protected void Rule_Air() {
            #region Rule
            col.groundDepth = 0;
            col.UpdateGroundCollision();
            animateJump();

            if (col.ground.AnyGround && velY <= 0) {//<= jumpCutoff) {
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
                if (velY <= 0) {
                    jumping = false;
                }
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
        protected void Rule_Sleeping() {
            anim.Set(48);

            snoringTimer.Start(3f, () => { snoreSFXPlayer.Play(); }, false);

            //While sleeping, his """vision""" radius is greatly reduced, but he does have 360 degrees """field of view"""
            //  Then we can still use the same logic for detecting where Rayman is, but have it seem like the Pirate only noticed him because he "heard" Ray come close
            //...

            if (rayman != null) {
                if (Vector3.Distance(pos, rayman.pos) < 6) {  //6
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
            surpriseTimer.Start(0.9f, () => { SetRule("Decide"); }, false);
        }
        #endregion

        #region Running
        Timer StuckRunning = new Timer();
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
                velXZ = Vector3.zero;
                StuckRunning.Abort();

                SetRule("Decide");
                return;
            }

            StuckRunning.Start(8f, () => {
                Debug.LogError(perso.name + ": Got stuck trying to reach Waypoint " + WPTarget + " from Waypoint " + WPCurrent + " at position " + transform.position.ToString() + "!");
                SetRule("Decide");
            }, false);
        }
        #endregion

        #region Jumping
        //Jumping between Waypoints
        void Rule_JumpAround() {
            WPConnection conn = GetConnection();

            //move and look at where we're headed
            if (!lookAtRay)
                SetLookAt2D(WPTarget.transform.position, 180);

            #region Paraboloid Movement Calculus
            //Paraboloid with start C and end T and apex H
            Vector3 C = WPCurrent.transform.position;
            Vector3 H = conn.pathHandle.transform.position;
            Vector3 T = WPTarget.transform.position;

            float apex = Mathf.Sqrt(Mathf.Pow(H.y - C.y, 2));

            //find t; we need to split the curve in half because of the differences in gravity up and down, and because the points might start / end on different heights
            //first half of the curve
            float t1 = Mathf.Sqrt(2 * apex / 13);         //g = 13 while jumping

            //second half of the curve
            float t2 = Mathf.Sqrt(2 * Mathf.Sqrt(Mathf.Pow(T.y - H.y, 2)) / 25);         //g = 25 while falling
            float t = (t1 + t2);

            //2) find and set instantaneous vertical velocity vy_0
            // dy = vy_0 * t - 0.5g(t^2)
            // dy + 0.5g(t^2) = vy_0 * t
            // (dy + 0.5g(t^2)) / t = vy_0
            vy_0 = (apex + (0.5f * 13 * Mathf.Pow(t1, 2))) / t1;

            //3) find and return instantaneous horizontal velocity vxz_0
            // v = s / dt
            Vector3 jumpDir = (WPTarget.transform.position - pos).normalized;
            float jumpDist = Vector3.Distance(C, T);
            velXZ = jumpDir * (jumpDist / t);
            #endregion

            if (!jumping) {
                velXZStored = velXZ;
                SetRule("PrepareJump");
            }
        }

        //Animation and transition into becoming airborne
        Timer PrepareJumpTimer = new Timer();
        void Rule_PrepareJump() {
            velXZ = Vector3.zero;
            if (anim.currAnim == 14) {  //12 frames in 30fps; skip one
                PrepareJumpTimer.Start(11f / 30f, () => anim.Set(10), false); //transition to airborne anim; transitions to 11 (airborne loop)

            } else if (anim.currAnim == 10) {
                velXZ = velXZStored;

                if (lookAtRay)
                    SetLookAt2D(rayman.pos, 180);

                Jump();
            } else {
                if (!lookAtRay)
                    anim.Set(14); //anticipation
                else
                    anim.Set(10); //straight up ignore the anticipation animation if we're supposed to be "strafe-jumping" (recall the Fairy Glade Pirate?)
            }
        }

        //Landing from a jump
        Timer LandTimeout = new Timer();
        Timer LandedTimer = new Timer();
        void Rule_Land() {
            //timeout catch in case something goes wrong; transition to next state
            LandTimeout.Start(2, () => {
                SetRule("Decide");
            }, false);

            switch (anim.currAnim) {
                case 13:                //jump declination loop
                    if (velY <= 0f) {
                        Ground();
                        velXZ = Vector3.zero;
                        landClankSFXPlayer.Play();

                        lookAtRay = false;
                        anim.Set(15);   //landing transition
                    }
                    break;
                case 15:    //13 frames in 30 fps; animation is 13/30 s long. However, we can only ever get here starting from frame 1, as we break out of the switch when we set the animation to this one
                    Ground();

                    LandedTimer.Start(12f / 30f, () => {
                        LandTimeout.Abort();
                        SetRule("Decide");
                    }, false);

                    break;
                default:
                    break;
            }
        }
        #endregion

        #region Drilling
        Timer DrillSubmergeTimer = new Timer();
        void Rule_PrepareDrill() {
            if (newRule) {
                SetLookAt2D(rayman.pos, 180);
                anim.Set(39); //submerging --> 27
                DrillSubmergeTimer.Start(1.82f, () => SetRule("DrillTravelling"), false);
            }
        }

        Timer DrillEmergeTimer = new Timer();
        void Rule_DrillTravelling() {
            if (newRule) {
                anim.Set(27);   //go invisible while "drilling in the ground" --> 23
                DrillEmergeTimer.Start(GetConnection().drillTime, () => SetRule("DrillEmerge"), false);  //drill down for the amount of s specified in the Waypoint connection
            }
        }

        Timer DrillWindDownTimer = new Timer();
        void Rule_DrillEmerge() {
            if (newRule) {
                pos = GetConnection().wp.transform.position;
                SetLookAt2D(rayman.pos, 180);
                anim.Set(23); //--> 0 (idle)
                DrillWindDownTimer.Start(0.934f, () => SetRule("Decide"), false);
            }
        }

        #endregion
        #endregion
    }
}
