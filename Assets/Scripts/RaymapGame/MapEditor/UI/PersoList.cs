//================================
//  By: Adsolution
//================================

using UnityEngine;
using UnityEngine.UI;

namespace RaymapGame.EditorUI {
    public class PersoList : MonoBehaviour {
        public VerticalLayoutGroup list;
        public Color colorGeneric, colorMain, colorController, colorControllerDot;


        public void Load() {
            foreach (var p in FindObjectsOfType<PersoBehaviour>()) {
                ResManager.Inst<PersoListItem>("MapEditor/UI/PersoListItem", list).perso = p;
            }
        }
    }
}