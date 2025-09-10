using System;
using Data;
using UI;
using UnityEngine;

namespace UI.Controller
{
    public class FinishPanelController
    {
        private FinishPanel _finishPanel;
        private GameObject _gridPrefab;

        public FinishPanelController(FinishPanel finishPanel,GameObject gridPrefab)
        {
            _finishPanel = finishPanel;
            _gridPrefab = gridPrefab;
            _finishPanel.SetGridPrefab(_gridPrefab);
        }

        /// <summary>
        /// 展示结算面板并绑定按钮事件
        /// </summary>
        public void ShowSettlement(
            TotalSettlementData data, 
            Action onContinue,
            Action onExit)
        {
            _finishPanel.Show(data);

            _finishPanel.continueButton.onClick.RemoveAllListeners();
            _finishPanel.continueButton.onClick.AddListener(() => onContinue?.Invoke());

            _finishPanel.exitButton.onClick.RemoveAllListeners();
            _finishPanel.exitButton.onClick.AddListener(() => onExit?.Invoke());
        }

        public void Hide()
        {
            _finishPanel.Hide();
        }
    }
}