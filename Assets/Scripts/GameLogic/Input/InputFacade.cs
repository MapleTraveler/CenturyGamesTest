using System;
using System.Collections.Generic;
using GameLogic.Input.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameLogic.Input
{
    /// <summary>
    /// 输入门面实现 - 整个输入系统的核心协调者
    /// 
    /// 架构体现：
    /// 1. 门面模式：统一输入访问入口，隐藏InputSystem复杂性
    /// 2. 上下文栈：管理不同游戏状态的输入映射切换
    /// 3. 命令模式：通过OnCommand事件分发复杂输入命令
    /// 
    /// 设计优势：
    /// - 业务层无需了解InputSystem细节
    /// - 支持动态的输入映射切换
    /// - 便于输入系统
    /// </summary>
    public class InputFacade : IInputFacade
    {
        private InputActionAsset actions;// TODO:这里未来应该是通过配表进行初始化的
        public IInputSnapShot Snapshot { get; }
        // —— 快照（连续量/保持态）——
        
        //public Vector2 LookDelta { get; }
        
        /// <summary>
        /// 命令事件 - 命令模式的分发中心
        /// 用于传递复杂的输入命令给上层系统
        /// </summary>
        public event Action<IInputCommand> OnCommand;

        
        // Action 引用（单一事实源）
        private InputAction _drag;
        
        
        public void Init(InputActionAsset actionAsset)
        {
            actions = actionAsset;
            _drag = actions.FindAction("Gameplay/Drag",true);
            
            _drag.Enable();
        }

        // TODO: 输入更新的时序是否统一？
        private void OnUpdate()
        {
            
        }

        
        // —— 上下文栈实现 - 支持嵌套的输入状态管理 ——
        /// <summary>
        /// 上下文栈 - 存储输入状态的层级结构
        /// 元组含义：(上下文名称, 是否阻塞下层)
        /// 
        /// 使用示例：
        /// 1. 游戏开始：Push("Gameplay")
        /// 2. 打开背包：Push("UI", true) - 阻塞Gameplay
        /// 3. 关闭背包：Pop() - 恢复Gameplay
        /// </summary>
        readonly Stack<(string name,bool blocks)> _contexts = new Stack<(string name,bool blocks)>();
        

        public void PushContext(string ctxName, bool blocksLower = true)
        {
            if (_contexts.Count > 0 && blocksLower)
            {
                SetMapEnabled(_contexts.Peek().name, false);
            }
            _contexts.Push((ctxName, blocksLower));
            SetMapEnabled(ctxName, true);
            SyncCursor(ctxName);
        }

        public void PopContext()
        {
            var top = _contexts.Pop();
            SetMapEnabled(top.name, false);
            if (_contexts.Count > 0)
            {
                var (n, _) = _contexts.Peek();
                SetMapEnabled(n, true);
                SyncCursor(n);
            }
        }

        void SetMapEnabled(string mapName, bool mapEnabled)
        {
            foreach (var m in mapName.Split('+'))
            {
                var map = actions.FindActionMap(m.Trim(), true);
                if (mapEnabled)
                {
                    map.Enable();
                }
                else
                {
                    map.Disable();
                }
            }    
        }

        void SyncCursor(string context)
        {
            bool gameplay = context.Contains("Gameplay");
            Cursor.lockState = gameplay ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = gameplay;
        }
        
    }
}