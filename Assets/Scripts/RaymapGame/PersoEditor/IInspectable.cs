namespace RaymapGame.PersoEditor {
    public enum EditorMode {
        UnityInspector, InGame
    }
    public interface IInspectable {
        void Inspect(EditorMode mode);
    }
}