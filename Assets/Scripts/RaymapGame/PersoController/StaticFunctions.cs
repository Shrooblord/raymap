//================================
//  By: Adsolution
//================================
using RaymapGame.Rayman2.Persos;

namespace RaymapGame {
    public partial class PersoController {
        //========================================
        //  Static Functions
        //========================================
        public static YLT_RaymanModel GetRayman() {
            return GetPersoModel("YLT_RaymanModel") as YLT_RaymanModel;
        }
        public static YLT_RaymanModel rayman => Main.rayman;

        public static PersoController GetPersoFamily(string persoName)
            => GetPersoFamily<PersoController>(persoName);
        public static PersoSubType GetPersoFamily<PersoSubType>(string persoType)
            where PersoSubType : PersoController {
            foreach (var perso in FindObjectsOfType<PersoSubType>()) {
                if (perso.perso.perso.nameFamily.ToLowerInvariant() == persoType.ToLowerInvariant())
                    return perso;
            }
            return null;
        }

        public static PersoController GetPersoModel(string persoName)
            => GetPersoModel<PersoController>(persoName);
        public static PersoSubType GetPersoModel<PersoSubType>(string persoModel)
            where PersoSubType : PersoController {
            foreach (var perso in FindObjectsOfType<PersoSubType>()) {
                if (perso.perso.perso.nameModel.ToLowerInvariant() == persoModel.ToLowerInvariant())
                    return perso;
            }
            return null;
        }

        public static PersoController GetPersoName(string persoName)
            => GetPersoName<PersoController>(persoName);
        public static PersoSubType GetPersoName<PersoSubType>(string persoName)
            where PersoSubType : PersoController {
            foreach (var perso in FindObjectsOfType<PersoSubType>()) {
                if (perso.perso.perso.namePerso.ToLowerInvariant() == persoName.ToLowerInvariant())
                    return perso;
            }
            return null;
        }
    }
}
