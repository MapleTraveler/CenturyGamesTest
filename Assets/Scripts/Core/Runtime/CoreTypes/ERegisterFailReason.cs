namespace Core.Runtime.CoreTypes
{
    public enum ERegisterFailReason 
    { 
        None,                   // 注册成功
        NullEntity,             // 实体为空
        DuplicateId             // 重复的实体Id
    }
}