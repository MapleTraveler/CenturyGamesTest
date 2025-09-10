using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StartPanel : PanelBase
    {
        public Button startBtn;
        public Button exitBtn;
        private CanvasGroup canvasGroup;
        protected override void Awake()
        {
            base.Awake();
            startBtn = GetElement<Button>("StartBtn");
            exitBtn = GetElement<Button>("ExitBtn");
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public override void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public override void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}