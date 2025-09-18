namespace Core.Runtime.Abstractions
{
    // 解决的问题：创建时的上下文（父节点、初始 Transform、阵营、出生格、是否静默加载、Addressables 句柄……）高度可变。
    /// <summary>
    /// 上下文应用器接口
    /// </summary>
    /// <typeparam name="T">上下文类型</typeparam>
    public interface IContextApplier<in T> 
    {
        void Apply(T entity, object context);
    }
}