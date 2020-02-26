//================================
//  By: Shrooblord
//================================
using System.Collections.Generic;
using UnityEngine;
using Shrooblord.lib;

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Robo-Pirate Henchman
    /// </summary>
    public partial class JCP_FRH_sbire_gnak_I1 : PersoController {
        #region Setup
        //public override float activeRadius => 999f;

        //Set up our Mind with the general feeling of what we're doing here and what we're feeling like at the moment.
        public HenchmanMind mind = new HenchmanMind(
            HenchmanMind.WaypointState.SeekingGrid,
            HenchmanMind.AttackState.Idle,
            HenchmanMind.Goal.Sleep,
            HenchmanMind.Mood.Chill
        );

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
        WPConnection conn;

        //waypoint targeting logic
        private struct PotentialConnection {
            public WPConnection connection;
            public int score;

            public PotentialConnection(WPConnection connection_in, int score_in) {
                connection = connection_in;
                score = score_in;
            }
        }

        //waypoint history
        int MaxRememberedConnections = 8;
        List<WPConnection> ConnectionHistory = new List<WPConnection>();   //keeps track of the last MaxRememberedConnections paths visited and attempts to avoid going back to those soon

        //SFX
        protected SFXPlayer snoreSFXPlayer;
        protected SFXPlayer landClankSFXPlayer;
        protected SFXPlayer idleSFXPlayer;

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

        protected override void OnInput() {
            if (Input.GetKeyDown(KeyCode.O)) SetRule("WokeUp");
        }


        public override bool interpolate => true;
        public override float activeRadius => 200;

        protected void ResetAll() {
            WPCurrent = null;
            WPTarget = null;
            conn = null;

            ConnectionHistory = new List<WPConnection>(); ;
            mind.DecisionQueue = new List<string>();

            DecisionTimeoutTimer.Abort();
            snoringTimer.Abort();
            wakeUpTimer.Abort();
            surpriseTimer.Abort();
            StuckRunning.Abort();
            PrepareJumpTimer.Abort();
            LandTimeout.Abort();
            LandedTimer.Abort();
            DrillSubmergeTimer.Abort();
            DrillEmergeTimer.Abort();
            DrillWindDownTimer.Abort();
        }

        protected override void OnStart() {
            ResetAll();

            for (int i = 0; i < MaxRememberedConnections; i++) {
                    ConnectionHistory.Add(null);
            }
            
            SetShadow(true);
            shadow.size = 2.5f;
            SetFriction(0, 0);

            //**** DELET THIS ****

            //Main.SetMainActor(this);
            Main.showMainActorDebug = true;

            //**** END OF DELET ****

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
        public static class Anim {
            public const int
                StateName1 = 0,
                StateName2 = 1; //...etc
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

        protected override void OnDebug() {
            DebugLabel("GOAL: " + mind.goal);
            DebugLabel("MOOD: " + mind.mood);
            DebugLabel("ATK: " + mind.atkState);
            DebugLabel("");

            DebugLabel("");
            DebugLabel("Decision Queue");
            for (int i = 0; i < mind.DecisionQueueMaximum; i++) {
                DebugLabel("[" + i.ToString() + "]",
                    (mind.DecisionQueue != null) ?
                        (mind.DecisionQueue.Count > i ? mind.DecisionQueue[i] : "NULL")
                    : "NULL");
            }

            DebugNewColumn();
            DebugNewColumn();
            DebugNewColumn();
            //DebugNewColumn();

            DebugLabel("WP State: " + mind.wpState);
            DebugLabel("WP Current: " + ((WPCurrent != null) ? WPCurrent.name : "NULL"));
            DebugLabel("WP Target: " + ((WPTarget != null) ? WPTarget.name : "NULL"));
            DebugLabel("HDL: " + ((conn != null) ? ((conn.pathHandle != null) ? conn.pathHandle.name : "NULL") : "NULL"));

            DebugLabel("");
            DebugLabel("Waypoint History");
            for (int i = 0; i < MaxRememberedConnections; i++) {
                DebugLabel("[" + i.ToString() + "]",
                    (ConnectionHistory != null) ?
                        ((ConnectionHistory.Count >= i && ConnectionHistory[i] != null) ?
                            (ConnectionHistory[i].pathHandle != null ? ConnectionHistory[i].pathHandle.name
                            : "NULL")
                        : "NULL")
                    : "NULL");
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


            #if UNITY_EDITOR
            //draw the LastWaypoints as numbers floating above those Waypoints
            if (ConnectionHistory != null) {
                for (var i = 0; i < ConnectionHistory.Count; i++) {
                    WPConnection connHist = ConnectionHistory[i];

                    if (connHist?.wp != null) {
                        ObjDrawText.Draw(connHist.pathHandle.transform, "[" + i.ToString() + "]", 500f, (GetComponent<PersoBehaviour>().poListIndex == 2) ? Color.red * 0.5f : SHR_Colours.purple);
                    }
                }
            }
            #endif
        }
        #endregion

        private void OnDrawGizmos() {
            DrawMind();
        }

        #region Core
        void Ground() {
            col.groundDepth = groundDepth;
            col.UpdateGroundCollision();

            slideJump = false;
            selfJump = false;

            if (col.ground.AnyGround && col.ground.hit.distance < 1.5f) {
                col.StickToGround();
            } else if (col.ground.Slide) {
                SetRule("Sliding"); return;
            } else if (col.water.Water && !col.waterIsShallow) {
                SetRule("Swimming"); return;
            } else {
                SetRule("Air"); return;
            }

            //SetFriction(30, 0);

            if (strafing) moveSpeed = 7;
            else moveSpeed = 10;
        }
        #endregion
        #region Mind
        public void EnqueueDecision(string rule) {
            if (mind.DecisionQueue != null) {
                if (mind.DecisionQueue.Count == mind.DecisionQueueMaximum) {
                    mind.DecisionQueue.RemoveAt(0);    //delete first item in the list and shove the rest up, or in other words, delete the "oldest" decision
                }
            }

            mind.DecisionQueue.Add(rule); //shove the made decision onto the end of the list
        }

        //defaults to the oldest decision (top of the list)
        public void DeleteDecision(int index = 0) {
            if ((mind.DecisionQueue != null) && (mind.DecisionQueue.Count >= index + 1)) mind.DecisionQueue.RemoveAt(index);
        }

        public string GetNextDecision() => ((mind.DecisionQueue != null) && (mind.DecisionQueue.Count > 0)) ? mind.DecisionQueue[0] : null;

        #endregion
        #region Waypoints
        new void GetNearestWaypoint() {
            WPCurrent = WPTarget;
            if (graph != null)
                WPTarget = graph.GetNearestWaypoint(pos);
        }

        WPConnection GetConnection() {
            if (WPCurrent != null) {
                foreach (var c in WPCurrent.next) {
                    if (c.wp == WPTarget) {
                        return c;
                    }
                }
            }

            return null;
        }

        void TrackLastWaypoints() {
            if (ConnectionHistory != null) {
                if (ConnectionHistory.Count == MaxRememberedConnections) {
                    ConnectionHistory.RemoveAt(0);    //delete first item in the list and shove the rest up, or in other words, delete the "oldest" Waypoint
                }
            }

            ConnectionHistory.Add(conn); //shove the current Waypoint onto the end of the list
        }

        void GetNextTargetWaypoint() {
            TrackLastWaypoints();
            WPCurrent = WPTarget;

            if (WPCurrent != null) {
                //let's build a small collection of potential connections we could form, assign them scores based on how attractive they are, and choose one from
                //  the most attractive ones at random
                int attempt = 10;
                List<PotentialConnection> PCs = new List<PotentialConnection>();
                List<PotentialConnection> valid = new List<PotentialConnection>();

                while (attempt > 0) {
                    bool useful = true;
                    //Create new connection with max score
                    PotentialConnection PC = new PotentialConnection(WPCurrent.GetRandomNextConnection(), 1000);
                    foreach (var pc in PCs) {
                        if (pc.connection == PC.connection) {
                            useful = false;  //don't add the same connection to our list twice
                            break;
                        }
                    }

                    if (useful) {
                        foreach (var c in ConnectionHistory) {
                            if (c != null) {

                                if (c == PC.connection)
                                    PC.score -= 400;                             //heavy score penalty for exactly the same path we just came from

                                //look through our own prev list
                                foreach (var p in WPCurrent.prev) {

                                    //ask whether one of the Waypoints here owns the Waypoint described by the Connection History
                                    if (p.wp.ConnectionInNext(c)) {

                                        //Are you the one we're trying to path towards?
                                        if (p.wp == PC.connection.wp) {
                                            //then subtract score
                                            PC.score -= 200; //we don't super like going back to a Waypoint we've visited in general,
                                        }                    //   but it's not as bad as using the exact same path

                                        break;  //If no, that's gucci; and also we can stop asking around, because there's no way someone else owns the same connection.
                                    }           //  Either way, we're breaking out of here
                                }
                            }
                        }

                        PCs.Add(PC);
                    }

                    attempt--;
                }

                //sort the list by highest scorers (highest at the end)
                PCs.Sort((PC1, PC2) => PC1.score.CompareTo(PC2.score));

                int threshold = 100;
                int highScore = PCs[PCs.Count - 1].score;

                valid.Add(PCs[PCs.Count - 1]);

                //starting at one before the end (which has already been added to valid), search through each PC and only add entries to valid that
                //  are within threshold of the highest scores
                for (var i = PCs.Count - 2; i >= 0; i--) {
                    if (PCs[i].score + threshold >= highScore)
                        valid.Add(PCs[i]);
                }

                //finally, go through all these valid candidates, and select one at random!
                conn = valid[Random.Range(0, valid.Count)].connection;
                WPTarget = conn.wp;
            }
        }
        #endregion
        #region Movement
        void SetVelXZ() {
            Vector3 dir = (WPTarget.transform.position - pos).normalized;
            velXZ = dir * moveSpeed;
        }

        public void Jump() {
            velY = vy_0;
            //jumpCutoff = vy_0 * 0.01f;
            jumping = true;
            SetRule("Air");
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
        #endregion

        #region Rules
        //***  Rulzez  ***//
        #region Core

        //Main logic loop interfaces with the Mind, which uses flags set by Waypoints, the current situation, and our current goals to make complex decision queues
        Timer DecisionTimeoutTimer = new Timer();
        protected void Rule_Decide() {
            if (newRule) {
                anim.Set(0); //always default to idle animation as the base state. other Rules will change this immediately in the same frame, so no need to worry about flickering

                //Do we still have an active Decision Queue?
                if (mind.DecisionQueue != null && mind.DecisionQueue.Count > 0) {
                    SetRule("DecisionMade");
                    return;
                }

                //in lieu of reaching any sort of meaningful conclusion to our pondering (see below), we just go with whatever's on our mind at the time
                DecisionTimeoutTimer.Start(3.14f, () => SetRule("WeighDecisions"), false);


                //Let's think about what we want to do...
                SetRule("Ponder");
            }
        }

        //Hmm... I wonder what to do next...
        //Decision-making process that takes time over multiple frames to compute and/or wait to see what Rayman does in order to react to him
        //  set flags accordingly, and then, in Rule_WeighDecision, we will check those flags and reach a conclusion from there
        //  ---> SetRule("WeighDecisions");
        protected void Rule_Ponder() {
            if (newRule)
                conn = GetConnection();

            //We are currently off-grid but would like to get back to it; relocate to the nearest Waypoint.
            if (conn == null) {
                if (mind.wpState == HenchmanMind.WaypointState.SeekingGrid) {
                    GetNextTargetWaypoint(); //also updates the Waypoint history

                    if (WPTarget == null) {
                        GetNearestWaypoint();   //find a target to run to
                    } else if (mind.goal != HenchmanMind.Goal.FindThePath) {
                        mind.goal = HenchmanMind.Goal.FindThePath;
                        mind.newGoal = true;

                        //no need to calculate any further or ponder other things; let's just go
                        SetRule("WeighDecisions");
                    }
                }
                return;
            }

            //We have a connection; are we on the grid, or still looking to join it?
            if (mind.wpState == HenchmanMind.WaypointState.SeekingGrid) {
                if (mind.goal != HenchmanMind.Goal.Patrol) {
                    mind.goal = HenchmanMind.Goal.Patrol;
                    mind.newGoal = true;

                    //no need to calculate any further or ponder other things; let's just go
                    SetRule("WeighDecisions");
                    return;
                }
            }

            //Just chillin' on the beaten path
            if ((mind.goal == HenchmanMind.Goal.Patrol) && (mind.wpState != HenchmanMind.WaypointState.NONE) &&
                (mind.wpState != HenchmanMind.WaypointState.OffGrid) && (mind.wpState != HenchmanMind.WaypointState.SeekingGrid)) {
                SetRule("WeighDecisions");
                return;
            }
        }

        //Given previous Ponder, we now have a bunch of flags to check in order to set up our next course of action. When this logic is completed,
        //  we will have arrived at our plan for the next couple of seconds and/or next couple of "steps" in our behaviour tree.
        //So call Rule_DecisionMade.
        //Naturally, these next steps can always be aborted at any time given a change in circumstances
        //  (Rayman moved, Rayman attacked us, we're low health, we walked somewhere specific, etc.).
        //This would trigger a new cascade of decision-making, essentially starting the whole process anew.
        protected void Rule_WeighDecisions() {
            DecisionTimeoutTimer.Abort();       //We're already here; no need to force us here again

            //We have a new plan! Let's see...
            if (mind.newGoal) {
                switch (mind.goal) {
                    case HenchmanMind.Goal.FindThePath:
                        //If we are here, that means we are currently off-grid and looking for a way back in
                        EnqueueDecision("RunAround"); //run to the target (and, once there, we'll have a new connection to examine)
                        break;

                    case HenchmanMind.Goal.Patrol:
                        EnqueueDecision("UseWaypointPath"); //start walking on the given path
                        EnqueueDecision("UseWaypointPath"); //let's just queue a couple to see what happens
                        EnqueueDecision("UseWaypointPath");
                        break;

                    default:
                        //do nothing for now if we don't have explicit instructions
                        return;
                }

                //---------//

                //Go off-grid and approach Ray
                //...

                //Attack Ray (Shoot, Hook, Barrel)
                //...

                //Idle
                //...

                //--------//

                mind.newGoal = false;
            } else {
                //Stay the course and steady as she goes...
                switch (mind.goal) {
                    case HenchmanMind.Goal.Patrol:
                        EnqueueDecision("UseWaypointPath"); //start walking on the given path
                        EnqueueDecision("UseWaypointPath"); //let's just queue a couple to see what happens
                        EnqueueDecision("UseWaypointPath");
                        break;
                    default:
                        //do nothing for now if we don't have explicit instructions
                        return;
                }
            }

            SetRule("DecisionMade");
        }

        protected void Rule_DecisionMade() {
            string decision = GetNextDecision();

            if (decision != null) {
                DeleteDecision(); //remove this decision from the queue
                SetRule(decision);
            } else {
                Debug.LogError(perso.name + ": Couldn't resolve Decisions; decision NULL" + " at position " + transform.position.ToString() + "!");
                SetRule("Decide");
            }
        }

        //Actively pathing on the Waypoint Graph
        protected void Rule_UseWaypointPath() {
            //WPConnection conn = GetConnection();

            if (newRule) GetNextTargetWaypoint();

            //Keep moving
            switch (conn.type) {
                case WPConnection.Type.Jump:
                    mind.wpState = HenchmanMind.WaypointState.Jumping;
                    SetRule("JumpAround");
                    break;

                case WPConnection.Type.Drill:
                    mind.wpState = HenchmanMind.WaypointState.Drilling;
                    SetRule("PrepareDrill");
                    break;

                default:
                    mind.wpState = HenchmanMind.WaypointState.Walking;
                    SetRule("RunAround");
                    break;
            }
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
                SetRule("Sliding");
                return;
            } else if (col.ground.Water && velY < 0 && !col.waterIsShallow) {
                SetRule("Swimming");
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
                LookAt2D(rayman.pos, 180);

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
                SetRule("Falling");
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

            snoringTimer.Start(3f, () => snoreSFXPlayer.Play(), false);

            //While sleeping, his """vision""" radius is greatly reduced, but he does have 360 degrees """field of view"""
            //  Then we can still use the same logic for detecting where Rayman is, but have it seem like the Pirate only noticed him because he "heard" Ray come close
            //...

            if (rayman != null) {
                if (Vector3.Distance(pos, rayman.pos) < 6) {  //6
                    SetRule("WokeUp");
                }
            }
        }

        Timer wakeUpTimer = new Timer();
        void Rule_WokeUp() {
            if (newRule) {
                snoringTimer.Abort();
                anim.Set(49);

                //timer for 1s
                wakeUpTimer.Start(1f, () => {
                    SetRule("Surprise");
                });
            }
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
                LookAt2D(WPTarget.transform.position, 180);
            SetVelXZ();
            anim.Set(2);

            if (Vector3.Distance(pos, WPTarget.transform.position) <= 0.5f) {
                velXZ = Vector3.zero;
                StuckRunning.Abort();

                SetRule("Decide");
                return;
            }

            if (newRule)
                StuckRunning.Start(8f, () => {
                    Debug.LogError(perso.name + ": Got stuck trying to reach Waypoint " + WPTarget + " from Waypoint " + WPCurrent + " at position " + transform.position.ToString() + "!");
                    SetRule("Decide");
                });
        }
        #endregion

        #region Jumping
        //Jumping between Waypoints
        void Rule_JumpAround() {
            //WPConnection conn = GetConnection();

            //move and look at where we're headed
            if (!lookAtRay)
                LookAt2D(WPTarget.transform.position, 180);

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
                    LookAt2D(rayman.pos, 180);

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
                LookAt2D(rayman.pos, 180);
                anim.Set(39); //submerging --> 27
                DrillSubmergeTimer.Start(1.82f, () => SetRule("DrillTravelling"));
            }
        }

        Timer DrillEmergeTimer = new Timer();
        void Rule_DrillTravelling() {
            if (newRule) {
                SetShadow(false);
                anim.Set(27);   //go invisible while "drilling in the ground" --> 23
                DrillEmergeTimer.Start(conn.drillTime, () => SetRule("DrillEmerge"));  //drill down for the amount of s specified in the Waypoint connection
            }
        }

        Timer DrillWindDownTimer = new Timer();
        void Rule_DrillEmerge() {
            if (newRule) {
                SetShadow(true);
                pos = conn.wp.transform.position;
                LookAt2D(rayman.pos, 180);
                anim.Set(23); //--> 0 (idle)
                DrillWindDownTimer.Start(0.934f, () => SetRule("Decide"));
            }
        }

        #endregion
        #endregion
    }
}




