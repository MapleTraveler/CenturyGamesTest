using System;
using Data;
using Data.FishingComponent;
using UnityEngine;

namespace GameLogic.FishingComponent
{
    public enum RodState
    {
        Idle,       // 空闲状态
        Sinking,    // 下沉状态
        Ascending,  // 上升状态
        SpeedUpAscending  // 加速上升状态
    }
    public class FishingLine : MonoBehaviour
    {
        private FishingLineData _data;
        public float MaxLength => _data.maxLength;
        // 运行时数据
        public float CurrentVerticalSpeed { get; private set; }
        public float CurrentDepth { get; private set; } // 当前深度
        public float MaxDepth { get; private set; }
        public RodState CurrentState { get; private set; }
        
        // 深度变化（currentIntDepth, maxIntLength）
        public event Action<int,int> OnDepthChanged;
        
        private bool _hasInit = false;
        
        private int _lastReportedDepthInt = int.MinValue;
        
        
        
        // 配置数据
        private float _sinkSpeed;
        private float _ascentSpeed;
        private float _accelerateAscentSpeed;

        public void Init(FishingLineData data, float sinkSpeed, float ascentSpeed, float accelerateAscentSpeed)
        {
            if(_hasInit) return;
            _data = data;
            _sinkSpeed = sinkSpeed;
            _ascentSpeed = ascentSpeed;
            _accelerateAscentSpeed = accelerateAscentSpeed;
            //OnDepthChanged += onDepthChanged;
                
            // 初始深度设为 0
            CurrentDepth = 0f;
            _lastReportedDepthInt = Mathf.RoundToInt(CurrentDepth);
            SetState(RodState.Sinking);
            
            // 通知初始值
            OnDepthChanged?.Invoke(_lastReportedDepthInt, Mathf.RoundToInt(MaxLength));
            
            _hasInit = true;
        }
        
        
        public void HandleVerticalMovement()
        {
            if(!_hasInit) return;
            float trueVerticalSpeed = CurrentVerticalSpeed * Time.deltaTime;
            CurrentDepth -= trueVerticalSpeed;
            transform.localPosition = new Vector3(transform.position.x, transform.position.y + trueVerticalSpeed, transform.position.z);
            MaxDepth = Mathf.Max(MaxDepth, CurrentDepth);
            // 只在整数深度变化时上报（避免每帧频繁触发 UI）
            int currentDepthInt = Mathf.RoundToInt(CurrentDepth);
            if (currentDepthInt != _lastReportedDepthInt)
            {
                _lastReportedDepthInt = currentDepthInt;
                OnDepthChanged?.Invoke(currentDepthInt, Mathf.RoundToInt(MaxLength));
            }
        }

        // TODO：缺少错误熔断，或许得在切换前加一步校验？
        public void SetState(RodState state)
        {
            CurrentState = state;
            switch (CurrentState)
            {
                case RodState.Idle:
                    CurrentVerticalSpeed = 0;
                    break;
                case RodState.Sinking:
                    CurrentVerticalSpeed = _sinkSpeed;
                    break;
                case RodState.Ascending:
                    CurrentVerticalSpeed = _ascentSpeed;
                    break;
                case RodState.SpeedUpAscending:
                    CurrentVerticalSpeed = _accelerateAscentSpeed;
                    break;
                default:
                    Debug.LogError("意料之外的类型：" + CurrentState);
                    break;
            }
        }
        public void ResetDepth()
        {
            CurrentDepth = 0f;
            MaxDepth = 0f;
            _lastReportedDepthInt = Mathf.RoundToInt(CurrentDepth);
            OnDepthChanged?.Invoke(_lastReportedDepthInt, Mathf.RoundToInt(MaxLength));
        }
    }
}