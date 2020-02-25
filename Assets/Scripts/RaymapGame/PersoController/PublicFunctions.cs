//================================
//  By: Adsolution
//================================
using UnityEngine;
using OpenSpace.Collide;
using static RaymapGame.InputEx;

namespace RaymapGame {
    public partial class PersoController {
        //========================================
        //  Public Functions
        //========================================

        // Management
        public void Reset() {
            pos = startPos;
            rot = startRot;
            vel = Vector3.zero;
            SetRule("");
            DisableForSeconds(1.2f);
            OnStart();
        }
        public void DisableForSeconds(float seconds) {
            velY = 0;
            velXZ = Vector3.zero;
            t_disable.Start(seconds);
        }
        Timer t_disable = new Timer();

        DsgVarComponent dsg;
        public T GetDsgVar<T>(string name) {
            for (int v = 0; v < dsg.dsgVarEntries.Length; v++)
                if (dsg.dsgVarEntries[v].NiceVariableName == name) {
                    var e = dsg.editableEntries[v]?.valueInitial?.val;
                    if (e != null) return Dsg.GetValue<T>(e);
                }
            return default;
        }



        // Visual
        public void SetVisibility(bool visible) {
            if (this.visible == visible) return;
            this.visible = visible;
            visChanged = true;
        }

        public Shadow shadow;
        public void SetShadow(bool enabled) {
            if (shadow == null && enabled)
                shadow = ResManager.Inst("Shadow", this).GetComponent<Shadow>();
            else if (shadow != null && !enabled) {
                Destroy(shadow.gameObject);
                shadow = null;
            }
            hasShadow = enabled;
        }



        // Audio
        public AnimSFX GetSFXLayer(int anim) {
            foreach (var s in animSfx)
                if (s.anim == anim) return s;
            return null;
        }


        // Transform
        public void SetRotY(float angle, float t = -1)
            => rot = Quaternion.Slerp(rot, Quaternion.Euler(rot.eulerAngles.x, angle, rot.eulerAngles.z), tCheck(t));
        public void RotateY(float angle, float t = -1)
            => rot.eulerAngles += new Vector3(0, angle, 0) * tCheck(t);
        //public void SetLocalRotY(float angle, float t = -1)
            //=> rot = Quaternion.Slerp(rot, Quaternion.Euler(rot.eulerAngles.x, angle, rot.eulerAngles.z), tCheck(t));
        //public void RotateLocalY(float angle, float t = -1)
            //=> rot = Quaternion.AxisAngle( * tCheck(t);
        public void LookAt3D(Vector3 target, float t = -1)
            => rot = lookAt(target, 0, 0, t);
        public void LookAt2D(Vector3 target, float t = -1)
            => LookAt3D(new Vector3(target.x, pos.y, target.z), t);
        public void LookAtX(Vector3 target, float addDegrees, float t = -1)
            => rot.eulerAngles = new Vector3(lookAt(target, addDegrees, 0, t).eulerAngles.x, rot.eulerAngles.y, 0);
        public void LookAtY(Vector3 target, float addDegrees, float t = -1)
            => rot.eulerAngles = new Vector3(rot.eulerAngles.x + addDegrees, lookAt(target, 0, addDegrees, t).eulerAngles.y, 0);
        public void FaceDir3D(Vector3 dir, float t = -1)
            => LookAt3D(pos + dir, t);
        public void FaceDir2D(Vector3 dir, float t = -1)
            => LookAt2D(pos + dir, t);
        public void FaceVel3D(float t = -1)
            => LookAt3D(pos + apprVel, t);
        public void FaceVel2D(float t = -1)
            => LookAt2D(pos + apprVel, t);
        public void Orbit(Vector3 target, float angle, Vector3 offset, float t_v = -1, float t_h = -1) {
            var targ = target +
                Matrix4x4.Rotate(Quaternion.Euler(0, angle - 180, 0))
                .MultiplyPoint3x4(offset);
            pos.x = Mathf.Lerp(pos.x, targ.x, tCheck(t_h));
            pos.z = Mathf.Lerp(pos.z, targ.z, tCheck(t_h));
            pos.y = Mathf.Lerp(pos.y, targ.y, tCheck(t_v));
        }

        // transform helpers
        protected float tCheck(float t) => t == -1 ? 1 : t * dt;
        Quaternion lookAt(Vector3 target, float addDegreesX, float addDegreesY, float t)
            => Quaternion.Slerp(rot, Matrix4x4.LookAt(pos, target, Vector3.up).rotation * Quaternion.Euler(addDegreesX, addDegreesY - 180, 0), tCheck(t));




        // Spacial
        public Vector3 forward
            => Matrix4x4.Rotate(rot).MultiplyVector(-Vector3.forward);
        public float DistTo(Vector3 point)
            => Vector3.Distance(pos, point);
        public float DistTo2D(Vector3 point)
            => Vector3.Distance(new Vector3(point.x, 0, point.z), new Vector3(pos.x, 0, pos.z));
        public float DistToPerso(PersoController perso)
            => perso == null ? float.PositiveInfinity : DistTo(perso.pos);
        public float DistToPerso2D(PersoController perso)
            => perso == null ? float.PositiveInfinity : DistTo2D(perso.pos);
        public bool IsInLevel(string lvlName)
            => lvlName.ToLowerInvariant() == Main.lvlName.ToLowerInvariant();
        public bool IsInSector(int sectorIndex)
            => perso.sector == Main.controller.sectorManager.sectors[sectorIndex];
        public bool IsWithinCyl(Vector3 centre, float radius, float maxHeight)
            => DistTo2D(centre) < radius && pos.y < maxHeight;




        // Physics & Collision
        public void SetVelH(float x, float z, float t = -1)
            => velXZ = Vector3.Lerp(velXZ, new Vector3(x, 0, z), tCheck(t));
        public void SetVelV(float y, float t = -1)
            => velY = Mathf.Lerp(velY, y, tCheck(t));
        public void ApplyGravity()
            => velY = Mathf.Clamp(velY + gravity * dt, -80, 80);
        public void SetFriction(float horizontal, float vertical) {
            fricXZ = horizontal; fricY = vertical;
        }
        public float GetCollisionRadius(CollideType collideType) {
            if (perso == null || t_disable.active) return 0;
            foreach (Transform child in transform)
                if (child.name.Contains($"Collide Set {collideType}"))
                    return child.localScale.magnitude;
            return 0;
        }
        public bool CheckCollisionZone(PersoController perso, CollideType collideType) {
            return !(perso == null || t_disable.active)
                && DistToPerso(perso) < GetCollisionRadius(collideType) + perso.GetCollisionRadius(collideType);
        }
        public bool StoodOnByPerso(PersoController perso) {
            foreach (var c in GetComponentsInChildren<Collider>()) {
                if (perso.col.ground.hit.collider == c)
                    return true;
            }
            return false;
        }




        // Navigation
        public void NavDirection3D(Vector3 dir, bool tank = false) {
            if (navRotSpeed > 0) FaceDir2D(dir, navRotSpeed);
            var vec = tank ? forward : dir.normalized;
            velXZ += vec * fricXZ * moveSpeed * dt;
            velY += vec.y * fricY * moveSpeed * dt;
        }
        public void NavDirection(Vector3 dir, bool tank = true)
            => NavDirection3D(new Vector3(dir.x, 0, dir.z), tank);
        public void NavDirectionCam(Vector3 dir, bool tank = true)
            => NavDirection3D(Matrix4x4.Rotate(Camera.main.transform.rotation).MultiplyPoint3x4(dir));
        public void NavTowards3D(Vector3 target, bool tank = true)
            => NavDirection3D(target - pos, tank);
        public void NavTowards(Vector3 target, bool tank = true)
            => NavTowards3D(new Vector3(target.x, pos.y, target.z), tank);
        public void NavForwards()
            => NavTowards3D(pos - forward);
       



        // Waypoint navigation
        public Waypoint GetNearestWaypoint() {
            Waypoint closest = null;
            float cdist = 1000000;
            foreach (var wp in Waypoint.all) {
                float dist = DistTo(wp.transform.position);
                if (dist < cdist) {
                    cdist = dist;
                    closest = wp;
                }
            }
            return closest;
        }
        public bool InWaypointRadius(Waypoint wp) {
            var ch = transform.GetChild(0);
            float dist = DistTo(wp.pos);
            if (ch == null) return dist < 2;
            else return dist < ch.transform.localScale.magnitude;
        }
        public void NavToWaypoint(Waypoint wp) {
            NavTowards(wp.pos);
            waypoint = wp;
        }

        Waypoint wp { get => waypoint; set { waypoint = value; } }
        public bool NavNearestWaypointGraph() {
            if (wp == null) {
                if ((wp = GetNearestWaypoint()) == null)
                    return false;
                if (wp.next != null && DistTo(wp.next.pos) < Vector3.Distance(wp.pos, wp.next.pos))
                    wp = wp.next;
            }
            if (InWaypointRadius(wp))
                wp = wp.next;
            if (wp == null)
                return true;

            NavToWaypoint(wp);
            return false;
        }



        // Input navigation
        public void InputMovement() {
            if ((isMainActor || Main.main.alwaysControlRayman)
                && lStick_s.magnitude > deadZone) {
                float mults = fricXZ * moveSpeed * dt * -Mathf.Clamp(lStick_s.magnitude, -1, 1);
                velXZ += mults * new Vector3(
                    Mathf.Sin(rot.eulerAngles.y * Mathf.Deg2Rad) * (1f + col.ground.hit.normal.x * 0.0f), 0,
                    Mathf.Cos(rot.eulerAngles.y * Mathf.Deg2Rad) * (1f + col.ground.hit.normal.z * 0.0f));
                /*else
                    velXZ += mults * Matrix4x4.Rotate(rot).MultiplyVector(lStick.normalized);*/
            }
        }

        public void RotateToStick(float t = 10) {
            if ((isMainActor || Main.main.alwaysControlRayman)
                && lStick_s.magnitude < deadZone) return;
            rot = Quaternion.Lerp(rot, Quaternion.Euler(0, lStickAngleCam, 0),
                t * Mathf.Clamp(lStick_s.sqrMagnitude, 0.2f, 50) * 2 * dt);
        }
    }
}