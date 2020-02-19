using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CustomGame.EditorUI {
    public class PersoList : MonoBehaviour {
        public VerticalLayoutGroup list;
        public Color colorGeneric, colorMain, colorController, colorControllerDot;

        void Start() {
            //Main.onLoad += Main_onLoad;
        }
        void Main_onLoad(object sender, System.EventArgs e) {
            Load();
        }

        bool loaded;
        void Update() {
            if (!loaded) {
                loaded = true;
                Load();
            }
        }

        public void Load() {
            foreach (var p in FindObjectsOfType<PersoBehaviour>()) {
                ResManager.Inst<PersoListItem>("MapEditor/UI/PersoListItem", list).perso = p;
            }
        }
    }
}
