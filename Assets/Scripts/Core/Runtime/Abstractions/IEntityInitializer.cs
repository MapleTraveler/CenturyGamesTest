namespace Core.Runtime.Abstractions
{
    // 解决的问题：初始化过程差异巨大（领域状态、皮肤/模型、Buff、AI 黑板、地址句柄、异步资源绑定……）
    /// <summary>
    /// 实体初始化接口
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    /// <typeparam name="TData">实体数据类型</typeparam>
    public interface IEntityInitializer<in T, in TData>
    {
        void Init(T entity, TData data);
    }
}