using System.Collections.Generic;
using TMPro;

namespace UI
{
    public class PlayingPanel : PanelBase
    {
        TextMeshProUGUI _shieldValueText; 
        TextMeshProUGUI _currentDepthText;
        TextMeshProUGUI _currentFishCountText;
        protected override void Awake()
        {
            base.Awake();
            _shieldValueText = GetElement<TextMeshProUGUI>("SinkerText");
            _currentDepthText = GetElement<TextMeshProUGUI>("LineText");
            _currentFishCountText = GetElement<TextMeshProUGUI>("HookText");

        }

        public override void Show()
        {
            
        }

        public override void Hide()
        {
            
        }

        public void UpdateRodDataView(KeyValuePair<int,int> shieldValue, KeyValuePair<int,int> currentDepth, KeyValuePair<int,int> currentFishCount)
        {
            // 简单的 UI 更新方法，由 Controller 调用
            if (_shieldValueText != null)
                _shieldValueText.text = $"{shieldValue.Key}/{shieldValue.Value}";
            if (_currentDepthText != null)
                _currentDepthText.text = $"{currentDepth.Key}/{currentDepth.Value}";
            if (_currentFishCountText != null)
                _currentFishCountText.text = $"{currentFishCount.Key}/{currentFishCount.Value}";
        }
    }
}