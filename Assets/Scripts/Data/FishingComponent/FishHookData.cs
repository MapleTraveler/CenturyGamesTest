using UnityEngine;

namespace Data.FishingComponent
{
    [CreateAssetMenu(fileName = "FishHookData", menuName = "FishingGame/Components/FishHook")]
    public class FishHookData : FishingComponentData
    {
        [Header("移动属性")]
        public float horizontalSpeed = 1.0f;

        [Header("捕捉属性")] public int canHookFishCount;
    }
}