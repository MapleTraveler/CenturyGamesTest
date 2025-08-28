using UnityEngine;

namespace Data.FishingComponent
{
    [CreateAssetMenu(fileName = "FishSinkerData", menuName = "FishingGame/Components/FishSinker")]
    public class FishSinkerData : FishingComponentData
    {
        [Header("护盾属性")]
        public int shieldCount = 1; // 护盾数量
    }
}