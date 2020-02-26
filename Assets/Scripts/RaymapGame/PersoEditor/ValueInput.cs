//================================
//  By: Adsolution
//================================

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;


#if UNITY_EDITOR
namespace RaymapGame.PersoEditor {
    [System.Serializable]
    public struct ValueInput : IInspectable {
        public enum Type { Const, Func }

        public ParameterInfo pi;
        public ValueInput(ParameterInfo pi) {
            this.pi = pi;
            type = Type.Const;
            constVal = 0;
            _func = null;
            funcParams = null;
        }
        public System.Type valueType => pi.ParameterType;
        public Type type;
        public object constVal;

        MethodInfo _func;
        public MethodInfo func { get => _func; set {
                _func = value;
                var fp = value.GetParameters();
                funcParams = new ValueInput[fp.Length];
            } }
        public ValueInput[] funcParams;

        public void Inspect(EditorMode mode) {
            switch (mode) {
                case EditorMode.UnityInspector:
                    if (GUILayout.Button("C", GUILayout.Width(20)))
                        type = Type.Const;
                    if (GUILayout.Button("F", GUILayout.Width(20)))
                        type = Type.Func;
                    switch (type) {
                        case Type.Const: GUILayout.TextField(constVal.ToString()); break;
                        case Type.Func: EditorGUILayout.Popup(0, new string[0]); break;
                    }
                    break;
            }
        }

        public object[] GetParamValuesOn(object perso) {
            var r = new List<object>();
            foreach (var v in funcParams)
                r.Add(v.InvokeAndGetOn(perso));
            return r.ToArray();
        }

        public object InvokeAndGetOn(object perso) {
            return func.Invoke(perso, GetParamValuesOn(perso));
        }
    }
}
#endif
