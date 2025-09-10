using System;
using System.Collections.Generic;
using Data;
using Data.FishData;
using GameLogic.FishLogic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Manager
{
    public enum ESpawnSide { Left, Right }
    
    public struct SpawnInfo
    {
        public ESpawnSide side;
        public Vector3   initialPos;
        public float     depth;
    }
    
    /// <summary>
    /// 鱼类实体声明周期管理器，兼职生成器，或许可以把他通用化
    /// </summary>
    public class FishManager
    {
        // ----- 配置数据 -----
        private readonly IReadOnlyDictionary<EFishType, BaseFishData> _fishDataDic;
    
        // ----- 运行时数据 -----
        private readonly Dictionary<int,BaseFish> _fishInstanceDic = new Dictionary<int, BaseFish>();
       
        public IEnumerable<BaseFishData> IEFishData => _fishDataDic.Values; // 给 Spawner 用来筛选深度可用鱼种
        
        private readonly Dictionary<int, SpawnInfo> _spawnInfos = new();// 生成元数据（只记录“初始占位”，用于生点去重/统计）
        private readonly List<int> _hookedFishIds = new List<int>(); // 钩住的鱼类列表（用于结算）
        
        public event Action<BaseFish, SpawnInfo> OnFishSpawned;
        public event Action<BaseFish> OnFishDestroyed;

    
        public FishManager(Dictionary<EFishType,BaseFishData> fishDataDic)
        {
            _fishDataDic = fishDataDic;
        }
        
        
        // ----- 对外函数 -----
        // 按策略在指定位置/深度生成（并注册） 
        public BaseFish SpawnAt(Vector3 pos, float depth, ESpawnSide side, IFishSpawnPolicy policy)
        {
            var fishType = policy.Select(depth, IEFishData);
            if (!fishType.HasValue) return null;
            return GenerateAndRegister(fishType.Value, pos, depth, side);
        }
        // 将钩住的鱼进行缓存
        public bool TryHookFish(int fishInstanceID, Transform hookTransform)
        {
            if (!_fishInstanceDic.ContainsKey(fishInstanceID) || _hookedFishIds.Contains(fishInstanceID)) return false;
            // Debug.Log($"FishMgr Receive Hook Fish {fishInstanceID}");
            BaseFish fish = _fishInstanceDic[fishInstanceID];
            if (fish.Hooked(hookTransform))
            {
                _hookedFishIds.Add(fishInstanceID);
                return true;
            }

            return false;
        }
        
        
        
        
        // 占位检测（用于 Spawner 的最小距离判断） 
        public bool IsAreaFree(ESpawnSide side, Vector3 candidatePos, float minDistance)
        {
            foreach (var kv in _spawnInfos)
            {
                var info = kv.Value;
                if (info.side != side) continue;
                if (Vector3.Distance(candidatePos, info.initialPos) < minDistance)
                    return false;
            }
            return true;
        }
        
        
        public int GetSideCount(ESpawnSide side)
        {
            int cnt = 0;
            foreach (var info in _spawnInfos.Values)
                if (info.side == side) cnt++;
            return cnt;
        }
    
        public IEnumerable<Vector3> GetInitialPositions(ESpawnSide side)
        {
            foreach (var info in _spawnInfos.Values)
                if (info.side == side) 
                    yield return info.initialPos;
        }
        public FishSettlementData GetSettlementData()
        {
            var dict = new Dictionary<BaseFishData, int>();
            int totalValue = 0;
            foreach (var id in _hookedFishIds)
            {
                if (_fishInstanceDic.TryGetValue(id, out var fish))
                {
                    var data = _fishDataDic[fish.FishType];
                    dict.TryAdd(data, 0);
                    dict[data]++;
                    totalValue += data.fishValue;
                }
            }
            return new FishSettlementData { caughtFishData = dict, totalValue = totalValue };
        }

        // ----- 内部函数 -----
        
        
        /// <summary>
        /// 在指定位置实例化并注册鱼类实体
        /// </summary>
        /// <param name="fishType"></param>
        /// <param name="generatePos"></param>
        /// <param name="depth"></param>
        /// <param name="side"></param>
        /// <returns></returns>
        private BaseFish GenerateAndRegister(EFishType fishType, Vector3 generatePos, float depth, ESpawnSide side)
        {
            if (!_fishDataDic.TryGetValue(fishType, out var fishData))
            {
                Debug.LogError($"FishManager: 未找到鱼类数据，类型：{fishType}");
                return null;
            }

            var prefab = Resources.Load<GameObject>($"{GlobalSetting.FISH_PREFAB_PATH}{fishData.prefabPath}");
            if (prefab == null)
            {
                Debug.LogError($"FishManager: 预制体加载失败，路径错误？ {fishData.prefabPath}");
                return null;
            }
            // var radius = prefab.GetComponent<CircleCollider2D>()?.radius ?? 0.5f;
            var go = Object.Instantiate(prefab, generatePos, prefab.transform.rotation);
            var fish = go.GetComponent<BaseFish>();
            if (fish == null)
            {
                Debug.LogError($"FishManager: 预制体缺少 BaseFish 组件！ {fishData.prefabPath}");
                Object.Destroy(go);
                return null;
            }
            int startingDirection = side == ESpawnSide.Left ? -1 : 1; // 这样设置是因为在墙壁上重生，一开始会触发一次转向
            fish.Initialize(fishData,startingDirection);

            int id = go.GetInstanceID();
            _fishInstanceDic[id]  = fish;
            _spawnInfos[id] = new SpawnInfo { side = side, initialPos = generatePos, depth = depth };

            OnFishSpawned?.Invoke(fish, _spawnInfos[id]);
            return fish;
        }
    
        /// <summary>
        /// 通过 Id 删除鱼类
        /// </summary>
        /// <param name="fishInstanceID"></param>
        public void DestroyFish(int fishInstanceID)
        {
            if (_fishInstanceDic.TryGetValue(fishInstanceID, out var fish))
            {
                Object.Destroy(fish.gameObject);
                _fishInstanceDic.Remove(fishInstanceID);
                _spawnInfos.Remove(fishInstanceID);
                OnFishDestroyed?.Invoke(fish);
            }
            else
            {
                Debug.LogWarning($"FishManager: 尝试销毁不存在的鱼类实例，ID：{fishInstanceID}");
            }
        }

        public void DestroyAllFish()
        {
            foreach (var f in _fishInstanceDic.Values)
                Object.Destroy(f.gameObject);
            _fishInstanceDic.Clear();
            _spawnInfos.Clear();
        }

        public void Reset()
        {
            DestroyAllFish();         // 清除场景中所有鱼
            _hookedFishIds.Clear();   // 清空钩住的鱼ID列表
            _spawnInfos.Clear();      // 清空生成元数据
        }
        
    
        public void OnUpdate()
        {
            foreach (var fish in _fishInstanceDic.Values)
            {
                fish.UpdateLogic();
            }
        }
    }
}