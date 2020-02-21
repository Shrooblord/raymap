using System;
using System.Collections.Generic;
using System.Reflection;

namespace RaymapGame.PersoEditor {
    [System.Serializable]
    public class TypeData {
        public TypeData(Type type) {
            this.type = type;
            Init();
        }

        public Type type;
        public Dictionary<string, MethodInfo> actions = new Dictionary<string, MethodInfo>();
        public Dictionary<string, MethodInfo> functions = new Dictionary<string, MethodInfo>();

        void Init() {
            foreach (var m in type.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance)) {
                if (m.IsPublic && m.DeclaringType == type && !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_")) {
                    if (m.ReturnType == typeof(void))
                        actions.Add(m.Name, m);
                    else
                        functions.Add(m.Name, m);
                }
            }
        }
    }
}