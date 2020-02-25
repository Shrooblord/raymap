using System.Collections.Generic;

namespace RaymapGame.Rayman2.Persos {
    //Pirate decision-making flags
    public class HenchmanMind {
        public enum WaypointState {
            Walking, Jumping, Drilling, Parachuting, MoonJumping, Teleporting,  //generic movement along Waypoint Path

            StandingStill,                                                      //We are still in the process of pathing along the Waypoints, but right now we're not in motion

            OffGrid,                                                            //We are currently not on the Waypoint Path, and don't care about it
            SeekingGrid,                                                        //We are currently not on the Waypoint Path, but looking to get back onto it

            NONE                                                                //Error: Waypoint Path null
        };
        public WaypointState wpState;
        public bool newWPState; //true for the first tick that our WPS has changed; triggers cascading functionality to abort previous processes and immediately switch, etc.

        public enum AttackState {
            SeekingTarget, Aiming,                                              //acquiring target                      

            Shooting, TossingKeg,                                               //attacking at range

            Hooking, HookSlamming,                                              //melee attacks

            Idle,                                                               //not currently interested in attacking

            NONE                                                                //Error: I think I should have an enemy, but I don't / I've lost track of how I was supposed to interact with him
        };
        public AttackState atkState;
        public bool newAtkState; //true for the first tick that our AS has changed; triggers cascading functionality to abort previous processes and immediately switch, etc.

        //Abstract direction to push the incoming decision-making processes towards. When this value changes,
        //  my behaviour should be noticeably different.
        public enum Goal {
            OnGuard,                                                            //Hey, did I just see something?
            Patrol,                                                             //On-duty and looking for trouble
            Follow,                                                             //da leada, leada, leada...
            FindThePath,                                                        //The true way to enlightenment...

            Exterminate,                                                        //Enemy sighted. Time to make some mashed potato.
            Capture,                                                            //Freedom is an illusion. Here, I'll show you.
            Terrify,                                                            //Break their wills!! RESISTANCE IS FUTILE

            Heal,                                                               //I'm hurt kinda bad
            CallForBackup,                                                      //Come on, lads; the party's outside!

            Flee,                                                               //I don't wanna live on this planet anymore
            Surrender,                                                          //Please, I can't take any more...

            Sleep,                                                              //I'd like to find a nice, chill place where I can rest in peace
            Haul,                                                               //This big-ass thing needs going places.
            Rodeo,                                                              //Giddy-up!
        };
        public Goal goal;
        public bool newGoal; //true for the first tick that our Goal has changed; triggers cascading functionality to abort previous processes and immediately switch, etc.

        //In combination with Goal and the current circumstances, Mood will help push certain decision-making more towards one or the other way
        //  For example, an Angry Henchman will be more likely to just go for the kill, while a Desperate one might surrender or flee.
        //  A Satisfied one with high health might be more reckless and willing to gloat,
        //    while a Focussed one is driven, calculating, determined, and iron-willed.
        public enum Mood { Chill, Satisfied, Focussed, Annoyed, Angry, Afraid, Desperate }
        public Mood mood;
        public bool newMood; //true for the first tick that our Mood has changed; triggers cascading functionality to abort previous processes and immediately switch, etc.

        public List<string> DecisionQueue = new List<string>();                 //List of current decision and follow-up plans
        public int DecisionQueueMaximum = 8;                                    //Maximum amount of steps I can think ahead in time to approach my goal

        public HenchmanMind(WaypointState wpState, AttackState atkState, Goal goal, Mood mood) {
            this.wpState = wpState;
            this.atkState = atkState;
            this.goal = goal;
            this.mood = mood;
        }

        public HenchmanMind(Goal goal, Mood mood) {
            this.wpState = WaypointState.NONE;
            this.atkState = AttackState.NONE;

            this.goal = goal;
            this.mood = mood;
        }

        public HenchmanMind() { }
    }
}
