using UnityEngine;
using System;
using GameLogic;
using GameLogic.FishingComponent;
using GameLogic.Input;

public class GameManager : MonoBehaviour
{
    public GlobalConfig globalConfig;
    public FishRod fishRod;
    // --- Systems ---
    private InputFacade _inputFacade;
    
    private bool _hasInit = false;
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        _inputFacade.EnableActions();
    }

    private void Update()
    {
        if(!_hasInit) return;
        _inputFacade.OnUpdate();
        fishRod.OnUpdate();
    }

    public void Init()
    {
        _inputFacade = new InputFacade(globalConfig.inputActionAsset);
        fishRod.Init(_inputFacade,globalConfig.fishRodConfig);

        _hasInit = true;
    }
}