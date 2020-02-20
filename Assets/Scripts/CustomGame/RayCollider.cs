//================================
//  By: Adsolution
//================================

using System;
using System.Collections;
using UnityEngine;
using OpenSpace.Collide;

namespace CustomGame
{
    public class RayCollider
    {
        public MonoBehaviour controller;
        public bool groundEnabled = true;
        public bool wallEnabled = true;
        public float radius = 0.5f;
        public float bottom = 0.5f;
        public float top = 1;
        public float groundDepth = 0.25f;
        public float ceilingHeight = 0.75f;
        public float wallAngle = 45;

        public static float waterShallowDepth = 1.5f;
        public bool waterAutoSurface = true;
        public float waterAutoSurfaceDepth = 4;
        public float waterRestOffset = 1.125f;

        
        public CollideInfo ground, groundFar, wall, ceiling, water;
        public bool atWaterSurface => water.hit.distance > 0 && water.hit.distance < 1.5f + waterRestOffset;
        public bool waterIsShallow => _waterIsShallow;
        bool _waterIsShallow;


        public PersoController perso => controller as PersoController;
        public Vector3 pos => perso != null ? perso.pos : controller.transform.position;


        //========================================
        //  Continuous Raycasting
        //========================================
        public static int raycastDepth = 5;
        public static CollideInfo Raycast(Vector3 origin, Vector3 direction, float distance, CollideMaterialType types = CollideMaterialType.Any) {
            bool hit = Physics.Raycast(origin, direction, out var newhit, distance, 1 << 9, QueryTriggerInteraction.Ignore);
            /*
            RaycastHit newhit = new RaycastHit();
            int casts = 0;
            for (float dist = 0; dist < distance && casts < raycastDepth; dist += newhit.distance, casts++) {
                bool hit = Physics.Raycast(casts == 0 ? origin : newhit.point, direction, out newhit, distance - dist, 1 << 9, QueryTriggerInteraction.Ignore);
                if (hit) {
                    var colComp = newhit.collider.GetComponent<CollideComponent>();
                    if (colComp != null) {
                        if (((CollideMaterialType)colComp.col.type & types) == types)
                            return new CollideInfo(newhit, colComp.collide);
                    }
                } else break;
            }*/
            return new CollideInfo(newhit);
        }

        public CollideInfo RaycastGround(CollideMaterialType types = CollideMaterialType.Any)
            => Raycast(pos + Vector3.up * 1, Vector3.down, 1 + groundDepth, types);
        public CollideInfo RaycastCeiling(CollideMaterialType types = CollideMaterialType.Any)
            => Raycast(pos + Vector3.up * top, Vector3.up, ceilingHeight, types);



        //========================================
        //  Collision Actions
        //========================================
        public void BlockCollision(float seconds) => t_blockCollision.Start(seconds);
        Timer t_blockCollision = new Timer();

        public void UpdateCollision()
        {
            UpdateGroundCollision();
            UpdateWallCollision();
        }

        public void UpdateGroundCollision()
        {
            var p = pos;
            if (groundEnabled) {
                ground = RaycastGround();
                groundFar = Raycast(p + Vector3.up * 1, Vector3.down, 1 + 10);
            }
            else {
                ground = new CollideInfo();
                groundFar = new CollideInfo();
            }
        }
        
        public void StickToGround() => StickToGround(ref perso.pos);
        public void StickToGround(ref Vector3 pos)
        {
            pos.y = ground.hit.point.y;
        }

        Vector3 wallPush, ceilPush;
        public void UpdateWallCollision()
        {
            wallPush = new Vector3();
            wall = new CollideInfo();
            var most = new Vector3();
            RaycastHit shortest = new RaycastHit { distance = radius };
            for (float h = bottom; h <= top; h += 0.25f)
                for (float a = 0; a < Mathf.PI * 2; a += Mathf.PI * 2 / 16)
                {
                    var col = Raycast(pos + h * Vector3.up, new Vector3(Mathf.Sin(a), 0, Mathf.Cos(a)), radius);
                    if (col.AnyWall && col.hit.distance < shortest.distance)
                    {
                        most = col.hit.normal * (-col.hit.distance + radius);
                        shortest = col.hit;
                        wall = col;
                    }
                }
            wallPush = new Vector3(most.x, most.y, most.z);

            ceilPush = new Vector3();
            ceiling = RaycastCeiling();
            if (ceiling.AnyWall)
                ceilPush = ceiling.hit.normal * (-ceiling.hit.distance + ceilingHeight);
        }

        public void ApplyWallCollision() => ApplyWallCollision(ref perso.pos);
        public void ApplyWallCollision(ref Vector3 pos)
        {
            pos += wallEnabled ? wallPush : Vector3.zero
                + ceilPush;
        }


        public void UpdateWaterCollision()
        {
            water = Raycast(pos, Vector3.up, 4);
            //var w = waterInfo.hit;
            /*
            _waterIsShallow = w.distance < waterShallowDepth
                && Raycast(w.point + Vector3.down * 0.1f, Vector3.down)
                && GetCollision(shalGround, waterShallowDepth - 0.1f).AnyGround;*/
        }


        public void ApplyWaterCollision() => ApplyWaterCollision(ref perso.pos, ref perso.velY);
        public void ApplyWaterCollision(ref Vector3 pos)
        {
            if (water.hit.distance < 1 + waterRestOffset)
                pos.y = water.hit.point.y - waterRestOffset;
        }
        public void ApplyWaterCollision(ref Vector3 pos, ref float velY)
        {
            ApplyWaterCollision(ref pos);
            if (waterAutoSurface && water.hit.distance < waterAutoSurfaceDepth)
                velY += 4 * (controller is IInterpolate ? Time.fixedDeltaTime : Time.deltaTime);
        }
    }
}