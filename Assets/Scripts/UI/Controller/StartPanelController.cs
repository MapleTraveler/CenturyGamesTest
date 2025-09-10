using System;

namespace UI.Controller
{
    public class StartPanelController
    {
        private StartPanel _startPanel;

        public StartPanelController(StartPanel startPanel)
        {
            _startPanel = startPanel;
        }

        /// <summary>
        /// 展示结算面板并绑定按钮事件
        /// </summary>
        public void ShowSettlement(
            Action onStart,
            Action onExit)
        {
            _startPanel.Show();

            _startPanel.startBtn.onClick.RemoveAllListeners();
            _startPanel.startBtn.onClick.AddListener(() => onStart?.Invoke());

            _startPanel.exitBtn.onClick.RemoveAllListeners();
            _startPanel.exitBtn.onClick.AddListener(() => onExit?.Invoke());
        }

        public void Hide()
        {
            _startPanel.Hide();
        }
    }
}