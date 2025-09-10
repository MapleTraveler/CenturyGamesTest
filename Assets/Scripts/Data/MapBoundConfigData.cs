namespace Data
{
    /// <summary>
    /// 地图的配置数据，目前暂时由 GameManager 创建并分发
    /// </summary>
    public class MapBoundConfigData
    {
        public float leftSpawnX;     // 左侧重生点X坐标
        public float rightSpawnX;    // 右侧重生点X坐标
        public float topBoundary;    // 垂直空间顶部
        public float bottomBoundary; // 垂直空间底部
        public float hookMinX;
        public float hookMaxX;
    }
}