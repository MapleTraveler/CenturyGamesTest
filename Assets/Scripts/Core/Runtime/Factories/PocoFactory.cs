using System;
using Core.Runtime.Abstractions;
using Core.Runtime.Factories.DefaultStrategies;

namespace Core.Runtime.Factories
{
    public sealed class PocoFactory<TDataKey, TData, TEntity, TEntityKey>
        : IEntityFactory<TDataKey, TData, TEntity, TEntityKey>
        where TEntity : class, new()
    {
        private readonly IIdAllocator<TEntityKey> _idAlloc;
        private readonly IObjectPool<TEntity> _pool;
        private readonly IEntityInitializer<TEntity, TData> _initializer;
        private readonly Func<TEntity, TEntityKey> _getId;   // 如何读取实体 Id
        private readonly IContextApplier<TEntity> _ctxApplier;

        public PocoFactory(
            IIdAllocator<TEntityKey> idAllocator,
            IObjectPool<TEntity> pool,
            IEntityInitializer<TEntity, TData> initializer,
            Func<TEntity, TEntityKey> getId,
            IContextApplier<TEntity> ctxApplier = null)
        {
            _idAlloc = idAllocator;
            _pool = pool;
            _initializer = initializer;
            _getId = getId ?? throw new ArgumentNullException(nameof(getId));
            _ctxApplier = ctxApplier ?? new NoopContextApplier<TEntity>();
        }

        public TEntity Create(TDataKey key, TData data, object context = null)
        {
            var e = _pool.Get() ?? new TEntity();
            _initializer?.Init(e, data);
            _ctxApplier?.Apply(e, context);
            return e;
        }

        public void Destroy(TEntity entity) => _pool.Return(entity);
        public TEntityKey GetId(TEntity entity) => _getId(entity);
    }

}