using Core.Runtime.Abstractions;

namespace Core.Runtime.Factories.DefaultStrategies
{
    public sealed class NullMonoPool<T> : IObjectPool<T> where T : UnityEngine.Component
    {
        private readonly T _prefab;
        public NullMonoPool(T prefab) { _prefab = prefab; }
        public T Get() => UnityEngine.Object.Instantiate(_prefab);
        public void Return(T obj) => UnityEngine.Object.Destroy(obj.gameObject);
        
    }
}