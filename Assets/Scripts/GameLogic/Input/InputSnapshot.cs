using System.Collections.Generic;
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
    public class InputSnapshot
    {
        public Vector2 DragDelta { get; private set; }
        private readonly Dictionary<string, float> _pressTime = new();
        private float bufferMs = 150f;
        
        public void UpdateSnapshot(InputActionAsset actions)
        {
            var drag = actions.FindAction("Gameplay/Drag");
            DragDelta = drag?.ReadValue<Vector2>() ?? Vector2.zero;
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