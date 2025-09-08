namespace Core.Interfaces
{
    /// <summary>
    /// 一个实体应该具有的基本属性和行为
    /// 1. 唯一标识符（ID）
    /// 2. 脱离Mono但希望有自己的生命周期情况下的模板方法
    /// 3. 这些都通过父类接口进行组合
    /// </summary>
    public interface IEntity : IEntityProperties
    {
        
    }
}