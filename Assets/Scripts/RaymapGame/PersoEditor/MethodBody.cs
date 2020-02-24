//================================
//  By: Adsolution
//================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RaymapGame.PersoEditor {
    public struct MethodBody : IInspectable {
        public MethodBody(params IInspectable[] body) {
            this.body = body.ToList();
        }
        public List<IInspectable> body;

        public void Inspect(EditorMode mode) {
            foreach (var e in body)
                e.Inspect(mode);
        }

        public void ExecuteOn(PersoController perso) {
            foreach (var e in body) {
                if (e is MethodInvoke a) {
                    a.InvokeOn(perso);
                }
            }
        }
    }
}