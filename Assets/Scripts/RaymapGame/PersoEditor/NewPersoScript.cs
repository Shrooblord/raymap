﻿using System.Collections.Generic;
using UnityEngine;
namespace RaymapGame.Rayman2.Persos {
    /// <summary>
    /// Description
    /// </summary>
    public partial class NewPersoScript : PersoController {
        //public override AnimSFX[] animSfx => new AnimSFX[] { };

        protected override void OnStart() {
            SetRule("Default");
        }

        void Rule_Default() {

        }
    }
}