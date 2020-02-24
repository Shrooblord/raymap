using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomGame.EditorUI {
    public class MapEditor : MonoBehaviour {
        public PersoList persoList;
        void Start() {
            persoList.gameObject.SetActive(false);
        }

        void Update() {
            if (!Main.loaded) return;

            if (Input.GetKeyDown(KeyCode.E))
                persoList.gameObject.SetActive(!persoList.gameObject.activeSelf);
        }
    }
}
