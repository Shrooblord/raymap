//================================
//  By: Adsolution
//================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RaymapGame.EditorUI {
    public class MapEditor : MonoBehaviour {
        public PersoList persoList;
        void Start() {
            persoList.gameObject.SetActive(false);
        }

        void Update() {
            if (Input.GetKeyDown(KeyCode.E))
                persoList.gameObject.SetActive(!persoList.gameObject.activeSelf);

            if (Input.GetMouseButtonDown(0)) {
                Camera.main.ScreenPointToRay(Input.mousePosition);
            }
        }
    }
}
