using System;
using Core.Interfaces;
using Data.FishData;
using UnityEngine;

namespace GameLogic.FishLogic
{
    public enum EFishState
    {
        Swimming,
        Escaping,
        Hooked
    }
    /// <summary>
    /// 鱼类 AI 基类，提供方法接口，子类调用
    /// </summary>
    public abstract class BaseFish : MonoBehaviour,IEntity
    {
        protected BaseFishData _fishData;
        public string FishName => _fishData.fishName;
        public int FishValue => _fishData.fishValue; // 钱，这个或许不算运行的数据，理论上可以不放在这里
        public int ID => _fishInstanceID;
        
        // ----- 运行时数据 -----
        protected int _fishInstanceID => GetInstanceID();
        protected EFishState _fishState = EFishState.Swimming;
        protected bool _isEscaping = false;
        protected float _currentSpeed;
        protected int _facingDirection = 1; // 1=向右，-1=向左
        protected Transform _HookedFollowPosition = null; // 鱼被钩住后，跟随钩子位置
        
        private bool hasInit = false;
        
        // ----- 组件区 -----
        Collider2D _collider2D;
        
        
        // ----- 核心函数  ----
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(BaseFishData fishData, int startingDirection)
        {
            _fishData = fishData;
            _facingDirection = startingDirection;
            _collider2D = GetComponent<Collider2D>();
            OnInitialize(fishData);
            UpdateVisualDirection();

            hasInit = true;
        }
        

        
        
        // ----- 模板函数  -----
        
        /// <summary>
        /// 要在子类处理的初始化逻辑
        /// </summary>
        /// <param name="fishData"></param>
        public virtual void OnInitialize(BaseFishData fishData)
        {
            _currentSpeed = fishData.fishMoveSpeed;
        }
        public void UpdateLogic()
        {
            if (!hasInit) return;
            OnUpdate();
        }
        /// <summary>
        /// 更新逻辑 - 子类实现具体行为
        /// </summary>
        public abstract void OnUpdate();
        
        // ----- Action 函数 -----
        
        /// <summary>
        /// 水平移动 - 向自己的朝向方向移动
        /// </summary>
        protected virtual void HorizontalMove()
        {
            // 计算移动方向
            Vector3 moveDirection = Vector3.right * _facingDirection;
    
            // 计算移动距离（速度 * 时间）
            Vector3 movement = moveDirection * (_currentSpeed * Time.deltaTime);
    
            // 只保留水平移动，忽略Y轴变化
            movement.y = 0f;
    
            // 应用移动
            transform.position += movement;
            
        }

        public virtual void Hooked(Transform hookTransform)
        {
            _fishState = EFishState.Hooked;
            _HookedFollowPosition = hookTransform;
            _collider2D.enabled = false; // 关闭碰撞体，防止被墙壁等物体卡住
            // 其他被钩住的逻辑
        }
        
        // ----- Condition 函数 -----
        /// <summary>
        /// 检测和鱼钩的距离是否小于探测值
        /// </summary>
        protected virtual bool CheckHookCollision(Vector3 hookPosition)
        {
            float distance = Vector3.Distance(transform.position, hookPosition);
            return distance <= _fishData.detectionRadius;
        }
        
        /// <summary>
        /// 检查边界并调转方向
        /// </summary>
        protected void CheckBoundaryAndTurn()
        {
            _facingDirection = -_facingDirection;
        }

        /// <summary>
        /// 更新鱼的视觉朝向（翻转Sprite）
        /// </summary>
        protected void UpdateVisualDirection()
        {
            // 通过缩放X轴来翻转Sprite
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * _facingDirection;
            transform.localScale = scale;
        }
        
        /// <summary>
        /// 开始逃跑
        /// </summary>
        protected virtual void StartEscape()
        {
            _isEscaping = true;
        }
        
        // ----- 被动的事件函数 -----
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(_fishState == EFishState.Hooked) return;
            
            if (other.CompareTag("Wall"))
            {
                CheckBoundaryAndTurn();
                UpdateVisualDirection();
            }
        }

        
    }
}