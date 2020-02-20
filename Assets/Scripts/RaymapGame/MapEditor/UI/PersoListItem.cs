//================================
//  By: Adsolution
//================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace RaymapGame.EditorUI {
    public class PersoListItem : MonoBehaviour, IPointerDownHandler {
        public PersoBehaviour perso;
        public PersoController persoCtrl;
        public Text persoName;
        public Image bg;
        public Image dot;
        PersoList list;

        public void OnPointerDown(PointerEventData eventData) {
            if (persoCtrl != null)
                Main.SetMainActor(persoCtrl);
        }

        void Start() {
            list = GetComponentInParent<PersoList>();
            persoCtrl = perso.GetComponent<PersoController>();

            persoName.text = perso.perso.namePerso;
        }

        void Update() {
            if (perso == Main.mainActor.perso)
                dot.color = list.colorControllerDot;
            else dot.color = list.colorGeneric;

            if (persoCtrl != null)
                bg.color = list.colorController;
            else bg.color = list.colorGeneric;
        }
    }
}
