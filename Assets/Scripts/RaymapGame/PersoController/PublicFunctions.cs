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
            SetRule("");
            OnStart();
            pos = startPos;
            rot = startRot;
            vel = Vector3.zero;
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
        public void RotateToAngle(float angle, float t = 10)
            => rot = Quaternion.Slerp(rot, Quaternion.Euler(0, angle, 0), t * dt);
        public void RotateToDestination(Vector3 dest, float t = 10)
            => RotateToAngle(Matrix4x4.LookAt(pos, dest, Vector3.up).rotation.eulerAngles.y);
        public void SetRotY(float angle, float t = -1)
            => rot = Quaternion.Slerp(rot, Quaternion.Euler(rot.eulerAngles.x, angle, rot.eulerAngles.z), t == -1 ? 1 : t * dt);
        public void RotateY(float angle, float t = -1)
            => rot = Quaternion.Slerp(rot, Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y + angle, rot.eulerAngles.z), t == -1 ? 1 : t * dt);
        public void SetLookAt3D(Vector3 target, float t = -1)
            => rot = Quaternion.Slerp(rot, Matrix4x4.LookAt(pos, target, Vector3.up).rotation * Quaternion.Euler(0, 180, 0), t == -1 ? 1 : t * dt);
        public void SetLookAt2D(Vector3 target, float t = -1)
            => rot = Quaternion.Slerp(rot, Matrix4x4.LookAt(pos, new Vector3(target.x, pos.y, target.z), Vector3.up).rotation * Quaternion.Euler(0, 180, 0), t == -1 ? 1 : t * dt);



        // Spacial
        public float DistTo(Vector3 point)
            => Vector3.Distance(pos, point);
        public float DistToPerso(PersoController perso)
            => perso == null ? float.PositiveInfinity : Vector3.Distance(pos, perso.pos);
        public bool IsInLevel(string lvlName)
            => lvlName.ToLowerInvariant() == Main.lvlName.ToLowerInvariant();
        public bool IsInSector(int sectorIndex)
            => perso.sector == Main.controller.sectorManager.sectors[sectorIndex];
        public bool IsWithinCyl(Vector3 centre, float radius, float maxHeight)
            => !(Vector3.Distance(
                new Vector3(centre.x, 0, centre.z),
                new Vector3(pos.x, 0, pos.z))
            > radius || pos.y > maxHeight);




        // Physics & Collision
        public void ApplyGravity() {
            velY = Mathf.Clamp(velY + gravity * dt, -80, 80);
        }
        public void SetFriction(float horizontal, float vertical) {
            fricXZ = horizontal; fricY = vertical;
        }
        public void SetWallCollision(bool enabled) {
            col.wallEnabled = enabled;
        }
        public void SetWallCollision(bool enabled, float radius) {
            col.wallEnabled = enabled;
            col.radius = radius;
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
        public void NavDirection3D(Vector3 dir) {
            dir.Normalize();
            velXZ += new Vector3(dir.x, 0, dir.z) * fricXZ * moveSpeed * dt;
            velY += velY * fricY * moveSpeed * dt;
            if (navRotYSpeed > 0) SetLookAt2D(pos + dir, navRotYSpeed);
        }

        public void NavDirection(Vector3 dir) {
            dir.y = 0;
            NavDirection3D(dir);
        }

        public void NavTowards3D(Vector3 target) {
            NavDirection3D(target - pos);
        }

        public void NavTowards(Vector3 target) {
            target.y = pos.y;
            NavTowards3D(target);
        }

        public void NavForwards() {
            NavTowards3D(pos - transform.forward);
        }



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
            if (!isMainActor) return;
            if (lStick_s.magnitude > deadZone) {
                float mults = fricXZ * moveSpeed * dt * -Mathf.Clamp(lStick_s.magnitude, -1, 1);
                velXZ += mults * new Vector3(
                    Mathf.Sin(rot.eulerAngles.y * Mathf.Deg2Rad) * (1f + col.ground.hit.normal.x * 0.0f), 0,
                    Mathf.Cos(rot.eulerAngles.y * Mathf.Deg2Rad) * (1f + col.ground.hit.normal.z * 0.0f));
                /*else
                    velXZ += mults * Matrix4x4.Rotate(rot).MultiplyVector(lStick.normalized);*/
            }
        }

        public void RotateToStick(float t = 10) {
            if (!isMainActor || lStick_s.magnitude < deadZone) return;
            rot = Quaternion.Lerp(rot, Quaternion.Euler(0, lStickAngleCam, 0),
                t * Mathf.Clamp(lStick_s.sqrMagnitude, 0.2f, 50) * 2 * dt);
        }
    }
}