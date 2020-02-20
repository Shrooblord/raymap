//================================
//  By: Adsolution
//================================

using UnityEngine;

namespace CustomGame
{
    public class Shadow : MonoBehaviour
    {
        public float size = 0.7f;
        public float fadeDistance = 8;
        Transform caster;
        MeshRenderer mr;

        void Start()
        {
            mr = GetComponent<MeshRenderer>();
            mr.enabled = false;
            caster = transform.parent;
        }

        void Update()
        {
            var c = RayCollider.Raycast(caster.position + Vector3.up * 0.25f, Vector3.down, fadeDistance);
            if (c.AnyGround)
            {
                mr.enabled = true;
                transform.position = c.hit.point + c.hit.normal * 0.0375f;
                transform.LookAt(c.hit.point + c.hit.normal);
                transform.rotation *= Quaternion.Euler(180, 0, 0);
                transform.localScale = Vector3.one * size - Vector3.one * size * c.hit.distance / fadeDistance;
            }
            else
                mr.enabled = false;
        }
    }
}