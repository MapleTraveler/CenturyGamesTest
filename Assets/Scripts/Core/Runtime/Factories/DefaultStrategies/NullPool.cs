using Core.Runtime.Abstractions;

namespace Core.Runtime.Factories.DefaultStrategies
{
    public sealed class NullPool<T> : IObjectPool<T> where T : new()

    {
        public T Get()
        {
            return new T();
        }

        public void Return(T obj)
        {
            // Do nothing
        }
    }
}