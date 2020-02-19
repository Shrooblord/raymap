using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace CustomGame
{
    public class Main : MonoBehaviour {
        public PersoController _mainActor; // Inspector display only
        public static bool useFixedTimeWithInterpolation = true;
        public static Main main;
        public static PersoController mainActor;
        public static Type[] persoScripts;
        public static Rayman2.Persos.StdCam cam;
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


        void Awake() {
            main = this;
            persoScripts = (from t in System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.Namespace == $"CustomGame.{gameName}.Persos"
                    select t).ToArray();

            controller = FindObjectOfType<Controller>();
            music = GetComponent<AudioSource>();
            env = gameObject.AddComponent<EnvHandler>();
        }

        void Start() {
            Time.fixedDeltaTime = 1f / 144;
            Timer.StartNew(0.5f, () => StartCoroutine(Load()));
            onLoad += Main_onLoad;
        }


        IEnumerator Load() {
            FindObjectOfType<EnvHandler>().Enable();

            while (!canLoad) yield return new WaitForEndOfFrame();

            while (canLoad) {

                // Remove colliders on everything but actual world collision
                foreach (var col in FindObjectsOfType<Collider>())
                    if (col.GetComponent<CollideComponent>() == null)
                        Destroy(col);


                // Apply perso scripts
                foreach (var pb in FindObjectsOfType<PersoBehaviour>())
                    if (pb.GetComponent<PersoController>() == null)
                        foreach (var scr in persoScripts) {
                            if (scr.Name.ToLowerInvariant() == pb.perso.namePerso.ToLowerInvariant())
                                pb.gameObject.AddComponent(scr);
                            else if (scr.Name.ToLowerInvariant() == pb.perso.nameModel.ToLowerInvariant())
                                pb.gameObject.AddComponent(scr);
                            else if (scr.Name.ToLowerInvariant() == pb.perso.nameFamily.ToLowerInvariant())
                                pb.gameObject.AddComponent(scr);
                        }


                // Find the player Rayman perso and set as Main Actor
                if (mainActor == null)
                    SetMainActor((Rayman2.Persos.YLT_RaymanModel)PersoController.GetPersoName("Rayman"));


                if (!loaded) Timer.StartNew(2, () => onLoad.Invoke(this, EventArgs.Empty));
                loaded = true;
                yield return new WaitForSeconds(0.5f);
            }
            yield return null;
        }


        // Debug/cheat stuff
        void Update() { 
            if (Input.GetKeyDown(KeyCode.D))
                showMainActorDebug = !showMainActorDebug;

            if (Input.GetKeyDown(KeyCode.H) && mainActor is Rayman2.Persos.YLT_RaymanModel ray)
                ray.hasSuperHelic = !ray.hasSuperHelic;
        }
    }
}
