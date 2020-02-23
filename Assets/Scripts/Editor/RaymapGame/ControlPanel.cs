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

        public static string persoPath => $"Assets/Scripts/RaymapGame/{Main.gameName}/Persos";



        public static void CreatePersoScript(PersoBehaviour pb, string description, string author, string rank) {
            string baseName = "";
            string newName = "";
            string newDir = "";
            switch (rank) {
                case "Family":
                    newName = pb.perso.nameFamily;
                    newDir = $"{persoPath}/{pb.perso.nameFamily}";
                    break;
                case "Model":
                    baseName = pb.perso.nameFamily;
                    newName = pb.perso.nameModel;
                    newDir = $"{persoPath}/{pb.perso.nameFamily}/Models/{pb.perso.nameModel}";
                    break;
                case "Instance":
                    baseName = pb.perso.nameModel;
                    newName = pb.perso.namePerso;
                    newDir = $"{persoPath}/{pb.perso.nameFamily}/Models/{pb.perso.nameModel}/Instances";
                    break;
            }
            
            if (!Directory.Exists(newDir))
                Directory.CreateDirectory(newDir);
            string newPath = $"{newDir}/{newName}.cs";
            if (File.Exists(newPath)) return;

            var scr = new StreamReader($"Assets/Scripts/RaymapGame/PersoEditor/Objects/NewScript_{rank}.txt");
            var outs = scr.ReadToEnd()
                .Replace("Author", author)
                .Replace("NewScript", newName)
                .Replace("DerivedScript", baseName)
                .Replace("Description", description).Split(new string[] { "~~" }, System.StringSplitOptions.RemoveEmptyEntries);

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


        static string author = System.Environment.UserName;
        static string description = "";

        public void OnGUI() {
            Header("Create Perso Script");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Author:", GUILayout.Width(nameWidth));
            author = GUILayout.TextField(author);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("English description:", GUILayout.Width(nameWidth));
            description = GUILayout.TextField(description);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Create from Selected Perso", GUILayout.Width(nameWidth));
            if (GUILayout.Button("Family")) {
                var pb = GetSelectedPersoBehaviour();
                CreatePersoScript(pb, description, author, "Family");
            }
            if (GUILayout.Button("Model")) {
                var pb = GetSelectedPersoBehaviour();
                CreatePersoScript(pb, description, author, "Family");
                CreatePersoScript(pb, description, author, "Model");

            }
            if (GUILayout.Button("Instance")) {
                var pb = GetSelectedPersoBehaviour();
                CreatePersoScript(pb, description, author, "Family");
                CreatePersoScript(pb, description, author, "Model");
                CreatePersoScript(pb, description, author, "Instance");

            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(nameWidth);
            if (t_error.active) {
                GUILayout.Label("Perso script with this name already exists");
            }
            GUILayout.EndHorizontal();




            Header("Perso Scripts");


            GUILayout.BeginHorizontal();
            GUILayout.Label("Live Preview", GUILayout.Width(nameWidth));
            if (GUILayout.Button("Reload Scripts")) {
                Main.GetPersoScripts();
            }
            if (GUILayout.Button("Update Live Scene")) {
                RescanLevel();
            }
            GUILayout.EndHorizontal();



            float ind = 25;
            int columns = Mathf.CeilToInt((Screen.width - ind) / 100);
            float oWidth = Screen.width / columns;

            foreach (var s in Main.persoScripts) {
                int i = 0;
                EditorGUILayout.ToggleLeft(s.Name, true);
                foreach (var p in persos) {
                    if (p.GetType() == s) {
                        if (i == 0) {
                            GUILayout.BeginHorizontal();
                            GUILayout.Space(ind);
                        }
                        EditorGUILayout.ObjectField(p, typeof(PersoController), true, GUILayout.MaxWidth(oWidth));
                        i++;
                    }
                    if (i > 0 && (i == columns || System.Array.IndexOf(persos, p) == persos.Length - 1)) {
                        GUILayout.EndHorizontal();
                        i = 0;
                    }
                }
            }
        }
    }
}
