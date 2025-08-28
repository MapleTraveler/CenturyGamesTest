using Data;
using Data.FishingComponent;
using GameLogic.Input.Interfaces;
using UnityEngine;

namespace GameLogic.FishingComponent
{
    public class FishRod : MonoBehaviour
    {
        // ----- Config -----
        private FishRodConfig _fishRodConfig;
        // ----- Input -----
        private IInputFacade _inputFacade;
        
        
        // ----- Components -----
        private FishHook _fishHook;
        private FishingLine _fishingLine;
        private FishSinker _fishSinker;
        
        // ----- State -----
        private bool _hasInit = false;
        public void Init(IInputFacade inputFacade,FishRodConfig fishRodConfig)
        {
            if (_hasInit) return;
            _inputFacade = inputFacade;
            _fishRodConfig = fishRodConfig;
            _fishHook = GetComponentInChildren<FishHook>();
            _fishingLine = GetComponentInChildren<FishingLine>();
            _fishSinker = GetComponentInChildren<FishSinker>();
            
            _fishingLine.Init(_fishRodConfig.fishingLineData,_fishRodConfig.sinkSpeed,_fishRodConfig.ascentSpeed,_fishRodConfig.accelerateAscentSpeed);
            _hasInit = true;
        }

        public void OnUpdate()
        {
            if(!_hasInit) return;
            _fishHook.HorizontalMovement(_inputFacade.Snapshot.DragDelta * Time.deltaTime);
            
            // 处理钓线（鱼钩水平移动）状态机
            switch (_fishingLine.CurrentState)
            {
                case RodState.Idle:
                    break;
                case RodState.Sinking:
                    // TODO：差一个护盾破碎后碰到鱼的状态检测
                    if(_fishingLine.CurrentDepth >= _fishingLine.MaxLength)
                        _fishingLine.SetState(RodState.Ascending);
                    break;
                case RodState.Ascending:
                    // TODO：差鱼钓满后的加速转换
                    if(_fishingLine.CurrentDepth <= 0)
                        _fishingLine.SetState(RodState.Idle);
                    
                    break;
                case RodState.SpeedUpAscending:
                    if(_fishingLine.CurrentDepth <= 0)
                        _fishingLine.SetState(RodState.Idle);
                    
                    break;
                default:
                    Debug.LogError("意料之外的类型：" + _fishingLine.CurrentState);
                    break;
            }
            _fishingLine.HandleVerticalMovement();
        }
        
    }
}