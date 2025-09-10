using System;
using System.Collections;
using System.Collections.Generic;
using Data.FishingComponent;
using UnityEngine;

namespace GameLogic.FishingComponent
{
    public class FishSinker : MonoBehaviour
    {
        private FishSinkerData _fishSinkerData;
        public event Action<int,int> OnShieldChangedUIEvent; // 护盾破碎事件，参数为当前剩余护盾数量
        //public event Action<float> OnShieldChanged;
        private event Action OnShieldDepleted; // 护盾耗尽事件
        public int CurrentShieldCount { get; private set; }
        public int MaxShieldCount => _fishSinkerData.shieldCount;
        
        // ----- 运行时参数 -----
        private SpriteRenderer _renderer;
        private Collider2D _collider;
        private Coroutine _untouchableCoroutine;
        public void Initialize(FishSinkerData fishSinkerData,Action onShieldDepleted)
        {
            _fishSinkerData = fishSinkerData;
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider2D>();
            
            CurrentShieldCount = _fishSinkerData.shieldCount;
            //OnShieldChanged += onShieldChanged;
            OnShieldDepleted += onShieldDepleted;
            
            OnShieldChangedUIEvent?.Invoke(CurrentShieldCount, MaxShieldCount);
            StartUntouchablePeriod(fishSinkerData.invincibleTime);
        }

        private void OnFishCollision()
        {
            if (CurrentShieldCount > 0)
            {
                CurrentShieldCount--;
                OnShieldChangedUIEvent?.Invoke(CurrentShieldCount,MaxShieldCount);
                StartUntouchablePeriod(_fishSinkerData.invincibleTime);
                
            }
        }
        public void StartUntouchablePeriod(float duration)
        {
            if(_untouchableCoroutine != null)
                StopCoroutine(_untouchableCoroutine);
            _untouchableCoroutine = StartCoroutine(StartUntouchableTimer(duration));
        }
        private IEnumerator StartUntouchableTimer(float duration)
        {
            int blockedLayer = LayerMask.NameToLayer("Fish");
            int bit = 1 << blockedLayer;
            _collider.excludeLayers = bit;                // 只排除这个层
            SetColor(_fishSinkerData.invincibleColor, 0.5f);
            yield return new WaitForSeconds(duration);
            _collider.excludeLayers = 0;// 恢复
            SetColor(_fishSinkerData.shieldColor, 0.5f);
            if (CurrentShieldCount == 0)
            {
                ShieldBreak();
            }
        }

        public void ShieldBreak()
        {
            Color c = _renderer.color;
            c.a = 0;
            _renderer.color = c;
            _collider.enabled = false;
                    
            OnShieldDepleted?.Invoke();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Fish"))
            {
                OnFishCollision();
            }
        }
        public void ResetSinker()
        {
            CurrentShieldCount = _fishSinkerData.shieldCount;
            _collider.enabled = true;
            Color c = _fishSinkerData.shieldColor;
            c.a = 0.5f;
            _renderer.color = c;
            OnShieldChangedUIEvent?.Invoke(CurrentShieldCount, MaxShieldCount);
        }

        public void SetColor(Color c,float alpha = 1f)
        {
            c.a = alpha;
            _renderer.color = c;
        }
        public void DeInit()
        {
            OnShieldChangedUIEvent = null;
            //OnShieldChanged = null;
            OnShieldDepleted = null;
        }
    }
}