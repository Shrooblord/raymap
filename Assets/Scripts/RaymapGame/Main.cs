//================================
//  By: Adsolution
//================================

using System;
using RaymapGame.Rayman2.Persos;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

namespace RaymapGame
{
    public class Main : MonoBehaviour {
        public PersoController _mainActor; // Inspector display only
        public static bool useFixedTimeWithInterpolation = true;
        public static Main main;
        public bool alwaysControlRayman;
        public bool showLiveScripts;
        public bool emptyLevel;
        public static PersoController mainActor;
        public static YLT_RaymanModel rayman;
        public static Type[] persoScripts = new Type[0];
        public static List<PersoController> persos = new List<PersoController>();
        public static StdCam cam;
        public static EnvHandler env;
        public static Controller controller;
        public static AudioSource music;
        public static bool showMainActorDebug;
        public static bool loaded;
        public static event EventHandler onLoad;
        void Main_onLoad(object sender, EventArgs e) { }


        public static string lvlName => controller.loader.lvlName;
        bool canLoad => controller.loader.loadingState == "Filling in comport names";
        public static string gameName => "Rayman2";

        public static PersoController SetMainActor(PersoController perso) {
            return main._mainActor = mainActor = perso;
        }

        public static Type[] GetPersoScripts() {
            return persoScripts = (from t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
            where t.IsClass && t.Namespace == $"{nameof(RaymapGame)}.{gameName}.Persos" && !t.IsAbstract
            select t).ToArray();
        }


        void Awake() {
            main = this;
            GetPersoScripts();
            controller = FindObjectOfType<Controller>();
            music = GetComponent<AudioSource>();
            env = gameObject.AddComponent<EnvHandler>();
            onLoad += Main_onLoad;
        }

        void Update() {
            if (!loaded && controller.loaded)
                Load();

            // Debug
            if (Input.GetKeyDown(KeyCode.D))
                showMainActorDebug = !showMainActorDebug;
        }


        public void ApplyPersoScript(PersoBehaviour pb, ref List<Type> list, ref int iterator) {
            pb.gameObject.AddComponent(list[iterator]);
            list.Remove(list[iterator--]);
        }

        public void Load() {
            Time.fixedDeltaTime = 1f / 144;
            FindObjectOfType<EnvHandler>().Enable();

            // Remove colliders on everything but actual world collision
            foreach (var col in GameObject.Find("Actual World").GetComponentsInChildren<Collider>())
                if (col.GetComponent<CollideComponent>() == null)
                    Destroy(col);

            // Find Waypoint graphs
            foreach (Transform gr in controller.graphManager.transform.GetChild(0)) {
                gr.gameObject.AddComponent<WaypointGraph>();
            }

            // Apply perso scripts with (Name > Model > Family) priority
            foreach (var pb in FindObjectsOfType<PersoBehaviour>()) {
                Type matchName = null, matchModel = null, matchFamily = null;
                foreach (var s in persoScripts) {
                    string name = s.Name.ToLowerInvariant();
                    if (name == pb.perso.namePerso.ToLowerInvariant()) matchName = s;
                    if (name == pb.perso.nameModel.ToLowerInvariant()) matchModel = s;
                    if (name == pb.perso.nameFamily.ToLowerInvariant()) matchFamily = s;
                }
                if (matchName != null) persos.Add((PersoController)pb.gameObject.AddComponent(matchName));
                else if (matchModel != null) persos.Add((PersoController)pb.gameObject.AddComponent(matchModel));
                else if (matchFamily != null) persos.Add((PersoController)pb.gameObject.AddComponent(matchFamily));
            }

            // Find the player Rayman perso and set as Main Actor
            if (mainActor == null)
                SetMainActor(rayman = PersoController.GetPerso<YLT_RaymanModel>());


                onLoad.Invoke(this, EventArgs.Empty);
            loaded = true;

            if (emptyLevel)
                ClearLevel();
        }


        public static bool anyCollision;
        public void ClearLevel() {
            GameObject ray = null, cam = null;
            foreach(Transform t in GameObject.Find("Dynamic World").transform) {
                if (t.name.Contains("YLT_RaymanModel"))
                    ray = t.gameObject;
                else if (t.name.Contains("StdCam"))
                    cam = t.gameObject;
                else
                    Destroy(t.gameObject);
            }
            GameObject.Find("Father Sector").SetActive(false);
            anyCollision = true;
        }
    }
}