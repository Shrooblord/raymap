//================================
//  By: Adsolution
//================================
using RaymapGame.Rayman2.Persos;

namespace RaymapGame {
    public partial class PersoController {
        //========================================
        //  Static Functions
        //========================================
        public static YLT_RaymanModel rayman => Main.rayman;

        public static PersoSubType GerPerso<PersoSubType>()
            where PersoSubType : PersoController
            => FindObjectOfType<PersoSubType>();

        public static PersoController GetPerso(string perso)
            => FindObjectOfType(System.Type.GetType(perso)) as PersoController;
    }
}
