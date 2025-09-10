using UnityEngine;
using System;
using Data;
using GameLogic;
using GameLogic.FishingComponent;
using GameLogic.Input;
using Manager;
using UI;
using UI.Controller;

public class GameManager : MonoBehaviour
{
    public GlobalConfigData globalConfig;
    public FishRod fishRod;
    public PlayingPanel playingPanel;
    public FinishPanel finishPanel;
    public StartPanel startPanel;
    // ----- Systems -----
    private InputFacade _inputFacade;
    private GameLoopManager _gameLoopManager;
    private PlayingPanelController _playingPanelController;
    private FinishPanelController _finishPanelController;
    private StartPanelController _startPanelController;
    
    private FishManager _fishManager;
    private VerticalFishSpawner _verticalFishSpawner;
    private MapBoundConfigData _mapBoundConfigData;
    private GameObject _fishUICellPrefab;
    
    // ----- Temp -----
    Transform _topBound;
    Transform _bottomBound;
    Transform _leftBound;
    Transform _rightBound;
    private bool _hasInit = false;

    private void Start()
    {
        Init();
        _inputFacade.EnableActions();
        _gameLoopManager.EnterStartPanel();
        
    }

    private void Update()
    {
        if(!_hasInit) return;
        _inputFacade.OnUpdate();
        _gameLoopManager.UpdateGameLoop();
    }

    public void Init()
    {
        if (_hasInit) return;
        _bottomBound = GameObject.Find("BottomBound").transform;
        _topBound = GameObject.Find("TopBound").transform;
        _leftBound = GameObject.Find("LeftBound").transform;
        _rightBound = GameObject.Find("RightBound").transform;
        // 地图边界数据
        _mapBoundConfigData = new MapBoundConfigData
        {
            topBoundary = _topBound.position.y,
            bottomBoundary = _bottomBound.position.y,
            leftSpawnX = _leftBound.position.x,
            rightSpawnX = _rightBound.position.x,
            hookMinX = _leftBound.position.x,
            hookMaxX = _rightBound.position.x
        };
        
        // 输入层
        _inputFacade = new InputFacade(globalConfig.inputActionAsset);
        // 鱼类管理器和生成器
        _fishManager = new FishManager(globalConfig.fishDataDict);
        _verticalFishSpawner = new VerticalFishSpawner(_mapBoundConfigData, _fishManager, new WeightedByDepthPolicy(),
            2f, 2f, 100);
        // 鱼竿
        fishRod.Init(_inputFacade,globalConfig.fishRodConfig,_mapBoundConfigData,globalConfig.fishHookStartPos,_fishManager.TryHookFish);
        
        // UI
        _fishUICellPrefab = Resources.Load<GameObject>($"{GlobalSetting.UI_PREFAB_PATH}FishUICell");
        _playingPanelController = new PlayingPanelController(playingPanel,fishRod.FishSinker,fishRod.FishingLine,fishRod.FishHook);
        _finishPanelController = new FinishPanelController(finishPanel,_fishUICellPrefab);
        _startPanelController = new StartPanelController(startPanel);
        // 游戏循环管理器
        _gameLoopManager = new GameLoopManager(_fishManager, _verticalFishSpawner,fishRod,_finishPanelController,_startPanelController);
        
        

        _hasInit = true;
    }
    
}