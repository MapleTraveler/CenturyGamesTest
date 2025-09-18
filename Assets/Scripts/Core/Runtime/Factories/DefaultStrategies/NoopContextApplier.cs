using Core.Runtime.Abstractions;

namespace Core.Runtime.Factories.DefaultStrategies
{
    public sealed class NoopContextApplier<T> : IContextApplier<T>
    {
        public void Apply(T entity, object context) { /* no-op */ }
    }
}