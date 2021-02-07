namespace ModTools
{
    public interface IGameObject
    {
        void Update();
    }

    public interface IDestroyableObject
    {
        void OnDestroy();
    }

    public interface IAwakingObject
    {
        void Awake();
    }

    public interface IUIObject
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709", MessageId = "GUI", Justification = "Unity method")]
        void OnGUI();
    }
}
