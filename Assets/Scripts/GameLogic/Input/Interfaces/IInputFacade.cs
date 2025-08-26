namespace GameLogic.Input.Interfaces
{
    /// <summary>
    /// 输入系统门面接口 - 门面模式的核心
    /// 目的：为上层提供统一的输入访问入口，隐藏底层复杂性
    /// 优势：
    /// 1. 解耦上层业务逻辑与具体输入实现
    /// 2. 统一管理不同游戏状态下的输入映射
    /// 3. 便于输入系统的整体替换和扩展
    /// </summary>
    public interface IInputFacade
    {
        /// <summary>
        /// 输入快照访问器 - 连续量数据的统一入口
        /// 如：拖拽增量、摇杆状态等需要每帧读取的数据
        /// </summary>
        IInputSnapShot Snapshot { get; }
        /// <summary>
        /// 上下文栈管理 - 推入新的输入上下文
        /// 用途：游戏状态切换时改变输入映射（如：主菜单 -> 游戏内 -> 暂停菜单）
        /// </summary>
        /// <param name="ctxName">上下文名称，可以是 "Gameplay" 或 "UI+Gameplay" 等组合</param>
        /// <param name="blocksLower">是否阻塞下层上下文（默认true，实现模态输入）</param>
        void PushContext(string ctxName, bool blocksLower = true);
        /// <summary>
        /// 弹出当前输入上下文，恢复到上一层
        /// 自动重新激活被阻塞的下层上下文
        /// </summary>
        void PopContext();
    }
}