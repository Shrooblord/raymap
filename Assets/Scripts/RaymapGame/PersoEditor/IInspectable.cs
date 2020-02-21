namespace RaymapGame.PersoEditor {
    public enum EditorMode {
        UnityEditor, InGame
    }
    public interface IInspectable {
        void Inspect(EditorMode mode);
    }
}
