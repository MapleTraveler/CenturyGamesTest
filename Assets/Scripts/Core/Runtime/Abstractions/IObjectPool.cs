namespace Core.Runtime.Abstractions
{
    // 解决的问题：是否池化、如何池化、池策略（预热、最大容量、回收策略）常常变化。
    /// <summary>
    /// 对象池接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IObjectPool<T>
    {
        T Get(); 
        void Return(T obj); 
    }
}