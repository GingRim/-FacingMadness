public static class StatTypeDisplayUtility
{
    public static string GetName(StatType statType)
    {
        switch (statType)
        {
            case StatType.Strength: return "근력";
            case StatType.Agility: return "민첩";
            case StatType.Health: return "건강";
            case StatType.Intelligence: return "지능";
            case StatType.Will: return "의지";
            default: return "없음";
        }
    }
}
