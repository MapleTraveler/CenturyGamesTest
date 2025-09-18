using Core.Runtime.Abstractions;

namespace Core.Runtime.Factories.DefaultStrategies
{
    public sealed class NoopInitializer<T, TData> : IEntityInitializer<T, TData>
    {
        public void Init(T entity, TData data) { /* no-op */ }
    }
}