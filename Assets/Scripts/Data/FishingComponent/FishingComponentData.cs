using UnityEngine;

namespace Data.FishingComponent
{
    public class FishingComponentData : ScriptableObject
    {
        [Header("基础信息")]
        public string componentName;
        public string componentDescription;
        public Sprite componentIcon;
    }
}