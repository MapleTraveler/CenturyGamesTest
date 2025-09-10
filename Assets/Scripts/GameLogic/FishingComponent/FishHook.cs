using System;
using Core.Interfaces;
using Data.FishingComponent;
using UnityEngine;

namespace GameLogic.FishingComponent
{
    public class FishHook : MonoBehaviour
    {
        private FishHookData _fishHookData;

        public Transform hookPoint;
        
        public int currentHookedFishCount = 0; // 当前钩住的鱼的数量
        public int MaxHookedFishCount => _fishHookData.canHookFishCount;

        public event Func<int,Transform,bool> OnFishCaught; // 捕获鱼类事件，参数为鱼的ID
        public event Action<int,int> OnHookCountChanged;// 钩住数量变化（current, max）

        public event Action<RodState> ChangeRodState;
        // ----- Components -----
        private Collider2D col;
        // ----- 运行时数据 -----
        public bool IsFull => currentHookedFishCount >= MaxHookedFishCount;
        public Vector2 startPos;
        public bool isLocked = false; // 钩子是否锁定，锁定后不再钩鱼
        private float _hookMinX, _hookMaxX;
        public bool hasInit = false;
        public void Init(FishHookData fishHookData,Vector2 fishHookStartPos,Func<int,Transform,bool> onFishCaught, Action<RodState> changeRodState,float hookMinX, float hookMaxX)
        {
            if (hasInit) return;
            _fishHookData = fishHookData;
            startPos = fishHookStartPos;
            hookPoint = GameObject.Find("HookPoint").transform;
            col = GetComponent<Collider2D>();
            OnFishCaught += onFishCaught;
            ChangeRodState += changeRodState;
            _hookMinX = hookMinX;
            _hookMaxX = hookMaxX;
            //OnHookCountChanged += onHookCountChanged;
            
            LockHook();
            
            OnHookCountChanged?.Invoke(currentHookedFishCount, MaxHookedFishCount);
            hasInit = true;
        }
        public void HorizontalMovement(Vector2 delta)
        {
            var pos = transform.localPosition;
            pos.x += delta.x;
            pos.x = Mathf.Clamp(pos.x, _hookMinX, _hookMaxX);
            transform.localPosition = pos;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //Debug.Log($"{other.gameObject.name} Enter");
            if (other.CompareTag("Fish"))
            {
                //Debug.Log($"Ensure Fish");
                var fish = other.GetComponent<IEntity>();
                if (fish != null)
                {
                    bool? flag = OnFishCaught?.Invoke(fish.ID, hookPoint);
                    //Debug.Log($"Try Hook Fish {fish.ID}, Result: {flag}");
                    if (flag != null && flag.Value)
                    {
                        currentHookedFishCount++;
                        OnHookCountChanged?.Invoke(currentHookedFishCount, MaxHookedFishCount);
                        if(currentHookedFishCount == 1) 
                            ChangeRodState?.Invoke(RodState.Ascending);
                    }
                    
                    if (IsFull) LockHook();
                }
                   
            }  
        }

        public void UnlockHook()
        {
           if(!isLocked) return;
           col.excludeLayers = 0;// 恢复
           isLocked = false;
           //Debug.Log("UnLocked Hook");
        }

        public void LockHook()
        {
            if(isLocked) return;
            // 只排除 Enemy，其他都允许.
            
            int blockedLayer = LayerMask.NameToLayer("Fish");
            int bit = 1 << blockedLayer;
            col.excludeLayers |= bit;                // 只排除这个层
            isLocked = true;
            //Debug.Log("Locked Hook");
        }
        
        public void ResetHook()
        {
            currentHookedFishCount = 0;
            isLocked = false;
            col.excludeLayers = 0;
            LockHook();
            transform.localPosition = new Vector3(startPos.x,startPos.y, transform.localPosition.z); // 可根据实际初始位置调整
            OnHookCountChanged?.Invoke(currentHookedFishCount, MaxHookedFishCount);
        }

        public void DeInit()
        {
            OnFishCaught = null;
            OnHookCountChanged = null;
        }
    }
}