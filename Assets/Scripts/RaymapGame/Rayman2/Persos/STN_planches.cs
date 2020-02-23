//================================
//  By: Adsolution
//================================
using System.Collections.Generic;
using UnityEngine;
namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Planks
    /// </summary>
    public partial class STN_planches : PersoController {
        public override AnimSFX[] animSfx => new AnimSFX[] { };

        int type;
        // 0, 950, 3500, 3501, 5000

        void OnGUI() {
            if (StoodOnByPerso(rayman))
                GUILayout.Label(type.ToString());
        }

        protected override void OnStart() {
            type = GetDsgVar<int>("Int_4");
            rvel = 0;
            SetRule("Wait");
        }

        protected void Rule_Wait() {
            switch (type) {
                case 950: break;
                default:
                    if (StoodOnByPerso(rayman))
                        SetRule("Shake"); break;
            }
        }

        float rvel;
        Timer t_fall = new Timer();
        void Rule_Shake() {
            rot = Quaternion.Euler(new Vector3(Random.value * 7 * Mathf.Sin(rvel += dt * 50 * Random.value), 0, 0)
                + startRot.eulerAngles);

            if (newRule)
                t_fall.Start(3.5f, () => SetRule("Fall"), false);
        }

        Quaternion fallRVel;
        void Rule_Fall() {
            if (newRule) fallRVel = Random.rotation;

            ApplyGravity();
            rot.eulerAngles += fallRVel.eulerAngles * dt / 2;
        }
    }
}