using System.Collections.Generic;
using System.Linq;
using OpenSpace.Object.Properties;
using UnityEngine;
using CustomGame;

public class AnimHandler : MonoBehaviour
{
    PersoBehaviour perso;

    Dictionary<int, float> prioCache = new Dictionary<int, float>();
    List<State> nextStates = new List<State>();

    public int currAnim => perso != null ? perso.state.index : 0;

    public float currPriority { get {
            if (prioCache.ContainsKey(currAnim)) return prioCache[currAnim]; else return 0;
        }
    }

    public void SetSpeed(float speed)
    { if (perso != null) perso.animationSpeed = speed; }

    public bool IsSet(int anim)
        => perso.state.index == anim;

    public void Set(int anim) => Set(anim, currPriority);
    public void Set(int anim, float priority, float speed)
    {
        SetSpeed(speed);
        Set(anim, priority);
    }
    public void Set(int anim, float priority)
    {
        if (perso == null || anim == currAnim || priority < currPriority)
            return;

        foreach (var ns in nextStates)
            if (ns.index == anim)
                return;

        if (!prioCache.ContainsKey(anim))
            prioCache.Add(anim, priority);
        perso.autoNextState = true;
        perso.SetState(anim);
        var next = perso.state;

        nextStates.Clear();
        for (int i = 0; i < 1; i++)
        {
            if (next != null)
            {
                nextStates.Add(next);
                next = State.FromOffset(next.NextEntry);
            }
        }
    }


    // SOUND
    public AnimSFX[] sfx;
    void Awake()
    {
        perso = gameObject.GetComponent<PersoBehaviour>();
    }
    void Start()
    {
        if (sfx == null) return;
        for (int i = 0; i < sfx.Length; i++)
            if (sfx[i].player == null)
                sfx[i].player = SFXPlayer.CreateOn(this, sfx[i].info);
        ok = true;
    }
    bool ok;

    uint lastFrame;
    void FixedUpdate()
    {
        if (!ok) return;
        if (perso.currentFrame <= 2)
            lastFrame = 0;
        foreach (var s in sfx)
            if (s.anim == currAnim)
            {
                foreach (var f in s.frames)
                    if (f >= lastFrame && f <= perso.currentFrame)
                        s.player.Play();
            }
            else if (s.player.polyphony != SFXPlayer.Polyphony.Poly)
                s.player.Stop();
        lastFrame = perso.currentFrame;
    }
}