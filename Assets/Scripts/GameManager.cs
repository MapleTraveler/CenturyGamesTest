using UnityEngine;
using System;
using Data;
using GameLogic;
using GameLogic.FishingComponent;
using GameLogic.Input;
using Manager;

public class GameManager : MonoBehaviour
{
    public GlobalConfigData globalConfig;
    public FishRod fishRod;
    // ----- Systems -----
    private InputFacade _inputFacade;
    private GameLoopManager _gameLoopManager;
    private FishManager _fishManager;
    private VerticalFishSpawner _verticalFishSpawner;
    private MapBoundConfigData _mapBoundConfigData;
    
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
        _gameLoopManager.StartGameLoop();
    }

    private void Update()
    {
        if(!_hasInit) return;
        _inputFacade.OnUpdate();
        //fishRod.OnUpdate();
        _gameLoopManager.UpdateGameLoop();
    }

    public void Init()
    {
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
            rightSpawnX = _rightBound.position.x
        };
        
        // 输入层
        _inputFacade = new InputFacade(globalConfig.inputActionAsset);
        
        // 鱼类管理器和生成器
        _fishManager = new FishManager(globalConfig.fishDataDict);
        _verticalFishSpawner = new VerticalFishSpawner(_mapBoundConfigData, _fishManager, new WeightedByDepthPolicy(),
            2f, 2f, globalConfig.fishCount);
        
        // 游戏循环管理器
        _gameLoopManager = new GameLoopManager(_fishManager, _verticalFishSpawner);
        
        fishRod.Init(_inputFacade,globalConfig.fishRodConfig);

        _hasInit = true;
    }
}