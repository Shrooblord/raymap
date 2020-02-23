using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    public partial class StdCam {
        void Rule_FocusColumn(Vector3 focus) {
            var focXZ = new Vector3(focus.x, 0, focus.z);
            var targXZ = new Vector3(targ.pos.x, 0, targ.pos.z);

            var vec = (targXZ - focXZ);
            vec.Normalize();
            pos = Vector3.Lerp(pos, targ.pos + vec * 10 + Vector3.up * 3, 6 * dt);
            rot = Quaternion.Slerp(rot, Matrix4x4.LookAt(pos, targ.pos, Vector3.up).rotation, dt * 7);
        }
    }
}