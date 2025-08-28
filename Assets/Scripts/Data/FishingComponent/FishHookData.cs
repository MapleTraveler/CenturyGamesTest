using UnityEngine;

namespace Data.FishingComponent
{
    [CreateAssetMenu(fileName = "FishHookData", menuName = "FishingGame/Components/FishHook")]
    public class FishHookData : FishingComponentData
    {
        [Header("移动属性")]
        public float horizontalSpeed = 1.0f;
        public float maxHorizontalRange = 2.5f; // 待定，可能配置在关卡类中
    }
}