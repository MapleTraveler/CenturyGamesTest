using System.Collections.Generic;
using Data;
using Data.FishData;
using GameLogic.FishLogic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Manager
{
    public enum PerRowSideMode
    {
        AlwaysLeft,       // 每格都选左
        AlwaysRight,      // 每格都选右
        RandomEachRow,    // 每格随机选左/右
        Alternate         // 左右交替
    }
    /// <summary>
    /// 鱼类生成策略接口
    /// </summary>
    public interface IFishSpawnPolicy
    {
        EFishType? Select(float depth, IEnumerable<BaseFishData> data);
    }

    public sealed class WeightedByDepthPolicy : IFishSpawnPolicy
    {
        public EFishType? Select(float depth, IEnumerable<BaseFishData> data)
        {
            var eligibles = new List<BaseFishData>();
            foreach (var d in data)
            {
                if (d.IsDepthEligible(depth))
                    eligibles.Add(d);
            }
            if (eligibles.Count == 0) return null;

            int total = 0;
            foreach (var d in eligibles) total += Mathf.Max(0, d.weight);

            int roll = Random.Range(0, total);
            foreach (var d in eligibles)
            {
                roll -= Mathf.Max(0, d.weight);
                if (roll < 0) return d.fishType;
            }
            return eligibles[^1].fishType;
        }
    }
    
    
    /// <summary>
    /// 垂直空间重生点管理器：仅负责“找位置 + 算深度 + 调用 Manager”
    /// </summary>
    public class VerticalFishSpawner
    {
        private readonly float _leftSpawnX, _rightSpawnX, _topBoundary, _bottomBoundary;

        // 位置/密度/碰撞参数（Spawner 的唯一职责）
        private float _gridSize = 1.5f; // 格栅宽度
        private float _minSpawnDistance = 2f; // 与已占初始点的最小距离
        private int   _maxObjectsPerSide = 5; // 每侧最大生成数量
        
        private float _objectRadius = 0.5f;  // 物理检测半径
        private LayerMask _checkLayerMask = -1; // 碰撞检测层

        // 依赖
        private readonly FishManager _fishManager;
        private readonly IFishSpawnPolicy _policy;
        
        private List<float> _cachedTopDownYList;

        public VerticalFishSpawner(
            MapBoundConfigData mapBound,
            FishManager fishManager,
            IFishSpawnPolicy policy,
            float gridSize = 1.5f,
            float minSpawnDistance = 2f,
            int   maxObjectsPerSide = 5,
            float objectRadius = 0.5f,
            LayerMask? checkLayerMask = null)
        {
            _leftSpawnX    = mapBound.leftSpawnX;
            _rightSpawnX   = mapBound.rightSpawnX;
            _topBoundary   = mapBound.topBoundary;
            _bottomBoundary= mapBound.bottomBoundary;

            _fishManager   = fishManager;
            _policy        = policy;

            _gridSize          = gridSize;
            _minSpawnDistance  = minSpawnDistance;
            _maxObjectsPerSide = maxObjectsPerSide;
            _objectRadius      = objectRadius;
            _checkLayerMask    = checkLayerMask ?? -1;

            _cachedTopDownYList = BuildTopDownYList();
        }
        

        /// <summary>
        /// 自上而下一次性遍历所有栅格；对每格只选择左/右一侧尝试生成
        /// </summary>
        public int FillDepthPassOnce(PerRowSideMode mode = PerRowSideMode.RandomEachRow)
        {
            var yList = _cachedTopDownYList;
            int spawned = 0;
            bool altLeft = true; // 供 Alternate 使用

            foreach (var y in yList)
            {
                // 决定本格要尝试的“唯一侧”
                ESpawnSide side = mode switch
                {
                    PerRowSideMode.AlwaysLeft    => ESpawnSide.Left,
                    PerRowSideMode.AlwaysRight   => ESpawnSide.Right,
                    PerRowSideMode.RandomEachRow => (Random.value > 0.5f ? ESpawnSide.Left : ESpawnSide.Right),
                    PerRowSideMode.Alternate     => (altLeft ? ESpawnSide.Left : ESpawnSide.Right),
                    _ => ESpawnSide.Left
                };
                if (mode == PerRowSideMode.Alternate) altLeft = !altLeft;

                // 若该侧已满，直接跳过这一格（不尝试另一侧）
                if (IsSideFull(side)) continue;

                // 在这一格、这一侧尝试“只生成一条”
                var fish = TrySpawnAtY(side, y);
                if (fish != null) spawned++;

                
            }

            return spawned;
        }
        public BaseFish TrySpawnAtY(ESpawnSide side, float y)
        {
            var pos = new Vector3(GetSideX(side), y, 0f);
            if (!IsPositionValid(pos, side)) return null;

            float depth = _topBoundary - y; // 深度 = 顶部到该 y 的距离
            return _fishManager.SpawnAt(pos, depth, side, _policy);
        }
        


        // —— 仅位置相关的工具 —— 
        private float GetSideX(ESpawnSide s) => s == ESpawnSide.Left ? _leftSpawnX : _rightSpawnX;

        private ESpawnSide ResolveSide(ESpawnSide s)
        {
            if (s != ESpawnSide.Left && s != ESpawnSide.Right)
            {
                // 随机，但优先未满侧
                bool leftFull  = IsSideFull(ESpawnSide.Left);
                bool rightFull = IsSideFull(ESpawnSide.Right);
                if (leftFull && rightFull) return ESpawnSide.Left;
                if (leftFull)  return ESpawnSide.Right;
                if (rightFull) return ESpawnSide.Left;
                return Random.value > 0.5f ? ESpawnSide.Left : ESpawnSide.Right;
            }
            return s;
        }

        private bool IsSideFull(ESpawnSide s)
            => _fishManager.GetSideCount(s) >= _maxObjectsPerSide;

        private List<float> BuildTopDownYList()
        {
            var list = new List<float>();
            float range = _topBoundary - _bottomBoundary;
            int gridCount = Mathf.Max(1, Mathf.FloorToInt(range / _gridSize));
            for (int i = 0; i < gridCount; i++)
            {
                float y = _topBoundary - (i + 0.5f) * _gridSize;
                if (y <= _bottomBoundary + _objectRadius || y >= _topBoundary - _objectRadius) continue;
                list.Add(y);
            }
            return list;
        }

        private bool IsPositionValid(Vector3 pos, ESpawnSide side)
        {
            // 与已占初始点的最小间距（由 Manager 的注册表提供）
            if (!_fishManager.IsAreaFree(side, pos, _minSpawnDistance))
                return false;

            // 物理重叠检测（场景物体）
            var cols = Physics.OverlapSphere(pos, _objectRadius + 0.05f, _checkLayerMask);
            return cols == null || cols.Length == 0;
        }

#if UNITY_EDITOR
        public void DrawGizmos()
        {
            // 边界线
            Gizmos.color = Color.yellow;
            var topL = new Vector3(_leftSpawnX,  _topBoundary, 0);
            var botL = new Vector3(_leftSpawnX,  _bottomBoundary, 0);
            var topR = new Vector3(_rightSpawnX, _topBoundary, 0);
            var botR = new Vector3(_rightSpawnX, _bottomBoundary, 0);
            Gizmos.DrawLine(topL, botL);
            Gizmos.DrawLine(topR, botR);
            Gizmos.DrawLine(topL, topR);
            Gizmos.DrawLine(botL, botR);

            // 栅格参考
            Gizmos.color = Color.gray;
            foreach (var y in _cachedTopDownYList)
            {
                Gizmos.DrawLine(new Vector3(_leftSpawnX - 0.5f,  y, 0), new Vector3(_leftSpawnX + 0.5f,  y, 0));
                Gizmos.DrawLine(new Vector3(_rightSpawnX - 0.5f, y, 0), new Vector3(_rightSpawnX + 0.5f, y, 0));
            }

            // 已占初始位置（从 Manager 查询）
            Gizmos.color = Color.red;
            foreach (var p in _fishManager.GetInitialPositions(ESpawnSide.Left))
                Gizmos.DrawWireSphere(p, _minSpawnDistance * 0.5f);
            foreach (var p in _fishManager.GetInitialPositions(ESpawnSide.Right))
                Gizmos.DrawWireSphere(p, _minSpawnDistance * 0.5f);
        }
#endif
    }
}
