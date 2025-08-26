using UnityEngine;
using UnityEngine.InputSystem;

namespace GameLogic
{
    public class FishingRod : MonoBehaviour
    {
        // ----- Input -----
        private InputAction _drag;
        private InputAction _click;
        
        // ----- Components -----
        private FishHook _fishHook;
        private FishingLine _fishingLine;
        private FishSinker _fishSinker;
        
        // ----- State -----
        private bool _hasInit = false;
        public void Init()
        {
            if (_hasInit) return;
            
            
            
            
            _hasInit = true;
        }
    }
}