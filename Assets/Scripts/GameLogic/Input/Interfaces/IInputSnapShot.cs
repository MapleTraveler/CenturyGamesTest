using UnityEngine;

namespace GameLogic.Input.Interfaces
{
    /// <summary>
    /// 输入快照接口 - 连续量数据的标准化访问
    /// 设计目的：
    /// 1. 将"连续量"与"离散事件"分离，避免混乱
    /// 2. 提供统一的数据读取接口，便于测试和Mock
    /// 3. 支持按键缓冲机制，提升游戏手感
    /// </summary>
    public interface IInputSnapShot
    {
        /// <summary>
        /// 拖拽增量 - 当前帧相对上一帧的位移
        /// 适用于：相机控制、物体拖拽、UI滑动等
        /// </summary>
        Vector2 DragDelta { get; }
        /// <summary>
        /// 消费式按键检测 - 带缓冲的离散事件处理
        /// 设计原因：
        /// 1. 解决输入与逻辑更新时序不同步问题
        /// 2. 提供150ms的按键缓冲，提升操作手感
        /// 3. 消费机制防止重复触发
        /// </summary>
        /// <param name="actionName">Action名称，如 "Cast"、"Jump" 等</param>
        /// <returns>是否在缓冲时间内被按下过</returns>
        bool ConsumePressed(string actionName);
    }
    
}