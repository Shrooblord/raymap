//================================
//  By: Adsolution
//================================

using UnityEngine;
using static RaymapGame.InputEx;

namespace RaymapGame.Rayman2.Persos {
    public partial class YLT_RaymanModel {
        void Rule_Ground()
        {
            #region Rule
            col.groundDepth = groundDepth;
            col.UpdateGroundCollision();

            if (newRule && lStick.magnitude < deadZone)
                velXZ = Vector3.zero;

            slideJump = false;
            selfJump = false;


            if (col.ground.AnyGround && col.ground.hit.distance < 1.5f) {
                col.StickToGround();
            }
            else if (col.ground.Slide) {
                SetRule("Sliding"); return;
            }
            else if (col.water.Water && !col.waterIsShallow) {
                SetRule("Swimming"); return;
            }
            else {
                SetRule("Air"); return;
            }


            SetFriction(30, 0);

            if (strafing) moveSpeed = 7;
            else moveSpeed = 10;

            InputMovement();
            RotateToStick(10);
            rot = Quaternion.Slerp(rot, Quaternion.Euler(0, rot.eulerAngles.y, 0), dt * 10);
            #endregion
            #region Animation
            if (velXZ.magnitude < 0.05f)
            {
                t_runStart.Start(0.033f);
                if (newRule)
                {
                    if (helic)
                        anim.Set(Anim.Rayman.HelicLandIdle, 1);
                    else
                        anim.Set(Anim.Rayman.LandIdle, 1);
                }
                else
                {
                    anim.Set(Anim.Rayman.Idle, 0);
                }
                if (anim.currAnim == Anim.Rayman.RunStop)
                    anim.SetSpeed(40);
                else anim.SetSpeed(25);
            }
            else if (velXZ.magnitude < 5)
            {
                if (newRule)
                {
                    if (helic)
                        anim.Set(Anim.Rayman.HelicLandWalk, 1);
                    else
                        anim.Set(Anim.Rayman.LandWalk, 1);
                }
                else
                    anim.Set(Anim.Rayman.Walk, 0);
                float spd = velXZ.magnitude * moveSpeed * 1.5f;

                if (anim.currAnim == Anim.Rayman.HelicLandWalk)
                    anim.SetSpeed(spd / 2);
                else
                    anim.SetSpeed(spd);
            }
            else
            {
                if (newRule)
                {
                    if (helic)
                        anim.Set(Anim.Rayman.HelicLandWalk, 1);
                    else
                        anim.Set(Anim.Rayman.LandRun, 1);
                }

                else
                {
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

            if ((anim.currAnim == Anim.Rayman.RunStop || velXZ.magnitude < 0.05f) && lStick.magnitude >= deadZone)
            {
                anim.Set(Anim.Rayman.RunStart, 1);
            }
            else if (velXZ.magnitude > 5 && lStick.magnitude < deadZone)
            {
                anim.Set(Anim.Rayman.RunStop, 1);
            }
            #endregion

            helic = false;
        }


        void Rule_Air()
        {
            #region Rule
            col.groundDepth = 0;
            col.UpdateGroundCollision();

            if (newRule)
                liftOffVel = velXZ.magnitude;

            if (col.ground.AnyGround && velY <= 0)
            {
                velY = 0;
                SetRule(StdRules.Ground);
                return;
            }
            else if (col.ground.Slide)
            {
                SetRule(StdRules.Sliding);
                return;
            }
            else if (col.ground.Water && velY < 0 && !col.waterIsShallow)
            {
                SetRule(StdRules.Swimming);
                return;
            }

            if (jumping)
            {
                gravity = -13;
                if (velY < jumpCutoff)
                    jumping = false;
            }
            else
            {
                gravity = -25;
            }

            ApplyGravity();

            if (helic)
            {
                if (superHelicAscend)
                    superHelicRev = Mathf.Lerp(superHelicRev, 38, dt * 45);
                else superHelicRev = Mathf.Lerp(superHelicRev, 0, dt * 1);

                //GetSFXLayer(Anim.YLT_RaymanModel.HelicIdle).player.asrc.pitch = 1 + superHelicRev / 300;

                SetFriction(10, hasSuperHelic ? 2.5f : 7);
                moveSpeed = 5;
                velY += dt * superHelicRev;
                velY = Mathf.Clamp(velY, hasSuperHelic ? -25 : -5, 5);
                selfJump = false;
            }
            else
            {
                if (slideJump)
                    SetFriction(0.1f, 0);
                else SetFriction(5, 0);
                moveSpeed = 10;
            }

            RotateToStick(6);
            InputMovement();


            if (pos.y < startPos.y - 1100)
                SetRule(StdRules.Falling);
            #endregion
            #region Animation
            if (helic)
            {
                anim.Set(Anim.Rayman.HelicEnable, 1);
            }
            else if (liftOffVel < 5 || !selfJump)
            {
                if (velY > 5 + jumpLiftOffVelY)
                {
                    anim.Set(Anim.Rayman.JumpIdleLoop, 0);
                }
                else
                {
                    if (newRule)
                        anim.Set(Anim.Rayman.FallIdleLoop, 0);
                    else
                        anim.Set(Anim.Rayman.FallIdleStart, 1);
                }
            }
            else
            {
                if (velY > 5 + jumpLiftOffVelY)
                {
                    anim.Set(Anim.Rayman.JumpRunLoop, 0);
                }
                else
                {
                    if (newRule)
                        anim.Set(Anim.Rayman.FallRunLoop, 0);
                    else
                        anim.Set(Anim.Rayman.FallRunStart, 1);
                }
            }
            #endregion
        }


        void Rule_Climbing()
        {
            #region Rule
            if (newRule)
            {
                velXZ = Vector3.zero;
                velY = 0;
                anim.Set(Anim.Rayman.ClimbWallStart, 1);

                if (col.wall.hit.point != Vector3.zero)
                    pos = col.wall.hit.point + col.wall.hit.normal * 0.5f;
                colClimb = col.wall;
            }

            if ((colClimb = RayCollider.Raycast(pos + colClimb.hit.normal, -colClimb.hit.normal, 3)).ClimbableWall)
            {
                rot = Matrix4x4.LookAt(pos, pos + colClimb.hit.normal, Vector3.up).rotation;
                pos = colClimb.hit.point + colClimb.hit.normal * 0.5f;
                if (lStick.magnitude > deadZone)
                    pos += Matrix4x4.Rotate(rot).MultiplyVector(new Vector2(-lStick_s.x, lStick_s.y)) * 6 * dt;
            }

            else if (apprVel.y > 2 && lStickAngle * Mathf.Sign(lStickAngle) < 30)
                Jump(4, false);

            col.wallEnabled = false;
            #endregion
            #region Animation
            float la = 0;
            if (lStick_s.magnitude > deadZone)
            {
                la = lStickAngle;
                anim.SetSpeed(lStick_s.magnitude * 35);
                if (la > -45 && la < 45)
                    anim.Set(Anim.Rayman.ClimbWallUpStart, 1);
                else if (la >= 45 && la <= 135)
                    anim.Set(Anim.Rayman.ClimbWallRightStart, 1);
                else if (la > 135 || la < -135)
                    anim.Set(Anim.Rayman.ClimbWallDownStart, 1);
                else if (la >= -135 && la <= -45)
                    anim.Set(Anim.Rayman.ClimbWallLeftStart, 1);
            }
            else
            {
                anim.SetSpeed(25);
                if (la > -45 && la < 45)
                    anim.Set(Anim.Rayman.ClimbWallUpEnd, 1);
                else if (la > 45 && la < 135)
                    anim.Set(Anim.Rayman.ClimbWallRightEnd, 1);
                else if (la > 135 || la < -135)
                    anim.Set(Anim.Rayman.ClimbWallDownEnd, 1);
                else if (la > -135 && la < -45)
                    anim.Set(Anim.Rayman.ClimbWallLeftEnd, 1);
            }
            #endregion
        }


        void Rule_Falling()
        {
            if (newRule) scale = 1;
            if (scale <= 0) return;
            scale -= dt / 2.5f;
            if (scale <= 0) t_respawn.Start(0.1f, RespawnRay);

            SetFriction(1, 2);
            ApplyGravity();

            anim.Set(Anim.Rayman.DeathFall, 1);
        }


        void Rule_Sliding()
        {
            anim.SetSpeed(20);
            if (newRule)
            {
                anim.Set(Anim.Rayman.RunToSlide1, 1);
                velXZ += Vector3.ClampMagnitude(col.ground.hit.normal * -velY, 20);
                velY = 0;
            }

            col.groundDepth = groundDepth;
            col.UpdateGroundCollision();

            moveSpeed = 15 + 10 * Mathf.Clamp(lStick_s.y, deadZone, 1);
            SetFriction(0.1f, 3 * -Mathf.Clamp(lStick_s.y, -1, -deadZone));


            if (col.ground.Slide)
            {
                col.StickToGround();
                velXZ += col.ground.hit.normal * moveSpeed * dt;

                velXZ = Vector3.ClampMagnitude(velXZ + Matrix4x4.Rotate(Quaternion.Euler(0,
                    rot.y + UnityEngine.Camera.main.transform.rotation.eulerAngles.y, 0)).MultiplyVector(Vector3.right)
                    * lStick_s.x * 15 * dt, velXZ.magnitude);
            }
            else if (col.ground.AnyGround)
            {
                SetRule(StdRules.Ground);
                if (lStick_s.magnitude > deadZone)
                    anim.Set(Anim.Rayman.SlideToRun, 2);
                else anim.Set(Anim.Rayman.SlideToIdle, 2);

                return;
            }
            else if (col.ground.None)
            {
                Jump(4, true, true, true);
                return;
            }

            FaceVel3D();
        }


        void Rule_Swimming()
        {
            if (col.atWaterSurface && col.ground.AnyGround)
            {
                SetRule(StdRules.Ground);
                return;
            }

            if (newRule)
            {
                anim.Set(Anim.Rayman.SwimEnter, 1);
                helic = false;
            }

            anim.SetSpeed(25);
            SetFriction(3, 7);
            moveSpeed = 7;

            col.ApplyWaterCollision(ref pos, ref velY);


            #region Correct hair for above/under water

            var ch = transform.Find("Channel 8");
            if (ch == null)
            {
                ch = transform.Find("Channel 0");
                if (ch != null) ch = ch.Find("Channel 8");
            }
            if (ch != null)
            {
                var surfHairLeft = ch.Find("Channel 12");
                var surfHairRight = ch.Find("Channel 13");
                var underHairLeft = ch.Find("Channel 5");
                var underHairRight = ch.Find("Channel 4");

                if (surfHairRight != null) surfHairRight.gameObject.SetActive(col.atWaterSurface);
                if (surfHairLeft != null) surfHairLeft.gameObject.SetActive(col.atWaterSurface);
                if (underHairRight != null) underHairRight.gameObject.SetActive(!col.atWaterSurface);
                if (underHairLeft != null) underHairLeft.gameObject.SetActive(!col.atWaterSurface);
            }
            #endregion


            if (lStick_s.magnitude > deadZone)
            {
                InputMovement();
                RotateToStick(2);
                anim.Set(Anim.Rayman.SwimStartMove, 0);
                if (!anim.IsSet(Anim.Rayman.SwimEnter))
                    anim.SetSpeed(lStick_s.magnitude * moveSpeed * 3);
            }
            else
            {
                anim.Set(Anim.Rayman.SwimStopMove, 0);
                anim.SetSpeed(22);
            }
        }
    }
}