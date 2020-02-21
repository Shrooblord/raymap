using System.Reflection;
using System.Collections.Generic;

namespace RaymapGame.PersoEditor {
    [System.Serializable]
    public struct MethodInvoke : IInspectable {
        public MethodInfo method;
        public ValueInput[] methodParams;

        public MethodInvoke(MethodInfo method) {
            this.method = method;

            var pars = new List<ValueInput>();
            foreach (var pi in method.GetParameters())
                pars.Add(new ValueInput(pi));
            methodParams = pars.ToArray();
        }

        public void Inspect(EditorMode mode) {
            switch (mode) {
                case EditorMode.UnityEditor: break;
            }
        }
    }
}