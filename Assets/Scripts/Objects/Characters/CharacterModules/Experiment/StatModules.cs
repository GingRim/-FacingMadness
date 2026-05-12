using UnityEngine;

public class StatModules : CharacterModule
{
    [SerializeField] private int[] stats = new int[(int)StatType._Length];

    [SerializeField] private StatType designatedStat; // 지정 능력치
    [SerializeField] private bool hasDesignatedStat;

    public sealed override System.Type RegistrationType => typeof(StatModules);

    public int GetStat(StatType type)
    {
        return stats[(int)type];
    }

    public void SetStat(StatType type, int value)
    {
        int max = GetMaxStat(type);
        stats[(int)type] = Mathf.Clamp(value, 0, max);
    }

    public void AddStat(StatType type, int amount)
    {
        SetStat(type, GetStat(type) + amount);
    }

    /// <summary>
    /// 게임 시작 시 1개의 지정 능력치를 설정한다.
    /// 지정 능력치는 +1 보너스를 받고 최대 10까지 가능하다.
    /// </summary>
    public void SetDesignatedStat(StatType type)
    {
        designatedStat = type;
        hasDesignatedStat = true;

        AddStat(type, 1);
    }

    /// <summary>
    /// 기본 최대치는 9.
    /// 지정 능력치만 최대 10.
    /// </summary>
    public int GetMaxStat(StatType type)
    {
        if (hasDesignatedStat && designatedStat == type)
            return 10;

        return 9;
    }

    /// <summary>
    /// 능력치 보정 = 능력치 / 2 소수점 버림
    /// </summary>
    public int GetModifier(StatType type)
    {
        return GetStat(type) / 2;
    }

}
