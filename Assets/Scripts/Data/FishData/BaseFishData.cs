using UnityEngine;

namespace Data.FishData
{
    [CreateAssetMenu(fileName = "BaseFishData", menuName = "FishingGame/Fish/BaseFish")]
    public class BaseFishData : ScriptableObject
    {
        public string fishName;
        public string description;
        public Sprite fishSprite; // 鱼的图片
        public int fishValue; // 鱼的价值
        public float fishMoveSpeed; // 鱼的重量
    }
}