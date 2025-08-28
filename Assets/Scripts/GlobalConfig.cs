using Data;
using Data.FishingComponent;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

/// <summary>
/// 所有需要配置的全部扔到这里
/// </summary>
public class GlobalConfig : MonoBehaviour
{ 
    [Header("输入配置文件")]
    public InputActionAsset inputActionAsset;
    
    [Header("鱼竿配置")]
    public FishRodConfig fishRodConfig;
    
    [Header("鱼钩配置")]
    public GameObject fishHookPrefab;

    [Header("全局配置")] 
    public float boardAxis = 2.5f;
}