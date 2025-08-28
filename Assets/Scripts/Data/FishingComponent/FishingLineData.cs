using UnityEngine;

namespace Data.FishingComponent
{
    [CreateAssetMenu(fileName = "FishingLineData", menuName = "FishingGame/Components/FishingLine")]
    public class FishingLineData : FishingComponentData
    {
        [Header("长度属性")]
        public float maxLength = 10.0f;         // 最大长度
    }
}