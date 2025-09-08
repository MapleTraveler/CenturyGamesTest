using UnityEngine;

namespace Data.FishData
{
    public enum EFishType
    {
        TropicalRedFish,  // 红色小鱼
        PufferFish,       // 河豚
        StripedFish,      // 条纹鱼
        SmallSilverFish   // 银色小鱼
    }
    [CreateAssetMenu(fileName = "BaseFishData", menuName = "FishingGame/Fish/BaseFish")]
    public class BaseFishData : ScriptableObject
    {
        [Header("基础属性")]
        public string fishName;
        public string description;
        public string prefabPath; // 预制体路径，使用 Resources 加载
        public EFishType fishType;
        public Sprite fishSprite; // 鱼的图片
        public int fishValue; // 鱼的价值
        public float fishMoveSpeed; // 鱼的移动速度
        
        [Min(0f)] public float minDepth;   // 允许的最小深度（含）
        [Min(0f)] public float maxDepth; // 允许的最大深度（含）
        [Min(0)] public int weight = 1;         // 权重（用于随机）
        
        [Header("基础行为参数")]
        public float detectionRadius = 2f;    // 探测半径
        public float escapeSpeed = 1.5f;      // 逃跑速度
        
        // —— 可选辅助：判断某深度是否可生成 ——
        public bool IsDepthEligible(float depth)
            => depth >= minDepth && depth <= maxDepth && weight > 0;
    }
}