using UnityEngine;
using OpenSpace.Collide;

[ExecuteInEditMode]
public class CollideComponent : MonoBehaviour {
    public GeometricObjectElementCollideTriangles collide;
    public CollideType type;
    public bool
        Slide, Trampoline, GrabbableLedge, Wall,
        FlagUnknown, HangableCeiling, ClimbableWall,
        Electric, LavaDeathWarp, FallTrigger, HurtTrigger,
        DeathWarp, FlagUnk2, FlagUnk3, Water, NoCollision;

    public CollideMaterial col => collide.gameMaterial.collideMaterial;
    public bool? updateReady = false;

    void Start() {
        if (col == null) return;
        type = collide.geo.type;
        Slide = col.Slide;
        Trampoline = col.Trampoline;
        GrabbableLedge = col.GrabbableLedge;
        Wall = col.Wall;
        FlagUnknown = col.FlagUnknown;
        HangableCeiling = col.HangableCeiling;
        ClimbableWall = col.ClimbableWall;
        Electric = col.Electric;
        LavaDeathWarp = col.LavaDeathWarp;
        FallTrigger = col.FallTrigger;
        HurtTrigger = col.HurtTrigger;
        DeathWarp = col.DeathWarp;
        FlagUnk2 = col.FlagUnk2;
        FlagUnk3 = col.FlagUnk3;
        Water = col.Water;
        NoCollision = col.NoCollision;
    }

    void Update() {
        if (col == null || !(bool)updateReady) return;
        updateReady = false;

        if (type != collide.geo.type) collide.geo.type = type;
        if (Slide != col.Slide) col.Slide = Slide;
        if (Trampoline != col.Trampoline) col.Trampoline = Trampoline;
        if (GrabbableLedge != col.GrabbableLedge) col.GrabbableLedge = GrabbableLedge;
        if (Wall != col.Wall) col.Wall = Wall;
        if (FlagUnknown != col.FlagUnknown) col.FlagUnknown = FlagUnknown;
        if (HangableCeiling != col.HangableCeiling) col.HangableCeiling = HangableCeiling;
        if (ClimbableWall != col.ClimbableWall) col.ClimbableWall = ClimbableWall;
        if (Electric != col.Electric) col.Electric = Electric;
        if (LavaDeathWarp != col.LavaDeathWarp) col.LavaDeathWarp = LavaDeathWarp;
        if (FallTrigger != col.FallTrigger) col.FallTrigger = FallTrigger;
        if (HurtTrigger != col.HurtTrigger) col.HurtTrigger = HurtTrigger;
        if (DeathWarp != col.DeathWarp) col.DeathWarp = DeathWarp;
        if (FlagUnk2 != col.FlagUnk2) col.FlagUnk2 = FlagUnk2;
        if (FlagUnk3 != col.FlagUnk3) col.FlagUnk3 = FlagUnk3;
        if (Water != col.Water) col.Water = Water;
        if (NoCollision != col.NoCollision) col.NoCollision = NoCollision;
    }
}