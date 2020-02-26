//================================
//  By: Adsolution
//================================
using UnityEngine;
using RaymapGame.Rayman2.Persos;
using System.Reflection;
using System.Linq;

namespace RaymapGame {
    public partial class PersoController {
        protected void Awake() {
            visible = true;
            perso = GetComponent<PersoBehaviour>();
            dsg = GetComponent<DsgVarComponent>();
            gameObject.AddComponent<Interpolation>().fixedTimeController = this;
            anim = gameObject.AddComponent<AnimHandler>();
            anim.sfx = animSfx;

            // Get rules
            foreach (var m in GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => !x.IsPublic && x.Name.StartsWith("Rule_")))
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

        bool ActiveChecks() => t_disable.active
            //|| outOfSector
            || outOfActiveRadius;

        protected void Update() {
            if (isMainActor || (Main.main.alwaysControlRayman && this is YLT_RaymanModel)) {
                OnInputMainActor();
            }

            perso.sector = Main.controller.sectorManager.GetActiveSectorWrapper(pos);

            if (!ActiveChecks())
                OnInput();
            if (!interpolate) LogicLoop();
        }

        protected virtual void LateUpdate() {
            if (visChanged) {
                foreach (var mr in GetComponentsInChildren<MeshRenderer>())
                    mr.enabled = visible;
                visChanged = false;
            }
            if (HD)
                foreach (var mr in GetComponentsInChildren<MeshRenderer>()) {
                    if (mr.material.name == "mat_gouraud (Instance)") {
                        var tex = mr.material.GetTexture("_Tex0");
                        mr.material = new Material(Shader.Find("Standard"));
                        mr.material.mainTexture = tex;
                        mr.receiveShadows = true;
                        mr.material.SetFloat("_Glossiness", 0);
                    }
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