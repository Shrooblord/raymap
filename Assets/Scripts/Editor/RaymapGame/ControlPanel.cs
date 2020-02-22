using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace RaymapGame {
    public class ControlPanel : EditorWindow {
        [MenuItem("Raymap/RaymapGame/Control Panel")]
        public static void ShowWindow() {
            GetWindow(typeof(ControlPanel));
        }

        public static void CreateNewPersoScript(string persoNameOrModelOrFamily, string englishName, string author = "") {
            string newPath = $"Assets/Scripts/RaymapGame/Rayman2/Persos/{persoNameOrModelOrFamily}.cs";
            if (File.Exists(newPath)) {
                t_error.Start(3);
                return;
            }
            var scr = new StreamReader("Assets/Scripts/RaymapGame/PersoEditor/NewPersoScript.txt");
            var outs = scr.ReadToEnd()
                .Replace("Author", author)
                .Replace("NewPersoScript", persoNameOrModelOrFamily)
                .Replace("Description", englishName).Split(new string[] { "~~~" }, System.StringSplitOptions.RemoveEmptyEntries);

            var newScr = new StreamWriter(newPath);
            if (author != "") newScr.Write(outs[0]);
            newScr.Write(outs[1]);

            scr.Close();
            newScr.Close();
        }


        static Timer t_error = new Timer();
        static float nameWidth = 160;

        public static PersoBehaviour GetSelectedPersoBehaviour() {
            return Selection.activeGameObject.GetComponentInParent<PersoBehaviour>();
        }


        void Header(string text) {
            GUILayout.BeginHorizontal();
            GUILayout.Label(text, EditorStyles.toolbarButton);
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        Timer t_scan = new Timer();
        static PersoController[] persos = new PersoController[0];
        public void RescanLevel() {
            persos = FindObjectsOfType<PersoController>();
        }

        void FixedUpdate() {
            t_scan.Start(1.5f, RescanLevel, false);
        }


        static string englishName = "";

        public void OnGUI() {
            Header("Create Perso Script");

            Main.GetPersoScripts();
            GUILayout.BeginHorizontal();
            GUILayout.Label("English name:", GUILayout.Width(nameWidth));
            englishName = GUILayout.TextField(englishName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Create from Selected Perso", GUILayout.Width(nameWidth));
            float bw = 50;
            if (GUILayout.Button("Name"))
                CreateNewPersoScript(GetSelectedPersoBehaviour().perso.namePerso, englishName);
            if (GUILayout.Button("Model"))
                CreateNewPersoScript(GetSelectedPersoBehaviour().perso.nameModel, englishName);
            if (GUILayout.Button("Family"))
                CreateNewPersoScript(GetSelectedPersoBehaviour().perso.nameFamily, englishName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(nameWidth);
            if (t_error.active) {
                GUILayout.Label("Perso script with this name already exists");
            }
            GUILayout.EndHorizontal();

            Header("Perso Scripts");

            foreach (var s in Main.persoScripts) {
                EditorGUILayout.BeginToggleGroup(s.Name, true);
                foreach (var p in persos) {
                    if (p.GetType() == s) {
                        EditorGUILayout.ObjectField(p, typeof(PersoController), true);
                    }
                }
                EditorGUILayout.EndToggleGroup();
            }
        }
    }
}
