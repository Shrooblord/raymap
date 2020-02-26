using OpenSpace.AI;
using UnityEngine;

namespace RaymapGame {
    public static class Dsg {
        public static T GetValue<T>(DsgVarValue dsg) {
            switch (typeof(T).Name) {
                case "Boolean":
                    return (T)(object)dsg.valueBool;
                case "SByte":
                    return (T)(object)dsg.valueByte;
                case "Byte":
                    return (T)(object)dsg.valueUByte;
                case "Int16":
                    return (T)(object)dsg.valueShort;
                case "UInt16":
                    return (T)(object)dsg.valueUShort;
                case "Int32":
                    return (T)(object)dsg.valueInt;
                case "UInt32":
                    return (T)(object)dsg.valueUInt;
                case "Single":
                    return (T)(object)dsg.valueFloat;
                case "Vector3":
                    return (T)(object)dsg.valueVector;
                case "String":
                    return (T)(object)("TextRef(" + dsg.valueText + ")");
                case "WaypointGraph":
                    return (T)(object)dsg.valueGraph;
                case "Waypoint":
                    return (T)(object)dsg.valueWayPoint;
                case "GameMaterial":
                    return (T)(object)dsg.valueGameMaterial;
                case "VisualMaterial":
                    return (T)(object)dsg.valueVisualMaterial;
                case "ObjectList":
                    return (T)(object)dsg.valueObjectList;
                case "List":
                    return (T)(object)dsg.valueList;
                case "LightInfo":
                    return (T)(object)dsg.valueLight;
                case "Behavior":
                    return (T)(object)dsg.valueComport?.ShortName;
                case "EntryAction":
                    return (T)(object)dsg.valueInput;
                case "PersoController":
                    return (T)(object)PersoController.GetPerso(dsg.valuePerso.namePerso);
                case "State":
                    return (T)(object)dsg.valueAction?.ShortName;
                case "SuperObject":
                    return (T)(object)dsg.valueSuperObject;/*
                case DsgVarInfoEntry.DsgVarType.Caps:
                    return (T)(object)dsg.valueCaps;
                case DsgVarInfoEntry.DsgVarType.SOLinks:
                    return (T)(object)dsg.valueSOLinks;
                case DsgVarInfoEntry.DsgVarType.SoundEvent:
                    return (T)(object)dsg.valueSoundEvent;
                case DsgVarInfoEntry.DsgVarType.Way:
                    return (T)(object)dsg.valueWay;*/
            }
            return default;
        }
    }
}
