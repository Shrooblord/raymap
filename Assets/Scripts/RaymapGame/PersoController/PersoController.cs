//================================
//  By: Adsolution
//================================

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using OpenSpace.Collide;
using static RaymapGame.InputEx;
using RaymapGame.Rayman2.Persos;
using System.Linq;

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
        public float navRotYSpeed = 1;
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
        protected virtual void OnInputMainActor() { }
        protected virtual void OnUpdate() { }


        // Only get allowed in scripts
        public Vector3 startPos { get; private set; }
        public Quaternion startRot { get; private set; }
        protected Vector3 posFrame { get; private set; }
        protected Vector3 posPrev { get; private set; }
        public Vector3 deltaPos { get; private set; }
        public Vector3 apprVel { get; private set; }
        protected string prevRule { get; private set; }
        public bool newRule { get; private set; }
        public bool hasShadow { get; private set; }
        public Waypoint waypoint { get; private set; }
        public bool visible { get; private set; }
        bool visChanged;

        public PersoController mainActor => Main.mainActor;
        public bool isMainActor => this == mainActor;
        public bool outOfSector => perso.sector != mainActor.perso.sector;
        public bool outOfActiveRadius => Main.mainActor == null || DistToPerso(mainActor) > activeRadius;
        public Vector3 apprVelXZCam => Matrix4x4.Rotate(Camera.main.transform.rotation).MultiplyPoint3x4(-apprVel);

        public string persoName => perso.perso.namePerso;
        public string persoModel => perso.perso.nameModel;
        public string persoFamily => perso.perso.nameFamily;


        // Interpolate fixed time movement
        public virtual bool interpolate => Main.useFixedTimeWithInterpolation;
        public Vector3 interpolPos => pos;
        public Quaternion interpolRot => rot;
        public float dt => interpolate ? Time.fixedDeltaTime : Time.deltaTime;
    }
}