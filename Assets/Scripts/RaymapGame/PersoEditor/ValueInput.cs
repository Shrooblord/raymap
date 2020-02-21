using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace RaymapGame.PersoEditor {
    [System.Serializable]
    public struct ValueInput : IInspectable {
        public enum Type { Const, Func }

        public ValueInput(ParameterInfo pi) {
            this.pi = pi;
            type = Type.Const;
            constVal = 0;
            func = null;
        }

        public ParameterInfo pi;
        public System.Type valueType => pi.ParameterType;
        public Type type;
        public object constVal;
        public MethodInfo func;

        public void Inspect() {
            if (GUILayout.Button("C", GUILayout.Width(20)))
                type = Type.Const;
            if (GUILayout.Button("F", GUILayout.Width(20)))
                type = Type.Func;

            switch (type) {
                case Type.Const: GUILayout.TextField(constVal.ToString()); break;
                case Type.Func: EditorGUILayout.Popup(0, new string[0]); break;
            }
        }
    }
}