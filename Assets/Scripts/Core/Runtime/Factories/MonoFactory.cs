using Core.Runtime.Abstractions;
using Core.Runtime.Factories.DefaultStrategies;

public interface IHasId<out TId> { TId Id { get; } }

public sealed class MonoFactory<TDataKey, TData, TView, TEntityKey>
    : IEntityFactory<TDataKey, TData, TView, TEntityKey>
    where TView : UnityEngine.Component, IHasId<TEntityKey>
{
    private readonly IObjectPool<TView> _pool;
    private readonly IEntityInitializer<TView, TData> _initializer;
    private readonly IContextApplier<TView> _ctxApplier;

    public MonoFactory(
        IObjectPool<TView> pool,
        IEntityInitializer<TView, TData> initializer,
        IContextApplier<TView> ctxApplier = null)
    {
        _pool = pool;
        _initializer = initializer;
        _ctxApplier = ctxApplier ?? new NoopContextApplier<TView>();
    }

    public TView Create(TDataKey key, TData data, object context = null)
    {
        var v = _pool.Get();
        _initializer?.Init(v, data);
        _ctxApplier?.Apply(v, context);
        return v;
    }

    public void Destroy(TView entity) => _pool.Return(entity);
    public TEntityKey GetId(TView entity) => entity.Id;
}