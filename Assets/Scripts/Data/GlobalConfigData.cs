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
        //public int fishCount;
        public SerializableDictionary<EFishType,BaseFishData> fishDataDict;
        
#if UNITY_EDITOR
        // 编辑器里改动任意字段都会触发，可安全提示重复键
        private void OnValidate()
        {
            var dups = fishDataDict.FindDuplicateKeys();
            if (dups.Count > 0)
            {
                Debug.LogWarning($"[GlobalConfigData] 检测到重复键：{string.Join(", ", dups)}", this);
            }
        }
#endif

        private void OnEnable()
        {
            // 运行时（含进Play）严格校验：有重复就报错/中断（按你的需求）
            var dups = fishDataDict.FindDuplicateKeys();
            if (dups.Count > 0)
            {
                Debug.LogError($"[GlobalConfigData] 运行时禁止重复键：{string.Join(", ", dups)}", this);
                // 这里你可以选择：抛异常、禁用系统、或自动去重
            }
        }
    }
}