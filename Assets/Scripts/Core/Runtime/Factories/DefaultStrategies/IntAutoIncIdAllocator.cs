using Core.Runtime.Abstractions;

namespace Core.Runtime.Factories.DefaultStrategies
{
    public sealed class IntAutoIncIdAllocator : IIdAllocator<int>
    {
        private int _currentId = 0;
        public int New()
        {
            return ++_currentId;
        }
    }
}