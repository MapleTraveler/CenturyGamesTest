using System;
using Data.FishData;

namespace GameLogic.FishLogic
{
    public class NormalFish : BaseFish
    {
        
        public override void OnInitialize(BaseFishData fishData)
        {
            base.OnInitialize(fishData);
        }

        public override void OnUpdate()
        {
            switch (_fishState)
            {
                case EFishState.Swimming:
                    HorizontalMove();
                    break;
                case EFishState.Escaping:
                    break;
                case EFishState.Hooked:
                    break;
            }
            
        }
        
        
    }
}