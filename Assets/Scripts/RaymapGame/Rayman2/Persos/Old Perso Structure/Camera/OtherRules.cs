using UnityEngine;
using static UnityEngine.Input;
using static RaymapGame.InputEx;

namespace RaymapGame.Rayman2.Persos {
    public partial class StdCam {
        protected void Rule_FocusColumn(Vector3 focus) {
            var focXZ = new Vector3(focus.x, 0, focus.z);
            var targXZ = new Vector3(targ.pos.x, 0, targ.pos.z);

            var vec = (targXZ - focXZ);
            vec.Normalize();
            pos = Vector3.Lerp(pos, targ.pos + vec * 10 + Vector3.up * 3, 6 * dt);
            rot = Quaternion.Slerp(rot, Matrix4x4.LookAt(pos, targ.pos, Vector3.up).rotation, dt * 7);
        }


        Vector3 freeRot;
        protected void Rule_Free() {
            if (newRule) freeRot = rot.eulerAngles;
            SetFriction(1, 1);

            // Look
            if (GetMouseButton(1)) {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = false;

                var add = mouseDelta * 0.175f;
                freeRot = new Vector3(Mathf.Clamp(freeRot.x - add.y, -90, 90), freeRot.y + add.x, 0);
            }
            else {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }


            // Movement
            if (lStick_s.magnitude > deadZone) { 
                float sprint = 1;
                if (GetKey(KeyCode.LeftShift)) sprint = 4;
                else if (GetKey(KeyCode.LeftControl)) sprint = 0.25f;

                moveSpeed = 45 * sprint * dt;

                NavDirectionCam(lStick3D_s);
            }

            rot.eulerAngles = freeRot;
        }
    }
}