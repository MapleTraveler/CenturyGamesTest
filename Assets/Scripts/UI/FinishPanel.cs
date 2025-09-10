// UI/FinishPanel.cs

using System.Collections.Generic;
using System.Linq;
using Data;
using Data.FishData;
using GameLogic;
using Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FinishPanel : PanelBase<TotalSettlementData>
    {
        public Transform fishListRoot;
        public GameObject fishItemPrefab;
        public TextMeshProUGUI totalValueText;
        public TextMeshProUGUI depthText;
        public Button continueButton;
        public Button exitButton;
        public CanvasGroup canvasGroup;
        
        
        private List<FishUICell> cells = new();

        protected override void Awake()
        {
            base.Awake();
            fishListRoot = GetElement<GridLayoutGroup>("HookedFishesGroup").transform;
            totalValueText = GetElement<TextMeshProUGUI>("TotalValueText");
            depthText = GetElement<TextMeshProUGUI>("DepthText");
            continueButton = GetElement<Button>("ContinueBtn");
            exitButton = GetElement<Button>("ExitBtn");
            canvasGroup = GetComponent<CanvasGroup>();


        }

        public override void Show(TotalSettlementData data)
        {
            // 清空旧列表
            foreach (Transform child in fishListRoot)
                Destroy(child.gameObject);
            cells.Clear();
            depthText.text = $"下潜深度：{data.RodData.maxDepth}米";
            // 展示每种鱼
            foreach (var kv in data.FishData.caughtFishData)
            {
                var item = Instantiate(fishItemPrefab, fishListRoot);
                FishUICell cell = item.GetComponent<FishUICell>();
                cell.SetData(kv.Key.fishSprite,kv.Key.fishValue,kv.Value);
            }

            totalValueText.text = $"总价值: {data.FishData.totalValue}";
            
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void SetGridPrefab(GameObject prefab)
        {
            fishItemPrefab = prefab;
        }
        
        public override void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}