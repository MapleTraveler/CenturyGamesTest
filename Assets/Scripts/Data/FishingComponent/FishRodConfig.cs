using UnityEngine;

namespace Data.FishingComponent
{
    // 鱼竿配置 - 组合各个组件
    [CreateAssetMenu(fileName = "FishRodConfig", menuName = "FishingGame/FishRodConfig")]
    public class FishRodConfig : ScriptableObject
    {
        [Header("基础信息")]
        public string rodName;
        
        [Header("组件配置")]
        public FishHookData fishHookData;
        public FishSinkerData fishSinkerData;
        public FishingLineData fishingLineData;
        
        [Header("下面全部为全局属性")]
        public float unselectableTime = 1.0f;   // 开局无敌时间
        
        [Header("下沉属性")]
        public float sinkSpeed = 1.0f;
        
        [Header("上升属性")]
        public float ascentSpeed = 0.8f;
        public float accelerateAscentSpeed = 1.5f;
        
    }
}