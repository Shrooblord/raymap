using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    public class JCP_FRH_sbire_gnak_I1 : FRH_sbire_gnak {
        public override float activeRadius => 9999f;

        protected override void OnStart() {
            base.OnStart();

            //**** DELET THIS ****

            Main.SetMainActor(this);
            Main.showMainActorDebug = true;

            //**** END OF DELET ****

            pos = new Vector3(-193.61f, 23.84f, 369.45f);
            rot = Quaternion.Euler(0, 0, 0);

            //Colour the Henchman. 1 = Red; 2 = Purple
            GetComponent<PersoBehaviour>().poListIndex = 2;

            SetRule("Sleeping");
        }
    }
}
