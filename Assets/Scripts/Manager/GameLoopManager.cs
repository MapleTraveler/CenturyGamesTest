namespace Manager
{
    public class GameLoopManager
    {
        private FishManager _fishManager;
        private VerticalFishSpawner _verticalFishSpawner;
        //private int _fishCount = 100;      // 当前地图中的鱼的数量
        
        public GameLoopManager(FishManager fishManager,VerticalFishSpawner verticalFishSpawner)
        {
            _fishManager = fishManager;
            _verticalFishSpawner = verticalFishSpawner;
        }
        
        public void StartGameLoop()
        {
            _verticalFishSpawner.FillDepthPassOnce();
        }
        
        public void UpdateGameLoop()
        {
            _fishManager.OnUpdate();
        }

        public void StopGameLoop()
        {
            
        }
    }
}