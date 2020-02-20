//================================
//  By: Adsolution
//================================

using UnityEngine;
using UnityEngine.UI;

namespace CustomGame.EditorUI {
    public class PersoList : MonoBehaviour {
        public VerticalLayoutGroup list;
        public Color colorGeneric, colorMain, colorController, colorControllerDot;

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