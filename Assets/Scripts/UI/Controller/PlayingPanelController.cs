using System;
using System.Collections.Generic;
using GameLogic.FishingComponent;
using UnityEngine;

namespace UI.Controller
{
    // Controller：订阅 Model 事件并推送到 View
    public class PlayingPanelController : IDisposable
    {
        private readonly PlayingPanel _view;
        private readonly FishSinker _sinker;
        private readonly FishingLine _fishingLine;
        private readonly FishHook _fishHook;

        // 保存最新三项数据，任何变化时一起刷新 View
        private KeyValuePair<int,int> _shieldPair;
        private KeyValuePair<int,int> _depthPair;
        private KeyValuePair<int,int> _fishPair;

        public PlayingPanelController(PlayingPanel view, FishSinker sinker, FishingLine fishingLine, FishHook fishHook)
        {
            _view = view;
            _sinker = sinker;
            _fishingLine = fishingLine;
            _fishHook = fishHook;

            // 初始化当前值
            if (_sinker != null)
                _shieldPair = new KeyValuePair<int,int>(_sinker.CurrentShieldCount, _sinker.MaxShieldCount);
            else
                _shieldPair = new KeyValuePair<int,int>(0,0);

            if (_fishingLine != null)
                _depthPair = new KeyValuePair<int,int>(Mathf.RoundToInt(_fishingLine.CurrentDepth), Mathf.RoundToInt(_fishingLine.MaxLength));
            else
                _depthPair = new KeyValuePair<int,int>(0,0);

            if (_fishHook != null)
                _fishPair = new KeyValuePair<int,int>(_fishHook.currentHookedFishCount, _fishHook.MaxHookedFishCount);
            else
                _fishPair = new KeyValuePair<int,int>(0,0);

            // 订阅事件
            if (_sinker != null) _sinker.OnShieldChangedUIEvent += OnShieldChangedUIEvent;
            if (_fishingLine != null) _fishingLine.OnDepthChanged += OnDepthChanged;
            if (_fishHook != null) _fishHook.OnHookCountChanged += OnHookCountChanged;

            // 首次刷新视图
            _view?.UpdateRodDataView(_shieldPair, _depthPair, _fishPair);
        }

        private void OnShieldChangedUIEvent(int current, int max)
        {
            _shieldPair = new KeyValuePair<int,int>(current, max);
            RefreshView();
        }

        private void OnDepthChanged(int currentDepthInt, int maxLengthInt)
        {
            _depthPair = new KeyValuePair<int,int>(currentDepthInt, maxLengthInt);
            RefreshView();
        }

        private void OnHookCountChanged(int current, int max)
        {
            _fishPair = new KeyValuePair<int,int>(current, max);
            RefreshView();
        }

        private void RefreshView()
        {
            _view?.UpdateRodDataView(_shieldPair, _depthPair, _fishPair);
        }

        // 调用方在销毁时应调用 Dispose 以取消订阅
        public void Dispose()
        {
            if (_sinker != null) _sinker.OnShieldChangedUIEvent -= OnShieldChangedUIEvent;
            if (_fishingLine != null) _fishingLine.OnDepthChanged -= OnDepthChanged;
            if (_fishHook != null) _fishHook.OnHookCountChanged -= OnHookCountChanged;
        }
    }
}
