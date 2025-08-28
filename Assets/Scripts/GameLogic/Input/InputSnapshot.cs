using System.Collections.Generic;
using GameLogic.Input.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameLogic.Input
{
    /// <summary>
    /// 输入快照实现 - 负责缓存和管理输入状态数据
    /// 
    /// 设计原理：
    /// 1. 将输入读取与业务逻辑解耦
    /// 2. 提供按键缓冲机制，解决时序问题
    /// 3. 统一的数据访问接口，便于测试Mock
    /// </summary>
    public class InputSnapshot : IInputSnapShot
    {
        public Vector2 DragDelta { get; private set; }
        private readonly Dictionary<string, float> _pressTime = new();
        private float bufferMs = 150f;
        
        
        private InputAction _dragAction;
        public InputSnapshot(InputActionAsset actions)
        {
            DragDelta = Vector2.zero;
            // 初始化Action引用
            _dragAction = actions.FindAction("Gameplay/Drag");
            
        }
        
        public void UpdateSnapshot()
        {
            DragDelta = _dragAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }
    
        /// <summary>
        /// 记录按键按下事件 - 由InputFacade调用
        /// </summary>
        public void RecordPress(string actionName)
        {
            _pressTime[actionName] = Time.unscaledTime;
        }
        /// <summary>
        /// 清理过期的按键记录，避免内存泄漏
        /// </summary>
        private void CleanExpiredPresses()
        {
            var currentTime = Time.unscaledTime;
            var expiredKeys = new List<string>();
        
            foreach (var kvp in _pressTime)
            {
                if ((currentTime - kvp.Value) * 1000f > bufferMs)
                {
                    expiredKeys.Add(kvp.Key);
                }
            }
        
            foreach (var key in expiredKeys)
            {
                _pressTime.Remove(key);
            }
        }
        // TODO:按键按下事件的缓冲消费，这一块还没有完全理解
        public bool ConsumePressed(string actionName)
        {
            if (!_pressTime.TryGetValue(actionName, out var t)) return false;
            bool hit = (Time.unscaledTime - t) * 1000f <= bufferMs;
            _pressTime.Remove(actionName);
            return hit;
        }
        
    }
}