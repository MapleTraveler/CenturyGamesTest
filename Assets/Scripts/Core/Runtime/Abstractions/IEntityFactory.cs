namespace Core.Runtime.Abstractions
{
    /// <summary>
    /// 实体工厂接口
    /// </summary>
    /// <typeparam name="TDataKey">数据类Id的数据类型</typeparam>
    /// <typeparam name="TData">数据类的类型</typeparam>
    /// <typeparam name="TEntity">实体类型</typeparam>
    /// <typeparam name="TEntityKey">实体Id的数据类型</typeparam>
    public interface IEntityFactory<in TDataKey, in TData, TEntity, out TEntityKey>
    {
        TEntity Create(TDataKey key, TData data, object context = null);
        void Destroy(TEntity entity);
        TEntityKey GetId(TEntity entity);
    }
}