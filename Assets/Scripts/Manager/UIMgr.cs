using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Manager
{
    /// <summary>
    /// 使用VContainer的UI管理器
    /// </summary>
    public class UIMgr
    {
        public static UIMgr Instance { get; private set; }
        private bool m_Initialized;
        private bool m_Initializing;

        // Canvas对象的位置
        public RectTransform RootCanvas { get; private set; }
        public Camera UICamera { get; private set; }
        // Canvas上显示不同面板的层级位置
        private readonly List<Transform> m_Layers = new List<Transform>();
        // Canvas上已经加载出的面板
        private readonly Dictionary<string, PanelBase> m_Panel = new Dictionary<string, PanelBase>();
        private readonly Dictionary<string, int> m_PanelLayer = new Dictionary<string, int>();
        private readonly Dictionary<string, GameObject> m_Prefab = new Dictionary<string, GameObject>();



        public UIMgr()
        {
            Instance = this;
        }

        /// <summary>
        /// 从Addressables里加载并打开面板
        /// </summary>
        /// <param name="layer">面板在Canvas中的层级</param>
        public T ShowPanel<T>(int layer = -1) where T : PanelBase
        {
            T panel = ShowPanelWithoutPanelInit<T>(layer);
            panel.Show();
            return panel;
        }

        public TPanel ShowPanel<TPanel, TParam>(TParam param, int layer = -1) where TPanel : PanelBase<TParam>
        {
            TPanel panel =  ShowPanelWithoutPanelInit<TPanel>(layer);
            panel.Show(param);
            return panel;
        }

        private T ShowPanelWithoutPanelInit<T>(int layer) where T : PanelBase
        {
            if (!m_Initialized) return null;

            // 检查面板是否已经加载
            if (GetPanel<T>() != null)
            {
                Debug.LogWarning($"面板{typeof(T)}已经加载过了");
                return GetPanel<T>();
            }
            // 实例化面板
            GameObject panelPrefab = Resources.Load<GameObject>($"{GlobalSetting.UI_PREFAB_PATH}{nameof(T)}");
            T panel = panelPrefab.GetComponent<T>();
            
            // 确定层级
            layer = layer == -1 ? panel.DefaultLayer : layer;
            if (layer < 0 || layer >= m_Layers.Count)
            {
                Debug.LogWarning($"面板{typeof(T)}层级有误");
                layer = 0;
            }
            // 实例化面板并设置父对象
            GameObject obj = Object.Instantiate(panelPrefab, m_Layers[layer], true);
            obj.name = typeof(T).Name;
            m_Prefab.Add(typeof(T).Name, obj);

            
            // 设置初始位置
            RectTransform trans = obj.GetComponent<RectTransform>();
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            trans.anchoredPosition3D = Vector3.zero;
            trans.anchorMax = trans.anchorMin = Vector2.one * 0.5f;
            //trans.offsetMax = trans.offsetMin = Vector2.zero;
            trans.pivot = Vector2.one * 0.5f;
            

            // 记录面板
            m_Panel.Add(panel.name, panel);
            m_PanelLayer.Add(panel.name, layer);
            return panel;
        }

        /// <summary>
        /// 获取面板
        /// </summary>
        /// <param name="panelName">面板对象名称 null表示使用类型名</param>
        /// <returns>未找到面板时返回null</returns>
        public T GetPanel<T>(string panelName = null) where T : PanelBase
        {
            string key = panelName ?? typeof(T).Name;
            m_Panel.TryGetValue(key, out PanelBase panel);
            return panel as T;
        }

        /// <summary>
        /// 关闭并销毁面板
        /// </summary>
        public void HidePanel<T>()
        {
            HidePanel(typeof(T).Name);
        }
        public void HidePanel(string key)
        {
            if (!m_Panel.ContainsKey(key))
                return;
            m_Panel[key].Hide();

            if (m_Panel[key].gameObject)
                // TODO:改成淡入淡出，不再销毁
                Object.Destroy(m_Panel[key].gameObject);
            m_Panel.Remove(key);
            m_PanelLayer.Remove(key);
            
            // TODO:梳理一下UI的加载和卸载
            m_Prefab.Remove(key);
        }
        public void HideAll(params string[] except)
        {
            HashSet<string> temp = new HashSet<string>(m_Panel.Keys.Except(except));
            foreach (string panel in temp)
                HidePanel(panel);
        }
        public void HideAllExceptCurtain()
        {
            HashSet<string> temp = new HashSet<string>(m_Panel.Select(pair => pair.Key));
            foreach (string panel in temp)
                HidePanel(panel);
        }

        public bool IsTopPanel<T>() => IsTopPanel(typeof(T).Name);
        public bool IsTopPanel(string key)
        {
            return m_PanelLayer.TryGetValue(key, out int layer) && m_PanelLayer.Values.Max() == layer;
        }
        // 等待被外部调用
        public void InitializeAsync()
        {
            if (m_Initialized)
                return;
            
            GameObject root = Resources.Load<GameObject>($"{GlobalSetting.UI_PREFAB_PATH}UIRoot");

            // 创建UIRoot
            GameObject obj = Object.Instantiate(root);
            Object.DontDestroyOnLoad(obj);

            // 记录位置
            RootCanvas = (RectTransform)obj.transform.Find("RootCanvas");
            if (RootCanvas.childCount == 0)
                m_Layers.Add(RootCanvas);
            else
                for (int i = 0; i < RootCanvas.childCount; i++)
                    m_Layers.Add(RootCanvas.GetChild(i));

            // 记录相机
            UICamera = GameObject.Find("UICamera").GetComponent<Camera>();
            RootCanvas.GetComponent<Canvas>().worldCamera = UICamera;

            
            m_Initialized = true;
        }
    }
}
