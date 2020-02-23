//================================
//  By: Adsolution
//================================

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using OpenSpace.Collide;
using static RaymapGame.InputEx;
using RaymapGame.Rayman2.Persos;

namespace RaymapGame {
    public partial class PersoController : MonoBehaviour, IInterpolate {
        //========================================
        //  Variables
        //========================================
        public PersoBehaviour perso;
        public AnimHandler anim;

        public Vector3 pos;
        public Quaternion rot;
        public float scale = 1;
        
        public Vector3 velXZ;
        public float velY;
        public Vector3 vel {
            get => new Vector3(velXZ.x, velY, velXZ.z);
            set {
                velXZ = new Vector3(value.x, 0, value.z);
                velY = value.y;
            }
        }

        public float fricXZ = 50, fricY = 0;
        public float moveSpeed = 10;
        public float gravity = -25;

        public int hitPoints, maxHitPoints = 1;

        public RayCollider col = new RayCollider();

        public virtual AnimSFX[] animSfx { get; }

        public virtual float activeRadius => 75;
        public virtual bool resetOnRayDeath => true;

        public string rule = "";
        public Dictionary<string, MethodBase> rules = new Dictionary<string, MethodBase>();
        object[] ruleParams;

        public MethodBase SetRule(string rule, params object[] ruleParams) {
            this.rule = rule;
            this.ruleParams = ruleParams;
            if (rules.ContainsKey(rule))
                return rules[rule];
            return null;
        }

        protected virtual void OnStart() { }
        protected virtual void OnInput() { }
        protected virtual void OnUpdate() { }


        // Only get allowed in scripts
        public Vector3 startPos { get; private set; }
        public Quaternion startRot { get; private set; }
        protected Vector3 posFrame { get; private set; }
        protected Vector3 posPrev { get; private set; }
        public Vector3 deltaPos { get; private set; }
        public Vector3 apprVel { get; private set; }
        protected string prevRule { get; private set; }
        protected bool newRule { get; private set; }
        public bool hasShadow { get; private set; }
        public bool visible { get; private set; }
        bool visChanged;

        public PersoController mainActor => Main.mainActor;
        public bool isMainActor => this == mainActor;
        public bool outOfSector => perso.sector != mainActor.perso.sector;
        public bool outOfActiveRadius => Main.mainActor == null || DistToPerso(mainActor) > activeRadius;
        public Vector3 apprVelXZCam => Matrix4x4.Rotate(Camera.main.transform.rotation).MultiplyVector(lStick3D_s);

        public string persoName => perso.perso.namePerso;
        public string persoModel => perso.perso.nameModel;
        public string persoFamily => perso.perso.nameFamily;


        // Interpolate fixed time movement
        public virtual bool interpolate => Main.useFixedTimeWithInterpolation;
        public Vector3 interpolPos => pos;
        public Quaternion interpolRot => rot;
        public float dt => interpolate ? Time.fixedDeltaTime : Time.deltaTime;


        //========================================
        //  Functions
        //========================================
        public static PersoController GetPersoName(string persoName) {
            foreach (var perso in FindObjectsOfType<PersoController>()) {
                if (perso.perso.perso.namePerso.ToLowerInvariant() == persoName.ToLowerInvariant())
                    return perso;
            }
            return null;
        }
        public static PersoController GetPersoType(string persoType) {
            foreach (var perso in FindObjectsOfType<PersoController>()) {
                if (perso.perso.perso.nameFamily.ToLowerInvariant() == persoType.ToLowerInvariant())
                    return perso;
            }
            return null;
        }
        public static PersoController GetPersoModel(string persoModel) {
            foreach (var perso in FindObjectsOfType<PersoController>()) {
                if (perso.perso.perso.nameModel.ToLowerInvariant() == persoModel.ToLowerInvariant())
                    return perso;
            }
            return null;
        }

        public static YLT_RaymanModel GetRayman() {
            return GetPersoModel("YLT_RaymanModel") as YLT_RaymanModel;
        }

        public static YLT_RaymanModel rayman => Main.rayman;
        public void SetVisibility(bool visible) {
            if (this.visible == visible) return;
            this.visible = visible;
            visChanged = true;
        }


        public float DistTo(Vector3 point) {
            return Vector3.Distance(pos, point);
        }

        public bool StoodOnByPerso(PersoController perso) {
            foreach (var c in GetComponentsInChildren<Collider>()) {
                if (perso.col.ground.hit.collider == c)
                    return true;
            }
            return false;
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

        public bool IsInLevel(string lvlName) {
            return lvlName.ToLowerInvariant() == Main.lvlName.ToLowerInvariant();
        }
        public bool IsInLevelSector(string lvlName, int sectorIndex) {
            return Main.lvlName == lvlName && perso.sector == Main.controller.sectorManager.sectors[sectorIndex];
        }

        public bool IsWithinCyl(Vector3 centre, float radius, float maxHeight) {
            centre.y = 0;
            return !(Vector3.Distance(centre, new Vector3(pos.x, 0, pos.z)) > radius || pos.y > maxHeight);
        }


        public void Reset() {
            SetRule("");
            OnStart();
            pos = startPos;
            rot = startRot;
            vel = Vector3.zero;
        }

        public float navRotYSpeed = 1;
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
        }


        Waypoint wp;
        public bool NavNearestWaypointGraph() {
            if (wp == null) {
                if ((wp = GetNearestWaypoint()) == null)
                    return false;
                if (wp.next != null && Vector3.Distance(wp.next.pos, pos) < Vector3.Distance(wp.next.pos, wp.pos)) {
                    wp = wp.next;
                }
            }

            if (InWaypointRadius(wp))
                wp = wp.next;
            if (wp == null)
                return true;

            NavToWaypoint(wp);
            return false;
        }


        public AnimSFX GetSFXLayer(int anim) {
            foreach (var s in animSfx)
                if (s.anim == anim) return s;
            return null;
        }
        public void DisableForSeconds(float seconds) {
            velY = 0;
            velXZ = Vector3.zero;
            t_disable.Start(seconds);
        }
        Timer t_disable = new Timer();

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

        DsgVarComponent dsg;
        public T GetDsgVar<T>(string name) {
            for (int v = 0; v < dsg.dsgVarEntries.Length; v++)
                if (dsg.dsgVarEntries[v].NiceVariableName == name) {
                    var e = dsg.editableEntries[v]?.valueInitial?.val;
                    if (e != null) return Dsg.GetValue<T>(e);
                }
            return default;
        }


        public float DistToPerso(PersoController perso)
            => perso == null ? float.PositiveInfinity : Vector3.Distance(pos, perso.pos);
       

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

        public void SetWallCollision(bool enabled) {
            col.wallEnabled = enabled;
        }
        public void SetWallCollision(bool enabled, float radius) {
            col.wallEnabled = enabled;
            col.radius = radius;
        }

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

        public void ApplyGravity() {
            velY = Mathf.Clamp(velY + gravity * dt, -80, 80);
        }

        public void SetFriction(float horizontal, float vertical) {
            fricXZ = horizontal; fricY = vertical;
        }


        //========================================
        //  MonoBehaviour
        //========================================
        protected void Awake() {
            visible = true;
            perso = GetComponent<PersoBehaviour>();
            dsg = GetComponent<DsgVarComponent>();
            gameObject.AddComponent<Interpolation>().fixedTimeController = this;
            anim = gameObject.AddComponent<AnimHandler>();
            anim.sfx = animSfx;
            foreach (var m in GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                if (m.Name.StartsWith("Rule_"))
                    rules.Add(m.Name.Replace("Rule_", ""), m);
        }

        protected void Start() {
            col.controller = this;
            pos = transform.position;
            rot = transform.rotation;
            startPos = pos;
            startRot = rot;
            OnStart();
        }

        bool ActiveChecks() => /*perso == null ||*/ outOfActiveRadius /*|| outOfSector*/ || t_disable.active;

        protected void Update() {
            if (isMainActor || (Main.main.alwaysControlRayman && this is YLT_RaymanModel)) {
                perso.sector = Main.controller.sectorManager.GetActiveSectorWrapper(pos);
                OnInput();
            }
            if (!interpolate) LogicLoop();
        }

        protected void LateUpdate() {
            if (visChanged) {
                foreach (var mr in GetComponentsInChildren<MeshRenderer>())
                    mr.enabled = visible;
                visChanged = false;
            }
        }

        protected void FixedUpdate() {
            if (interpolate) LogicLoop();
        }


        //========================================
        //  Logic loop
        //========================================
        void InvokeRule(string rule) {
            if (rules.ContainsKey(rule) && rule != "")
                rules[rule].Invoke(this, ruleParams);
        }

        void LogicLoop() {
            if (ActiveChecks()) return;
            if (rayman != null && resetOnRayDeath && rayman.onRespawn) {
                Reset(); return;
            }

            col.UpdateGroundCollision();
            col.UpdateWaterCollision();

            if (!t_disable.active)
                OnUpdate();

            if (!t_disable.active) {
                InvokeRule(rule);

                velY /= 1f + fricY * dt;
                velXZ /= 1f + fricXZ * dt;

                posPrev = pos;
                pos += new Vector3(velXZ.x, velY, velXZ.z) * dt;
                transform.localScale = Vector3.one * scale;
            }

            col.UpdateWallCollision();
            if (!t_disable.active)
                col.ApplyWallCollision();

            deltaPos = pos - posFrame;
            apprVel = deltaPos / dt;

            posFrame = pos;

            newRule = rule != prevRule;
            prevRule = rule;
        }
    }
}