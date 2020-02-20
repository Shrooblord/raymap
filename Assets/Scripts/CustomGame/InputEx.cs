//================================
//  By: Adsolution
//================================

using UnityEngine;
using static UnityEngine.Input;

namespace CustomGame
{
    public class InputEx : MonoBehaviour
    {
        public static float deadZone = 0.15f;
        public static float smoothing = 20;

        public static Vector2 lStick => new Vector2(GetAxisRaw("Horizontal"), GetAxisRaw("Vertical"));
        public static Vector2 rStick => new Vector2(GetAxisRaw("RHorizontal"), GetAxisRaw("RVertical"));
        public static float rStickAngle => Mathf.Atan2(rStick.x, rStick.y) * Mathf.Rad2Deg;
        public static Vector2 lStick_s => _lStick_s;
        static Vector2 _lStick_s;
        public static Vector2 rStick_s => _rStick_s;
        static Vector2 _rStick_s;
        public static Vector3 lStick3D => new Vector3(GetAxisRaw("Horizontal"), 0, GetAxisRaw("Vertical"));
        public static Vector3 rStick3D => new Vector3(GetAxisRaw("RHorizontal"), 0, GetAxisRaw("RVertical"));
        public static Vector3 lStick3D_s => new Vector3(lStick_s.x, 0, lStick_s.y);
        public static Vector3 rStick3D_s => new Vector3(rStick_s.x, 0, rStick_s.y);

        public static float lStickAngle => Mathf.Atan2(-lStick.x, lStick.y) * Mathf.Rad2Deg;
        public static float lStickAngleCam
            => Camera.main.transform.rotation.eulerAngles.y
            + Mathf.Atan2(-lStick_s.x, -lStick_s.y) * Mathf.Rad2Deg;
        public static Vector2 lStickCam_s
            => Matrix4x4.Rotate(Camera.main.transform.rotation).MultiplyVector(lStick3D_s);


        public static Vector2 mouseDelta;
        static Vector2 mousePosPrev;

        public static bool
            iJumpDown, iJumpHold, iJumpUp,
            iShootDown, iShootHold, iShootUp;

        void Update()
        {
            _lStick_s = Vector3.ClampMagnitude(Vector2.Lerp(_lStick_s, lStick, Time.deltaTime * smoothing), 1);
            _rStick_s = Vector3.ClampMagnitude(Vector2.Lerp(_rStick_s, rStick, Time.deltaTime * smoothing / 2), 1);
            mouseDelta = (Vector2)mousePosition - mousePosPrev;
            mousePosPrev = mousePosition;


            iJumpDown = GetKeyDown(KeyCode.A) || GetKeyDown(KeyCode.JoystickButton0);
            iJumpHold = GetKey(KeyCode.A) || GetKey(KeyCode.JoystickButton0);
            iJumpUp = GetKeyUp(KeyCode.A) || GetKeyUp(KeyCode.JoystickButton0);

            iShootDown = GetKeyDown(KeyCode.Space) || GetKeyDown(KeyCode.JoystickButton1);
            iShootHold = GetKey(KeyCode.Space) || GetKey(KeyCode.JoystickButton1);
            iShootUp = GetKeyUp(KeyCode.Space) || GetKeyUp(KeyCode.JoystickButton1);
        }
    }
}