//================================
//  By: Adsolution
//================================

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Camera
    /// </summary>
    public partial class Camera : PersoController {
        public UnityEngine.Camera cam => UnityEngine.Camera.main;
        public override bool interpolate => true;
        public override float activeRadius => 100000;
        public override bool resetOnRayDeath => false;
    }
}