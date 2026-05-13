using System;

[Serializable]
public struct CardCostData
{
    public CostType costType; // Action / Auxiliary / Reaction
    public int amount;        // 필요한 코스트 양
}
