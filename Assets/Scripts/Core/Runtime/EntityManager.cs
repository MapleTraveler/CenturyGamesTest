using System;
using System.Collections.Generic;

namespace Core.Runtime
{
    
    public enum EntityCreateFailReason
    {
        None,                   // 创建成功
        DataMissing,            // 数据缺失
        FactoryReturnedNull,    // 工厂返回了空对象
        DuplicateId,            // 重复的实体Id
        Unknown                 // 未知错误
    }
    public enum RegisterFailReason 
    { 
        None,                   // 注册成功
        NullEntity,             // 实体为空
        DuplicateId             // 重复的实体Id
    }
    public interface IEntityFactory<TDataKey, TData, TEntity,TEntityKey>
    {
        TEntity Create(TDataKey key, TData data, object context = null);
        void Destroy(TEntity entity);
        void Update(TEntity entity);
        TEntityKey GetId(TEntity entity);
    }
    /// <summary>
    /// 通用基类实体管理器，兼容 Mono和非Mono对象
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

        public event Action<TEntityKey, TEntity, TDataKey, TData, object> OnCreated;
        public event Action<TEntityKey, TEntity, TDataKey, TData, object> OnRegistered;
        public event Action<TEntityKey, TEntity> OnDestroyed;
        
        
        
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
                case EntityCreateFailReason.DataMissing:
                    throw new KeyNotFoundException($"数据字典中未找到 Key={key} 对应的数据");
                case EntityCreateFailReason.FactoryReturnedNull:
                    throw new InvalidOperationException($"实体工厂创建实体失败，Key={key}");
                case EntityCreateFailReason.DuplicateId:
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
            out EntityCreateFailReason failReason,
            object context = null)
        {
            entityId = default;
            entity = default;
            failReason = EntityCreateFailReason.None;

            if (!dataDic.TryGetValue(key, out var data))
            {
                failReason = EntityCreateFailReason.DataMissing;
                return false;
            }

            // 通过工厂创建实体
            TEntity created = entityFactory.Create(key, data, context);
            if (created == null)
            {
                failReason = EntityCreateFailReason.FactoryReturnedNull;
                return false;
            }

            // 获取实体Id并注册
            TEntityKey id = entityFactory.GetId(created);

            if (!instances.TryAdd(id, created))
            {
                // 重复则销毁刚创建的实体
                entityFactory.Destroy(created);
                failReason = EntityCreateFailReason.DuplicateId;
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
            out RegisterFailReason reason,
            TDataKey key = default,
            object context = null)
        {
            reason = RegisterFailReason.None;
            if (entity == null) { reason = RegisterFailReason.NullEntity; return false; }

            var id = entityFactory.GetId(entity);
            if (!instances.TryAdd(id, entity))
            {
                reason = RegisterFailReason.DuplicateId;
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
            OnDestroyed?.Invoke(entityId, entity);
            return true;
        }
        
        
        
        // TODO:这个要放在这里，工厂吗？（待问，待定）
        /// <summary>
        /// 通过工厂执行管理器中物体的更新逻辑
        /// </summary>
        public void UpdateAll()
        {
            foreach (var entity in instances.Values)
            {
                entityFactory.Update(entity);
            }
        }


    }
    
}