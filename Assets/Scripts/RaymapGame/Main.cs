//================================
//  By: Adsolution
//================================

using System;
using RaymapGame.Rayman2.Persos;
using System.Linq;
using UnityEngine;

namespace RaymapGame
{
    public class Main : MonoBehaviour {
        public PersoController _mainActor; // Inspector display only
        public static bool useFixedTimeWithInterpolation = true;
        public static Main main;
        public bool alwaysControlRayman;
        public static PersoController mainActor;
        public static YLT_RaymanModel rayman;
        public static Type[] persoScripts;
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
                            where t.IsClass && t.Namespace == $"RaymapGame.{gameName}.Persos"
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

            // Debug/cheat stuff
            if (Input.GetKeyDown(KeyCode.D))
                showMainActorDebug = !showMainActorDebug;

            if (Input.GetKeyDown(KeyCode.H) && mainActor is Rayman2.Persos.YLT_RaymanModel ray)
                ray.hasSuperHelic = !ray.hasSuperHelic;
        }


        public void Load() {
            Time.fixedDeltaTime = 1f / 144;
            FindObjectOfType<EnvHandler>().Enable();
            
            // Remove colliders on everything but actual world collision
            foreach (var col in FindObjectsOfType<Collider>())
                if (col.GetComponent<CollideComponent>() == null)
                    Destroy(col);

            // Find Waypoint graphs
            foreach (Transform gr in controller.graphManager.transform.GetChild(0)) {
                gr.gameObject.AddComponent<WaypointGraph>();
            }

            // Apply perso scripts
            foreach (var pb in FindObjectsOfType<PersoBehaviour>()) {
                foreach (var scr in persoScripts) {
                    var con = pb.gameObject;
                    //var con = new GameObject(scr.Name);
                    //pb.transform.parent = con.transform;
                    if (scr.Name.ToLowerInvariant() == pb.perso.namePerso.ToLowerInvariant())
                        con.AddComponent(scr);
                    else if (scr.Name.ToLowerInvariant() == pb.perso.nameModel.ToLowerInvariant())
                        con.AddComponent(scr);
                    else if (scr.Name.ToLowerInvariant() == pb.perso.nameFamily.ToLowerInvariant())
                        con.AddComponent(scr);
                }
            }

            // Find the player Rayman perso and set as Main Actor
            if (mainActor == null)
                SetMainActor(rayman = (YLT_RaymanModel)PersoController.GetPersoName("Rayman"));


            onLoad.Invoke(this, EventArgs.Empty);
            loaded = true;
        }
    }
}