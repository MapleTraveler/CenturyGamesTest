using System.Collections.Generic;
using Data.FishData;
using GameLogic.FishLogic;

namespace Data
{
    public struct RodSettlementData
    {
        public int maxDepth;
    }

    public struct FishSettlementData
    {
        public Dictionary<BaseFishData, int> caughtFishData; // 正确的存储或许是存储id，string标识符，传到下一个阶段进行查找？
        public int totalValue; // 总价值
    }
    public class TotalSettlementData
    {
        public RodSettlementData RodData { get; }
        public FishSettlementData FishData { get; }

        public TotalSettlementData(RodSettlementData rodData, FishSettlementData fishData)
        {
            RodData = rodData;
            FishData = fishData;
        }
    }
}