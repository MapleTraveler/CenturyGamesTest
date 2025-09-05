using Core.Collections;
using Data.FishData;
using Data.FishingComponent;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Data
{
    [CreateAssetMenu(fileName = "GlobalConfigData", menuName = "FishingGame/GlobalConfig")]
    public class GlobalConfigData : ScriptableObject
    {
        [Header("输入配置文件")]
        public InputActionAsset inputActionAsset;
    
        [Header("鱼竿配置")]
        public FishRodConfig fishRodConfig;
    
        [Header("鱼钩配置")]
        public GameObject fishHookPrefab;


        [Header("局内配置")] 
        public Vector2 fishHookStartPos;
        public int fishCount;
        public SerializableDictionary<EFishType,BaseFishData> fishDataDict;
    }
}