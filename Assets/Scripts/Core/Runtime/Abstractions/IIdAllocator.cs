namespace Core.Runtime.Abstractions
{
    // 解决不同项目/场景的 ID 生成策略不同的问题（外部给定、自增、GUID、雪花、复盘时稳定复现的可重复 ID……）
    /// <summary>
    /// Id分配器接口
    /// </summary>
    /// <typeparam name="TId">Id类型</typeparam>
    public interface IIdAllocator<out TId> 
    {
        TId New(); 
    }
}