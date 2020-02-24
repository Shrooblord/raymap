//================================
//  By: Adsolution
//================================

using UnityEngine;
using static RaymapGame.InputEx;
using static UnityEngine.Input;

namespace RaymapGame.Rayman2.Persos {
    public partial class YLT_RaymanModel : PersoController {
        public StdCam cam;

        public bool jumping;
        public bool selfJump;
        public bool slideJump;
        public float groundDepth = 0.5f;
        float jumpCutoff;
        float jumpLiftOffVelY;
        float liftOffVel;

        public bool strafing;
        public bool helic;
        public bool hasSuperHelic;
        bool superHelicAscend;
        float superHelicRev;

        public bool onRespawn;
        public override bool resetOnRayDeath => false;

        CollideInfo colClimb;

        Timer t_runStop = new Timer();
        Timer t_runStart = new Timer();
        Timer t_respawn = new Timer();

        protected override void OnDebug() {
            DebugLabel("Helic Active", helic);
            DebugLabel("Has Super Helic", hasSuperHelic);
            DebugLabel("Cam rule", cam.rule);
        }

        public override float activeRadius => 100000;

        public override AnimSFX[] animSfx => new AnimSFX[]
        {
            new AnimSFX(Anim.Rayman.Run, "Rayman2/Rayman/footsteps/grass", 11, 22),

            new AnimSFX(Anim.Rayman.LandRun, "Rayman2/Rayman/roll", SFXPlayer.Polyphony.Mono),
            new AnimSFX(Anim.Rayman.LandRun, "Rayman2/Rayman/footsteps/grass", 40, 51),

            new AnimSFX(Anim.Rayman.JumpIdleStart, new SFXPlayer.Info { path = "Rayman2/Rayman/jump", volume = 0.75f }),
            new AnimSFX(Anim.Rayman.JumpRunStart, new SFXPlayer.Info { path = "Rayman2/Rayman/jump", volume = 0.75f }),
            new AnimSFX(Anim.Rayman.FallRunStart, "Rayman2/Rayman/flip", SFXPlayer.Polyphony.Mono),

            new AnimSFX(Anim.Rayman.HelicEnable, "Rayman2/Rayman/helic", SFXPlayer.Polyphony.Loop),
            new AnimSFX(Anim.Rayman.HelicIdle, "Rayman2/Rayman/helic", SFXPlayer.Polyphony.Loop),
            new AnimSFX(Anim.Rayman.HelicDisable, "Rayman2/Rayman/helicstop"),
            new AnimSFX(Anim.Rayman.HelicLandIdle, "Rayman2/Rayman/helicstop"),
            new AnimSFX(Anim.Rayman.HelicLandWalk, "Rayman2/Rayman/helicstop"),
            new AnimSFX(Anim.Rayman.HelicLandWalk, "Rayman2/Rayman/footsteps/grass", 10),
            new AnimSFX(Anim.Rayman.RunStart, "Rayman2/Rayman/footsteps/grass", 30),

            new AnimSFX(Anim.Rayman.Despawn, new SFXPlayer.Info { path = "Rayman2/Rayman/despawn", volume = 0.6f }),
            new AnimSFX(Anim.Rayman.Respawn, new SFXPlayer.Info { path = "Rayman2/Rayman/respawn", volume = 0.85f }),
        };

        public static class StdRules {
            public const string
                Air = nameof(Air),
                Ground = nameof(Ground),
                Sliding = nameof(Sliding),
                Swimming = nameof(Swimming),
                Falling = nameof(Falling),
                Climbing = nameof(Climbing);
        }

        protected override void OnStart() {
            cam = GetPersoModel<StdCam>("StdCam");

            switch (Main.lvlName)
            {
                case "Learn_60":
                case "Helic_10":
                case "Helic_20":
                case "Helic_30":
                    hasSuperHelic = true; break;
                default: hasSuperHelic = false; break;
            }

            SetShadow(true);
            SetRule(StdRules.Air);
        }


        protected override void OnUpdate() {
            onRespawn = false;
            col.wallEnabled = true;

            if (col.ground.DeathWarp ||
                col.ground.LavaDeathWarp ||
                col.ground.HurtTrigger)
                Despawn();

            else if (col.ground.FallTrigger)
                SetRule(StdRules.Falling);

            else if (col.ground.Trampoline)
                Jump(16, true);

            else if (col.wall.ClimbableWall && velY <= 2)
                SetRule(StdRules.Climbing);
        }

        PersoController shot;
        protected override void OnInputMainActor() {
            if (GetKeyDown(KeyCode.R))
                Despawn();

            if (iShootDown) {
                shot = GetPersoName("Alw_Projectile_Rayman");
                shot.SetRule("Shoot");
            }

            switch (rule)
            {
                case StdRules.Ground:
                    if (iJumpDown)
                        Jump(4, false, true);
                    break;

                case StdRules.Climbing:
                    if (iJumpDown)
                        Jump(4, false, true);
                    break;

                case StdRules.Sliding:
                    if (iJumpDown)
                        Jump(4, false, true, true); break;

                case StdRules.Swimming:
                    if (iJumpDown && col.atWaterSurface)
                        Jump(4, false); break;

                case StdRules.Air:
                    if (jumping && GetKeyUp(KeyCode.JoystickButton0) || GetKeyUp(KeyCode.A))
                        jumping = false;

                    if (iJumpDown && !(helic && hasSuperHelic) && !slideJump)
                        ToggleHelic();

                    superHelicAscend = (GetKey(KeyCode.JoystickButton0) || GetKey(KeyCode.A))
                        && (helic && hasSuperHelic);

                    // Moonjump
                    if (GetKeyDown(KeyCode.JoystickButton2) || GetKeyDown(KeyCode.S))
                        Jump(4, false);

                    break;
            }


            // Debug/Cheat stuff
            if (GetKeyDown(KeyCode.H))
                hasSuperHelic = !hasSuperHelic;

            if (GetKeyDown(KeyCode.T)) {
                var c = RayCollider.RaycastMouse();
                if (c.Any) pos = c.hit.point;
            }
        }




        //----------------------------------------
        //  Rayman Actions
        //----------------------------------------

        public void RespawnRay() {
            pos = startPos + Vector3.up * 0.5f;
            rot = startRot;
            velXZ = Vector3.zero;
            velY = 0;
            scale = 1;
            selfJump = false;
            helic = false;
            onRespawn = true;

            SetRule(StdRules.Air);
            DisableForSeconds(1.8f);
            anim.Set(Anim.Rayman.Respawn, 1);
            if (cam != null)
                cam.ResetInstant();
        }
        public void Despawn(bool respawn = true) {
            DisableForSeconds(100000);
            anim.Set(Anim.Rayman.Despawn, 1);
            if (respawn)
                t_respawn.Start(2, RespawnRay);
        }

        public void Jump(float height, bool forceMaxHeight, bool selfJump = false, bool slideJump = false)
        {
            this.selfJump = selfJump;
            this.slideJump = slideJump;
            jumping = true;
            helic = false;
            rule = StdRules.Air;

            float am = Mathf.Sqrt((1920f / 97) * height);
            jumpLiftOffVelY = slideJump ? apprVel.y / 2 : 0;
            jumpCutoff = am * 0.65f + jumpLiftOffVelY;
            velY = am * 1.25f + jumpLiftOffVelY;

            if (velXZ.magnitude < moveSpeed / 2 || !selfJump)
                anim.Set(Anim.Rayman.JumpIdleStart, 1);
            else
                anim.Set(Anim.Rayman.JumpRunStart, 1);
        }

        public void ToggleHelic()
        {
            helic = !helic;
            if (!helic) anim.Set(Anim.Rayman.HelicDisable, 1);
        }
    }
}