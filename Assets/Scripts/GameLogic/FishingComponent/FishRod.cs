using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using Data.FishingComponent;
using GameLogic.Input.Interfaces;
using UnityEngine;

namespace GameLogic.FishingComponent
{
    public class FishRod : MonoBehaviour
    {
        MapBoundConfigData _mapBoundConfigData;
        public FishHook FishHook => _fishHook;
        public FishingLine FishingLine => _fishingLine;
        public FishSinker FishSinker => _fishSinker;

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

        // ----- Action -----
        public event Action OnFishEnd; 
        public event Func<int, Transform, bool> OnFishCaught; // 捕获鱼类事件，参数为鱼的ID和Transform


        public void Init(IInputFacade inputFacade, FishRodConfig fishRodConfig, MapBoundConfigData mapBoundConfigData,
            Vector2 fishHookStartPos,Func<int, Transform, bool> onFishCaught)
        {
            if (_hasInit) return;
            _inputFacade = inputFacade;
            _fishRodConfig = fishRodConfig;
            _mapBoundConfigData = mapBoundConfigData;
            OnFishCaught += onFishCaught;
            _fishHook = GetComponentInChildren<FishHook>();
            _fishingLine = GetComponentInChildren<FishingLine>();
            _fishSinker = GetComponentInChildren<FishSinker>();

            _fishHook.Init(_fishRodConfig.fishHookData,fishHookStartPos, TryHookFish, _fishingLine.SetState,
                _mapBoundConfigData.hookMinX, _mapBoundConfigData.hookMaxX);
            _fishingLine.Init(_fishRodConfig.fishingLineData, _fishRodConfig.sinkSpeed, _fishRodConfig.ascentSpeed,
                _fishRodConfig.accelerateAscentSpeed);
            _fishSinker.Initialize(_fishRodConfig.fishSinkerData, _fishHook.UnlockHook);
            _hasInit = true;
        }

        public void OnUpdate()
        {
            if (!_hasInit) return;
            _fishHook.HorizontalMovement(_inputFacade.Snapshot.DragDelta * Time.deltaTime);

            // 处理钓线（鱼钩水平移动）状态机
            switch (_fishingLine.CurrentState)
            {
                case RodState.Idle:
                    break;
                case RodState.Sinking:
                    if (_fishingLine.CurrentDepth >= _fishingLine.MaxLength)
                    {
                        _fishHook.UnlockHook();
                        _fishSinker.ShieldBreak();
                        _fishingLine.SetState(RodState.Ascending);
                    }

                    break;
                case RodState.Ascending:
                    if (_fishingLine.CurrentDepth <= 0)
                    {
                        _fishingLine.SetState(RodState.Idle);
                        OnFishEnd?.Invoke();
                    }
                        
                    if (_fishHook.IsFull)
                        _fishingLine.SetState(RodState.SpeedUpAscending);

                    break;
                case RodState.SpeedUpAscending:
                    if (_fishingLine.CurrentDepth <= 0)
                    {
                        _fishingLine.SetState(RodState.Idle);
                        OnFishEnd?.Invoke();
                    }
                        

                    break;
                default:
                    Debug.LogError("意料之外的类型：" + _fishingLine.CurrentState);
                    break;
            }

            _fishingLine.HandleVerticalMovement();
        }

        public bool TryHookFish(int id, Transform hookTransform)
        {
            bool? flag = OnFishCaught?.Invoke(id, hookTransform);
            //Debug.Log("Rod: Try Hook Fish " + id + ", Result: " + flag);
            return flag != null && flag.Value;
        }

        // 结算
        public RodSettlementData GetSettlementData()
        {
            return new RodSettlementData { maxDepth = Mathf.RoundToInt(FishingLine.MaxDepth) };
        }

        public void SetOnFishEndEvent(Action onFishEnd)
        {
            OnFishEnd += onFishEnd;
        }
        public void Reset()
        {
            if (!_hasInit) return;

            _fishingLine.ResetDepth();
            _fishingLine.SetState(RodState.Sinking);

            _fishHook.ResetHook();
            _fishSinker.ResetSinker();
        }
    }
}