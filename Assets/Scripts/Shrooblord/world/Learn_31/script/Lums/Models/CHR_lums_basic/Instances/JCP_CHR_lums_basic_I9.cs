//================================
//  By: Shrooblord
//================================
using Shrooblord.lib;
using System.Collections.Generic;
using UnityEngine;

namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Lum Back of the Tree
    /// This Yellow Lum is supposed to elude the player for quite some time, until we introduce either permanent Super Helico powers using some sort of
    ///   "mana bar" approach (so that the game doesn't get broken by you helico-ing out of level bounds)*, or by introducing a new Purple Lum right below it,
    ///   which the player should be able to think back to really soon after you get the Silver Lum from Ly in just a couple of minutes.
    ///   The second idea is what I'll probably end up going for, since this will trigger a process in the mind of the player: "hey; I *just* saw that Purple Lum...
    ///   wait, that was, like, really close. but now it's behind me. Ahh... there's some Castlevania bollocks going on right here."
    /// 
    /// *= What I like about this idea is that we could introduce this new mechanic to make the game significantly harder in for example the Sanctuary of Rock
    ///      and Lava. Imagine the Carmen the Whale bubble trail mechanic, but within that fricking dodge-fest moving in between lava flows. This should be spicy...!!
    ///      But also, to keep this power to further enhance the gameplay experience. When you can suddenly Super Helico everywhere, what changes?
    ///      Everything, right? How about aerial combat? Sure, why not! How about previously super unreachable spots that now you have access to? Absolutely, no problem.
    /// </summary>
    public partial class JCP_CHR_lums_basic_I9 : PersoController {
        #region Should Be in Parent (But Currently Doesn't Work)
        //***PARENT STUFF***//
        Vector3 origPos = Vector3.zero;
        int origPoListIndex;
        protected virtual void ResetToOriginal() {
            pos = origPos;
            GetComponent<PersoBehaviour>().poListIndex = origPoListIndex;
        }

        protected virtual void RecordOriginalStats() {
            origPos = pos;
            origPoListIndex = GetComponent<PersoBehaviour>().poListIndex;
        }
        //END OF PARENT STUFF//
        #endregion

        public Vector3 triggerPos = new Vector3(-10, 0, 0);

        protected override void OnStart() {
            //Keep track of its original place and type, etc.
            RecordOriginalStats();

            //make this a Super Yellow Lum instead of a Red One (we stole it from the waterslide just further ahead)
            anim.Set(13);

            //move this Lum to its new spot
            pos = new Vector3(-192.03f, 75.25f, 370.49f);
            transform.position = pos;

            if (gameObject.GetComponentInChildren<TriggerVolume>() == null) {
                var GO = new GameObject("TRIG_" + name);
                triggerVolume = GO.AddComponent<TriggerVolume>();
                triggerVolume.transform.position = triggerPos; //pos;
                triggerVolume.transform.parent = transform;
            }

            //This hot-swapping of assets is in general how I think I'll be able to """"spawn""" new objects, in lieu of *actually* having that ability, which still
            //  is in an unknown spot on the horizon of time.
            //ALTERNATIVELY: LOOK INTO HOW TO SPAWN ONE OF THE "SPAWNABLE PERSOS" SINCE LUMS ARE ONE OF THOSE x)

            SetRule("DefaultName");
        }

        protected override void LateUpdate() {
            base.LateUpdate();
            triggerVolume.transform.localPosition = triggerPos;
        }

        protected void Rule_DefaultName() {
            //
            LookAt3D(rayman.pos);
        }

        
//         
//         protected void Rule_RayClose() {
//             if (rayman.DistTo(awayTrigger) >= 1.2f) {
//                 SetRule("RayAway");
//             }
//         }
// 
//         protected void Rule_RayAway() {
//             if (rayman.DistTo(awayTrigger) < 1.2f) {
//                 //as Rayman moves into the tunnel, change its type back to Red Lum and move it back to where it came from. There... the player will never suspect a thing x)
//                 ResetToOriginal();
// 
//                 SetRule("RayClose");
//             }
//         }
    }
}