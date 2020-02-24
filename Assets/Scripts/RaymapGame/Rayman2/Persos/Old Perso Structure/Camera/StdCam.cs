//================================
//  By: Adsolution
//================================

using UnityEngine;
using static RaymapGame.InputEx;
using static RaymapGame.Rayman2.Persos.YLT_RaymanModel;

namespace RaymapGame.Rayman2.Persos {
    public partial class StdCam : PersoController {
        public PersoController targ => Main.mainActor;
        public Camera cam;
        public bool isRay => targ is YLT_RaymanModel;
        public Vector3 posOffset = new Vector3(0, 4, -7.5f);

        public override bool resetOnRayDeath => false;
        public override bool interpolate => false;
        public override float activeRadius => 100000;

        protected override void OnStart() {
            col.wallEnabled = true;
            lStickAngle_s = lStickAngle;
            SetRule("Default");
        }


        float a;
        float lStickAngle_s;
        Quaternion xr = Quaternion.identity;

        public void SetOffset(Vector3 offset) => this.offset = offset;
        Vector3 offset;

        public bool resetting;
        public void ResetInstant()
        {
            t_resetting.Start(0.05f);
        }

        float mvrotspd = 0.135f;
        Timer t_mvrotspd = new Timer();
        bool persoNewState;
        string persoRule;
        Vector3 cen;

        Vector3 lookOff;
        public void SetLookOffset(Vector3 off)
        {
            lookOff = Vector3.Lerp(lookOff, off, Time.deltaTime * 4);
        }

        Timer t_t_y = new Timer();
        public float t_y { get => _t_ty; set { _t_ty = Mathf.Lerp(_t_ty, value, Time.deltaTime * 5); } }
        float _t_ty;

        public void SetPosOffset(Vector3 dest, float t_xz)
        {
            posOffset.x = Mathf.Lerp(posOffset.x, dest.x, t_xz * dt);
            posOffset.y = Mathf.Lerp(posOffset.y, dest.y, t_y * dt);
            posOffset.z = Mathf.Lerp(posOffset.z, dest.z, t_xz * dt);
        }

        Timer t_resetting = new Timer();


        protected override void OnInput() {
            if (Input.GetKeyDown(KeyCode.Y)) {
                if (rule == "Default") {
                    SetRule("Free");
                }
            }
        }

        protected override void OnUpdate() {
            LevelRules();

            Camera.main.transform.position = transform.position;
            Camera.main.transform.rotation = transform.rotation;
        }





        void Rule_Default()
        {
            if (targ == null) return;

            if (t_resetting.active)
            {
                pos = targ.transform.position + Matrix4x4.Rotate(Quaternion.Euler(0, targ.transform.eulerAngles.y, 0)).
                    MultiplyPoint3x4(posOffset);
                cen = targ.transform.position;
                rot = Quaternion.Euler(20, targ.transform.eulerAngles.y + 180, 0);
                return;
            }

            if (targ.rule != null) {
                persoNewState = persoRule != targ.rule;
                persoRule = targ.rule;
            }
            else
                persoRule = StdRules.Ground;

            bool following = persoRule != StdRules.Falling;

            if (following)
            {
                lStickAngle_s = Mathf.Lerp(lStickAngle_s, lStickAngle, Time.deltaTime * 12);

                if (targ.rule == StdRules.Air)
                {
                    mvrotspd = 0.08f;
                    if (isRay && rayman.jumping && targ.velY > 0)
                        t_y = 1.5f;
                    else if (targ.col.groundFar.hit.distance > 4)
                        t_t_y.Start(0.3f, () => t_y = 4);
                    else if (!t_t_y.active)
                        t_y = 9;
                }
                else if (targ.rule == StdRules.Ground && persoNewState)
                    t_mvrotspd.Start(0.3f, () => mvrotspd = 0.135f);


                if (persoRule == StdRules.Ground)
                    t_y = 11;
                if (persoRule == StdRules.Climbing)
                    t_y = 0.35f;
                else
                    t_y = 3;
                if (isRay && !rayman.helic)
                {
                    float grndNrmOff = 0;
                    if (persoRule == StdRules.Ground)
                        grndNrmOff = -targ.apprVel.y * 0.4f;

                    SetPosOffset(new Vector3(0, 4 + grndNrmOff, -7.5f), 3);
                }

                //if (isRay && persoRay.strafing)
                    //rot.y = targ.rot.eulerAngles.y + 180;
                if (persoRule == StdRules.Sliding)
                {
                    t_y = 15;
                    SetPosOffset(new Vector3(0, 7, -6.5f), 2);
                }
                else if (lStickAngle > -160 && lStickAngle < 160 && persoRule != StdRules.Sliding)
                {
                    RotateY(Mathf.Clamp(-lStickAngle_s * new Vector3(targ.apprVel.x, 0, targ.apprVel.z).magnitude
                        * mvrotspd, -120, 120), 1);
                }
                else if (targ.velXZ.magnitude > 2)
                {
                    t_y = 5;
                    SetPosOffset(new Vector3(0, 6f, -13), 5);
                }

                if (isRay && rayman.helic)
                {
                    t_y = 3;
                    if (!rayman.hasSuperHelic)
                        SetPosOffset(new Vector3(0, 5.5f, -9f), 1);
                    else SetPosOffset(new Vector3(0, 4, -7.5f), 0.5f);
                }


                if (rStick_s.magnitude > deadZone)
                    RotateY(-rStick_s.x * 110, 1);
            }
            else
            {
                rot = Quaternion.Slerp(rot, Matrix4x4.LookAt(pos, targ.transform.position, Vector3.up).rotation,
                    dt * 2);
            }

            float vlerp = 6;
            if (targ.velY < 0 && targ.rule == StdRules.Air) vlerp = 3;

            var lookRot = Matrix4x4.LookAt(pos, targ.transform.position + Vector3.up * 1, Vector3.up);
            rot.eulerAngles = new Vector3(-1 + lookRot.rotation.eulerAngles.x, rot.eulerAngles.y, 0);


            xr = Quaternion.Slerp(xr, rot, 5 * dt);
            if (following)
            {
                if (targ.col.groundFar.hit.distance < 3)
                    SetLookOffset(new Vector3(0, -targ.col.groundFar.hit.distance + 1, 0));
                else SetLookOffset(Vector3.zero);

                cen = targ.transform.position + lookOff;
            }
            var off = cen + Matrix4x4.Rotate(Quaternion.Euler(0, rot.eulerAngles.y, 0)).
                MultiplyPoint3x4(posOffset);


            pos.y = Mathf.Lerp(pos.y, off.y, dt * t_y);



            var posXZ = new Vector3(pos.x, 0, pos.z);
            posXZ = Vector3.Lerp(posXZ, off, dt * 5);
            pos.x = posXZ.x;
            pos.z = posXZ.z;

            var cpos = pos;
            col.ApplyWallCollision(ref cpos);
            pos = Vector3.Lerp(pos, cpos, dt * 30);
            rot = Quaternion.Euler(xr.eulerAngles.x, rot.eulerAngles.y, 0);

            persoNewState = false;
        }
    }
}