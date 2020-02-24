//================================
//  By: Adsolution
//================================
using UnityEngine;
using static RaymapGame.InputEx;

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Camera
    /// </summary>
    public partial class StdCam : Camera {
        public bool targIsRay => targ is YLT_RaymanModel;
        public PersoController targ => mainActor;

        public static Vector3 defaultOffset = new Vector3(0, 4, 8.5f);
        public static float defaultOrbitSpeed = 0.135f;

        float orbRot;
        float orbSpd;
        float orbVel = defaultOrbitSpeed;
        Vector3 focus;
        Vector3 offset = defaultOffset;
        Timer t_land_orbSpd = new Timer();
        Timer t_tY = new Timer();

        float tY { get => _tY; set { _tY = Mathf.Lerp(_tY, value, dt * 5); } }
        float _tY;

        public void SetOrbitRot(float degrees, float t = -1)
            => orbRot = Quaternion.Slerp(Quaternion.Euler(0, orbRot, 0), Quaternion.Euler(0, degrees, 0), tCheck(t)).eulerAngles.y;
        public void SetOrbitOffset(Vector3 offset, float t = -1)
            => this.offset = Vector3.Lerp(this.offset, offset, tCheck(t));



        protected override void OnStart() {
            col.wallEnabled = true;
            SetRule("Follow");
        }


        protected override void OnInput() {
            if (Input.GetKeyDown(KeyCode.Y)) {
                if (rule == "Default") {
                    SetRule("Free");
                }
            }
        }

        protected override void OnUpdate() {
            LevelRules();
            cam.transform.position = pos;
            cam.transform.LookAt(pos + forward, Vector3.up);
        }


        protected void Rule_Follow() {
            // Manual rotate
            orbRot -= (rStick.x * 135) * dt;

            // Lower auto orbit speed when in air for any actor with matching rule names
            if (targ.rule == "Air") {
                t_land_orbSpd.Abort(); orbSpd = 0.08f;
            }
            else if (targ.newRule) {
                t_land_orbSpd.Start(0.3f, () => orbSpd = defaultOrbitSpeed, false);
            }

            // Auto rotate while moving
            if (lStickPress && lStickAngle > -160 && lStickAngle < 160)
                orbVel = Mathf.Lerp(orbVel, Mathf.Clamp(-lStickAngle * new Vector3(targ.apprVel.x, 0, targ.apprVel.z).magnitude
                    * orbSpd, -120, 120), 15 * dt);
            else orbVel = Mathf.Lerp(orbVel, 0, 20 * dt);
            orbRot += orbVel * dt;


            // Always offset for general actors
            if (!targIsRay) {
                tY = 20;
                SetOrbitOffset(defaultOffset);
            }

            // Rayman-specific offsets
            else {
                if (rayman.helic) {
                    if (!rayman.hasSuperHelic)
                        SetOrbitOffset(defaultOffset + new Vector3(0, 2.5f, 1.5f), 1);
                    else SetOrbitOffset(new Vector3(0, 3, 9), 1);
                }
                else if (targ.rule == "Ground") {
                    tY = 9;
                    if (targ.velXZ.magnitude < targ.moveSpeed / 2)
                        SetOrbitOffset(defaultOffset, 1);
                    else SetOrbitOffset(defaultOffset + new Vector3(0, 0.7f * -targ.apprVel.y, 0.6f), 3);
                }
                else if (targ.rule == "Air") {
                    if (rayman.jumping && targ.velY > 0)
                        tY = 1.5f;
                    else if (targ.col.groundFar.hit.distance > 4)
                        t_tY.Start(0.3f, () => tY = 3);
                }

                //if (targ.col.groundFar.AnyGround && targ.col.groundFar.hit.distance < 4)
                //SetOrbitOffset(defaultOffset + Vector3.down * ((pos.y - targ.pos.y) + targ.col.groundFar.hit.distance), 3);
            }

            // Transform
            Orbit(targ.pos, orbRot, offset, tY, 8);
            LookAtY(targ.pos, 0);
            LookAtX(targ.pos, 8, 4);
        }
    }
}