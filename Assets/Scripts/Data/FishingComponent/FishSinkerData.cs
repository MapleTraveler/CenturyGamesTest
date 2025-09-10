using UnityEngine;

namespace Data.FishingComponent
{
    [CreateAssetMenu(fileName = "FishSinkerData", menuName = "FishingGame/Components/FishSinker")]
    public class FishSinkerData : FishingComponentData
    {
        [Header("护盾属性")]
        public int shieldCount = 1; // 护盾数量

        public Color shieldColor;
        [Header("消耗护盾后的无敌时间")]
        public float invincibleTime = 1f; // 消耗护盾后的无敌
        public Color invincibleColor = Color.cyan;
    }
}