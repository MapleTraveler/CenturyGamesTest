using System;
using System.Collections.Generic;
using Core.Runtime.Abstractions;
using Core.Runtime.CoreTypes;

namespace Core.Runtime
{
    /// <summary>
    /// 通用基类实体生命周期管理器，兼容 Mono和非Mono对象
    /// </summary>
    /// <typeparam name="TDataKey">数据类Id的数据类型</typeparam>
    /// <typeparam name="TData">数据类的类型</typeparam>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TEntityKey">实体Id的数据类型</typeparam>
    public class EntityManager<TDataKey, TData, TEntity,TEntityKey>
    {
        protected readonly IReadOnlyDictionary<TDataKey, TData> dataDic;            // 实体数据字典
        protected readonly Dictionary<TEntityKey, TEntity> instances = new();       // 实体实例字典
        
        protected readonly IEntityFactory<TDataKey, TData, TEntity,TEntityKey> entityFactory;
        
        // 系统条目：系统 + 优先级 + 可选过滤（只更新满足条件的实体）
        private readonly List<(IEntityUpdater<TEntity> sys, int priority, Predicate<TEntity> filter)> _updaters = new();


        public event Action<TEntityKey, TEntity, TDataKey, TData, object> OnCreated;
        public event Action<TEntityKey, TEntity, TDataKey, TData, object> OnRegistered;
        public event Action<TEntityKey, TEntity> OnSelfDestroyed;
        
        
        
        public EntityManager(
            IReadOnlyDictionary<TDataKey, TData> dataDic,
            IEntityFactory<TDataKey, TData, TEntity,TEntityKey> entityFactory)
        {
            this.dataDic = dataDic ?? throw new ArgumentNullException(nameof(dataDic));
            this.entityFactory = entityFactory ?? throw new ArgumentNullException(nameof(entityFactory));
        }
        
        // ----- 对外接口 -----
        public int Count => instances.Count;
        
        public bool TryGet(TEntityKey id, out TEntity entity) => instances.TryGetValue(id, out entity);
        
        /// <summary>
        /// 通过工厂创建实体
        /// </summary>
        /// <param name="key">实体数据类Id</param>
        /// <param name="context">和实体创建有关的上下文</param>
        /// <returns>实体的唯一Id</returns>
        /// <exception cref="KeyNotFoundException">数据字典中未找到Key</exception>
        /// <exception cref="Exception"></exception>
        public TEntityKey Create(TDataKey key, object context = null)
        {
            if (TryCreate(key, out var entityId, out _, out var reason, context))
                return entityId;

            switch (reason)
            {
                case EEntityCreateFailReason.DataMissing:
                    throw new KeyNotFoundException($"数据字典中未找到 Key={key} 对应的数据");
                case EEntityCreateFailReason.FactoryReturnedNull:
                    throw new InvalidOperationException($"实体工厂创建实体失败，Key={key}");
                case EEntityCreateFailReason.DuplicateId:
                    throw new InvalidOperationException($"实体管理器中已存在相同实体Id，无法重复创建");
                default:
                    throw new InvalidOperationException("未知创建失败原因");
            }
        }
        
        
        /// <summary>
        /// 不抛异常的实体创建方法
        /// </summary>
        /// <param name="key">实体数据类Id</param>
        /// <param name="entityId">实体的唯一Id</param>
        /// <param name="entity">实体对象引用</param>
        /// <param name="failReason">创建失败的原因</param>
        /// <param name="context">和实体创建有关的上下文</param>
        /// <returns>创建结果</returns>
        public bool TryCreate(
            TDataKey key, 
            out TEntityKey entityId, 
            out TEntity entity,
            out EEntityCreateFailReason failReason,
            object context = null)
        {
            entityId = default;
            entity = default;
            failReason = EEntityCreateFailReason.None;

            if (!dataDic.TryGetValue(key, out var data))
            {
                failReason = EEntityCreateFailReason.DataMissing;
                return false;
            }

            // 通过工厂创建实体
            TEntity created = entityFactory.Create(key, data, context);
            if (created == null)
            {
                failReason = EEntityCreateFailReason.FactoryReturnedNull;
                return false;
            }

            // 获取实体Id并注册
            TEntityKey id = entityFactory.GetId(created);

            if (!instances.TryAdd(id, created))
            {
                // 重复则销毁刚创建的实体
                entityFactory.Destroy(created);
                failReason = EEntityCreateFailReason.DuplicateId;
                return false;
            }

            entityId = id;
            entity = created;
            OnCreated?.Invoke(id, created, key, data, context);
            return true;
        }
        
        // 不关心创建失败原因的重载
        public bool TryCreate(TDataKey key, out TEntityKey entityId, out TEntity entity, object context = null)
            => TryCreate(key, out entityId, out entity, out _, context);

        
        /// <summary>
        /// 注册非通过管理器创建的实体
        /// 注意：注册失败不会销毁实体
        /// 仅当实体确实是由外部创建且需要并入管理时使用
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <param name="reason">失败原因</param>
        /// <param name="key">实体的数据类Id</param>
        /// <param name="context">和实体创建有关的上下文</param>
        /// <returns>注册结果</returns>
        public bool TryRegisterExternal(
            TEntity entity,
            out ERegisterFailReason reason,
            TDataKey key = default,
            object context = null)
        {
            reason = ERegisterFailReason.None;
            if (entity == null) { reason = ERegisterFailReason.NullEntity; return false; }

            var id = entityFactory.GetId(entity);
            if (!instances.TryAdd(id, entity))
            {
                reason = ERegisterFailReason.DuplicateId;
                return false;
            }

            var hasData = dataDic.TryGetValue(key, out var data);
            OnRegistered?.Invoke(id, entity, key, hasData ? data : default, context);
            return true;
        }
        
        // 不关心注册失败原因的重载
        public bool TryRegisterExternal(TEntity entity, TDataKey key = default, object context = null)
            => TryRegisterExternal(entity, out _, key, context);
        
        /// <summary>
        /// 基于实体唯一Id通过工厂销毁实体
        /// </summary>
        /// <param name="entityId">实体唯一Id</param>
        /// <returns>销毁结果</returns>
        public bool Destroy(TEntityKey entityId)
        {
            if (!instances.Remove(entityId, out TEntity entity))
                return false;
            entityFactory.Destroy(entity);
            OnSelfDestroyed?.Invoke(entityId, entity);
            return true;
        }
        
        
        #region 更新系统
        

        /// <summary>
        /// 注册一个更新系统；可选：优先级（越小越先）、过滤器
        /// </summary>
        public void AddUpdater(IEntityUpdater<TEntity> system, int priority = 0, Predicate<TEntity> filter = null)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            _updaters.Add((system, priority, filter));
            // 如果系统实现了 ISystemOrder，用它的优先级
            if (system is ISystemOrder ord) priority = ord.Priority;
            // 重新排序（phase 以后再扩展，这里只按优先级）
            _updaters.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        /// <summary>
        /// 反注册更新系统
        /// </summary>
        public bool RemoveUpdater(IEntityUpdater<TEntity> system)
        {
            var idx = _updaters.FindIndex(t => ReferenceEquals(t.sys, system));
            if (idx >= 0) { _updaters.RemoveAt(idx); return true; }
            return false;
        }

        // 延迟队列：避免 Update 中直接改动 instances 造成枚举异常
        private readonly Queue<TEntityKey> _deferredRemove = new();
        private readonly List<(TDataKey key, object ctx)> _deferredCreate = new();

        /// <summary>
        /// 在本帧末尾移除实体（安全），更新循环中调用
        /// </summary>
        public void MarkForRemove(TEntityKey id) => _deferredRemove.Enqueue(id);

        /// <summary>
        /// 在本帧末尾创建实体（安全），更新循环内调用
        /// </summary>
        public void EnqueueCreate(TDataKey key, object context = null) => _deferredCreate.Add((key, context));

        // 延迟刷新结算
        private void FlushDeferred()
        {
            while (_deferredRemove.Count > 0)
            {
                var id = _deferredRemove.Dequeue();
                Destroy(id);
            }

            if (_deferredCreate.Count > 0)
            {
                for (int i = 0; i < _deferredCreate.Count; i++)
                {
                    var (k, ctx) = _deferredCreate[i];
                    TryCreate(k, out _, out _, ctx);
                }
                _deferredCreate.Clear();
            }
        }

        /// <summary>
        /// 通过已注册的系统执行更新逻辑（顺序：按优先级）
        /// </summary>
        public void UpdateAll(float deltaTime)
        {
            if (_updaters.Count == 0 || instances.Count == 0)
            {
                FlushDeferred(); // 即使没有系统，也处理一下延迟队列
                return;
            }

            // 快照一份，避免遍历过程中被修改
            // （这里用临时 List；若追求更少 GC，可做一个私有可复用缓存）
            var snapshot = new List<TEntity>(instances.Count);
            foreach (var e in instances.Values) snapshot.Add(e);

            // 系统调度
            for (int u = 0; u < _updaters.Count; u++)
            {
                var (sys, _, filter) = _updaters[u];
                for (int i = 0; i < snapshot.Count; i++)
                {
                    var e = snapshot[i];
                    if (filter == null || filter(e))
                        sys.Update(e, deltaTime);
                }
            }

            // 帧末 flush 增删
            FlushDeferred();
        }

        #endregion



    }
    
}