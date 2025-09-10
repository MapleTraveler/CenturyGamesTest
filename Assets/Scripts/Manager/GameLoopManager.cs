using System;
using Data;
using GameLogic.FishingComponent;
using UI.Controller;
using UnityEngine;

namespace Manager
{
    public class GameLoopManager : IDisposable
    {
        private FishManager _fishManager;
        private VerticalFishSpawner _verticalFishSpawner;
        private FishRod _fishRod;

        // ----- UI -----
        private PlayingPanelController _playingPanelController;
        private FinishPanelController _finishPanelController;
        private StartPanelController _startPanelController;
        private TotalSettlementData _totalSettlementData;


        public GameLoopManager(FishManager fishManager, VerticalFishSpawner verticalFishSpawner, FishRod fishRod,
            FinishPanelController finishPanelController,StartPanelController startPanelController, PlayingPanelController playingPanelController = null)
        {
            _fishManager = fishManager;
            _verticalFishSpawner = verticalFishSpawner;
            _fishRod = fishRod;
            _finishPanelController = finishPanelController;
            _startPanelController = startPanelController;
            _playingPanelController = playingPanelController;
            
            _fishRod.SetOnFishEndEvent(StopGameLoop);
        }
        public void EnterStartPanel()
        {
            _startPanelController.ShowSettlement(OnStartClicked, OnExitClicked);
        }

        public void StartGameLoop()
        {
            
            _fishRod.Reset();
            _fishManager.Reset(); 
            _verticalFishSpawner.FillDepthPassOnce();
            _finishPanelController.Hide();
            

        }

        public void UpdateGameLoop()
        {
            _fishRod.OnUpdate();
            _fishManager.OnUpdate();
        }

        public void StopGameLoop()
        {
            _totalSettlementData = new TotalSettlementData(
                _fishRod.GetSettlementData(),
                _fishManager.GetSettlementData()
            );
            _finishPanelController.ShowSettlement(
                _totalSettlementData,
                OnContinueClicked,
                OnExitClicked
            );
            
        }
        
        public void OnStartClicked()
        {
            _startPanelController.Hide();
            StartGameLoop();
        }
        public void OnContinueClicked()
        {
            StartGameLoop();
        }

        public void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            Debug.Log("退出游戏");
        }

        public void Dispose()
        {
            _playingPanelController?.Dispose();
            _playingPanelController = null;
        }
        
    }
}