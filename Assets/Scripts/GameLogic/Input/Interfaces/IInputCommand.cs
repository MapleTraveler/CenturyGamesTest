namespace GameLogic.Input.Interfaces
{
    /// <summary>
    /// 输入命令接口 - 命令模式的核心
    /// 设计目的：
    /// 1. 将复杂输入行为封装为可执行的命令对象
    /// 2. 支持命令的记录、重放、撤销等高级功能
    /// 3. 便于实现组合键、手势识别等复杂输入
    /// 
    /// 使用场景：
    /// - 技能释放：CastSpellCommand(spellId, targetPos)
    /// - 手势识别：SwipeGestureCommand(direction, strength)
    /// - 组合操作：ComboAttackCommand(sequence)
    /// </summary>
    public interface IInputCommand
    {
        
    }
    // ----- 各种Command -----
}