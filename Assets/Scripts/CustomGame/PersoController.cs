//================================
//  By: Adsolution
//================================

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using OpenSpace.Collide;
using static CustomGame.InputEx;
using CustomGame.Rayman2.Persos;

namespace CustomGame
{
    public partial class PersoController : MonoBehaviour, IInterpolate
    {
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

        public string rule;
        public Dictionary<string, MethodBase> rules = new Dictionary<string, MethodBase>();

        public static class StdRules
        {
            public const string
                Air = nameof(Air),
                Ground = nameof(Ground),
                Sliding = nameof(Sliding),
                Swimming = nameof(Swimming),
                Falling = nameof(Falling),
                Climbing = nameof(Climbing);
        }

        public MethodBase SetRule(string rule) {
            this.rule = rule;
            if (rules.ContainsKey(rule))
                return rules[rule];
            return null;
        }

        protected virtual void OnStart() { }
        protected virtual void OnInput() { }
        protected virtual void OnUpdate() { }


        // Only get allowed in scripts
        public Vector3 startPos { get; private set; }
        public float startRot { get; private set; }
        protected Vector3 posFrame { get; private set; }
        protected Vector3 posPrev { get; private set; }
        public Vector3 deltaPos { get; private set; }
        public Vector3 apprVel { get; private set; }
        protected string prevRule { get; private set; }
        protected bool newRule { get; private set; }
        public bool hasShadow { get; private set; }
        public bool visible { get; private set; }
        bool visChanged;

        public bool isMainActor => this == Main.mainActor;
        public bool outOfSector => perso.sector != Main.controller.sectorManager.GetActiveSectorWrapper(pos);
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

        public static YLT_RaymanModel rayman { get {
                if (_rayman == null) _rayman = GetRayman();
                return _rayman;
            }
        }
        static YLT_RaymanModel _rayman;

        public void SetVisibility(bool visible) {
            if (this.visible == visible) return;
            this.visible = visible;
            visChanged = true;
        }

        public bool CheckCollisionZone(PersoController perso, CollideType collideType) {
            foreach (Transform child in transform) {
                if (child.name.Contains($"Collide Set {collideType}"))
                    if (Vector3.Distance(child.transform.position,
                        perso.pos + (perso.col == null ? Vector3.zero : Vector3.up * perso.col.bottom))
                        <= child.localScale.magnitude + (perso.col == null ? 0 : perso.col.radius))
                        return true;
            }
            return false;
        }


        public void NavDirection3D(Vector3 dir) {
            dir.Normalize();
            vel += dir * moveSpeed * dt;
            SetLookAt2D(pos + dir);
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
            => rot = Quaternion.Lerp(rot, Quaternion.Euler(0, angle, 0), t * dt);
        public void RotateToDestination(Vector3 dest, float t = 10)
            => RotateToAngle(Matrix4x4.LookAt(pos, dest, Vector3.up).rotation.eulerAngles.y);

        public void SetRotYSmooth(float angle, float t)
            => rot = Quaternion.Slerp(rot, Quaternion.Euler(rot.eulerAngles.x, angle, rot.eulerAngles.z), t * dt);
        public void RotateYSmooth(float angle, float t) 
            => rot = Quaternion.Slerp(rot, Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y + angle, rot.eulerAngles.z), t * dt);
        public void SetRotY(float angle) 
            => rot = Quaternion.Euler(rot.eulerAngles.x, angle, rot.eulerAngles.z);
        public void RotateY(float angle) 
            => rot = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y + angle, rot.eulerAngles.z);
        
        public void SetLookAt3D(Vector3 target)
            => rot = Matrix4x4.LookAt(pos, target, Vector3.up).rotation;
        public void SetLookAt2D(Vector3 target, float addDegrees = 0)
            => rot = Matrix4x4.LookAt(pos, new Vector3(target.x, pos.y, target.z), Vector3.up).rotation * Quaternion.Euler(0, addDegrees, 0);
        

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
            startPos = pos;
            startRot = rot.eulerAngles.y;
            rot = transform.rotation;
            OnStart();
        }

        protected void Update() {
            if (perso == null) return;
            var s = Main.controller.sectorManager.GetActiveSectorWrapper(pos);
            if (s != null) perso.sector = s;
            if (t_disable.active) return;
            if (isMainActor) OnInput();
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
            if (rules.ContainsKey(rule))
                rules[rule].Invoke(this, null);
        }

        void LogicLoop() {
            if (outOfSector) return;

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