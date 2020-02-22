//================================
//  By: Adsolution
//================================

using UnityEngine;
using UnityEngine.UI;

namespace RaymapGame.EditorUI {
    public class PersoList : MonoBehaviour {
        public VerticalLayoutGroup list;
        public Color colorGeneric, colorMain, colorController, colorControllerDot;

        void Start() => Main.onLoad += Main_onLoad;
        void Main_onLoad(object sender, System.EventArgs e) {
            Load();
        }

        public void Load() {
            foreach (var p in FindObjectsOfType<PersoBehaviour>()) {
                ResManager.Inst<PersoListItem>("MapEditor/UI/PersoListItem", list).perso = p;
            }
        }
    }
}