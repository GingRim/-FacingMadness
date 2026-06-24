using System;
using UnityEngine;

public class StatModules : CharacterModule
{
    [SerializeField] private StatType designatedStatType;

    public StatType DesignatedStatType => designatedStatType;


    [SerializeField] private int[] stats;

    public sealed override System.Type RegistrationType => typeof(StatModules);

    private void Awake()
    {
        EnsureArraySize();
    }

    public override void OnRegistration(CharacterBase owner)
    {
        base.OnRegistration(owner);

        EnsureArraySize();
    }

    /// <summary>
    /// 능력치 배열 크기 확인
    /// StatType 개수와 맞지 않으면 자동 재생성
    /// </summary>
    private void EnsureArraySize()
    {
        int length = (int)StatType._Length;

        if (stats == null || stats.Length != length)
        {
            stats = new int[length];
        }
    }

    /// <summary>
    /// 능력치 반환
    /// </summary>
    public int GetStat(StatType type)
    {
        EnsureArraySize();

        int index = (int)type;
        int value = stats[index];

        Debug.Log($"[GetStat] module={GetInstanceID()} / " + $"owner={(_owner != null ? _owner.name : "null")} / " + $"{type}={value}");


        return stats[(int)type];
    }

    /// <summary>
    /// 능력치 설정
    /// </summary>
    public void SetStat(StatType type, int value) //EX
    {
        EnsureArraySize();
        
        int index = (int)type;
        int clampedValue = Mathf.Clamp(value, 0, 10);
        //stats[(int)type] = Mathf.Clamp(value, 0, 10);

        stats[index] = clampedValue;

        Debug.Log($"[SetStat] module={GetInstanceID()} / " + $"owner={(_owner != null ? _owner.name : "null")} / " + $"{type}={clampedValue}");
    }

    /// <summary>
    /// 능력치 보정 반환
    /// </summary>
    public int GetModifier(StatType type)
    {
        int stat = GetStat(type);

        return Mathf.FloorToInt(stat / 2.0f);
    }


    public StatType GetDesignatedStatType()
    {
        return designatedStatType;
    }


}
