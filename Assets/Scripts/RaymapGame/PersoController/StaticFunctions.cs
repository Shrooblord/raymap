//================================
//  By: Adsolution
//================================
using RaymapGame.Rayman2.Persos;
using System;

namespace RaymapGame {
    public partial class PersoController {
        //========================================
        //  Static Functions
        //========================================
        public static YLT_RaymanModel rayman => Main.rayman;

        public static PersoSubType GetPerso<PersoSubType>()
            where PersoSubType : PersoController
            => FindObjectOfType<PersoSubType>();

        public static PersoController GetPerso(string perso) {
            var t = Type.GetType($"{nameof(RaymapGame)}.{Main.gameName}.Persos.{perso}");
            if (t != null) return FindObjectOfType(t) as PersoController;
            return null;
        }
    }
}