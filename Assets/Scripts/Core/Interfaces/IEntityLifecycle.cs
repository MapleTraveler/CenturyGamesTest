namespace Core.Interfaces
{
    public interface IEntityLifecycle
    {
        public void OnInit();
        public void OnUpdate();
        public void OnDeinit();
    }
}