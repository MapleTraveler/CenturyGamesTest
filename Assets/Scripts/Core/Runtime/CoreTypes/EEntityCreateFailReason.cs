namespace Core.Runtime.CoreTypes
{
    public enum EEntityCreateFailReason
    {
        None,                   // 创建成功
        DataMissing,            // 数据缺失
        FactoryReturnedNull,    // 工厂返回了空对象
        DuplicateId,            // 重复的实体Id
        Unknown                 // 未知错误
    }
}