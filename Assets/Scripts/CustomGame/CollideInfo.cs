﻿using UnityEngine;
using OpenSpace.Collide;

namespace CustomGame {
    public enum CollideMaterialType : ushort {
        Any = ushort.MaxValue,
        Generic = 0,
        Slide = 1 << 0,
        Trampoline = 1 << 1,
        GrabbableLedge = 1 << 2,
        Wall = 1 << 3,
        HangableCeiling = 1 << 5,
        ClimbableWall = 1 << 6,
        Electric = 1 << 7,
        LavaDeathWarp = 1 << 8,
        FallTrigger = 1 << 9,
        HurtTrigger = 1 << 10,
        DeathWarp = 1 << 11,
        Water = 1 << 14,
        NoCollision = 1 << 15,
    }

    public struct CollideInfo {
        public GeometricObjectElementCollideTriangles collide;
        public RaycastHit hit;

        public CollideMaterial mat => collide.gameMaterial.collideMaterial;
        public bool isValid;
        public CollideType collideType => collide.geo.type;
        public CollideMaterialType type {
            get => (CollideMaterialType)mat.type;
            set { mat.type = (ushort)type; }
        }

        public CollideInfo(RaycastHit hit) { 
            var c = hit.collider?.GetComponent<CollideComponent>();
            if (c != null) {
                isValid = true;
                collide = c.collide;
                this.hit = hit;
            }
            else {
                isValid = false;
                collide = null;
                this.hit = new RaycastHit();
            }
        }
        public CollideInfo(RaycastHit hit, GeometricObjectElementCollideTriangles collide) {
            isValid = true;
            this.hit = hit;
            this.collide = collide;
        }


        bool Checks => isValid && collide?.gameMaterial?.collideMaterial != null;
        public bool None => !isValid;
        public bool Any => isValid;
        public bool Generic => isValid && collide?.gameMaterial?.collideMaterial == null;
        public bool AnyGround => Generic || GrabbableLedge || Trampoline;
        public bool AnyWall => Generic || GrabbableLedge || Slide || Trampoline || Wall || ClimbableWall;

        public bool Slide => Checks && mat.Slide;
        public bool Trampoline => Checks && mat.Trampoline;
        public bool GrabbableLedge => Checks && mat.GrabbableLedge;
        public bool Wall => Checks && mat.Wall;
        public bool FlagUnknown => Checks && mat.FlagUnknown;
        public bool HangableCeiling => Checks && mat.HangableCeiling;
        public bool ClimbableWall => Checks && mat.ClimbableWall;
        public bool Electric => Checks && mat.Electric;
        public bool LavaDeathWarp => Checks && mat.LavaDeathWarp;
        public bool FallTrigger => Checks && mat.FallTrigger;
        public bool HurtTrigger => Checks && mat.HurtTrigger;
        public bool DeathWarp => Checks && mat.DeathWarp;
        public bool FlagUnk2 => Checks && mat.FlagUnk2;
        public bool FlagUnk3 => Checks && mat.FlagUnk3;
        public bool Water => Checks && mat.Water;
        public bool NoCollision => Checks && mat.NoCollision;
    }
}