using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Dependencies.Sqlite.SQLite3;


public struct CostData
{
    public int currntCost;
    public int Max;
}

public class CostModule : CharacterModule
{
    // 각 코스트의 현재 값 / 최대 값을 저장
    // 배열 인덱스는 CostType enum과 연결됨
    private CostData[] costs = new CostData[(int)CostType._Length];

    public sealed override System.Type RegistrationType
        => typeof(CostModule);

    /// <summary>
    /// 현재 코스트가 지정한 수치 이상인지 확인
    /// 사용 가능 여부만 검사하며 실제 소모는 하지 않음
    /// </summary>
    public bool CanUse(CostType type, int amount)
    {
        return costs[(int)type].currntCost >= amount;
    }

    /// <summary>
    /// 코스트를 실제로 소모
    /// 부족할 경우 false 반환
    /// </summary>
    public bool Use(CostType type, int amount)
    {
        if (!CanUse(type, amount))
            return false;

        costs[(int)type].currntCost -= amount;

        return true;
    }

    /// <summary>
    /// 코스트 회복
    /// 최대치를 초과하지 않음
    /// </summary>
    public void Restore(CostType type, int amount)
    {
        int index = (int)type;

        costs[index].currntCost =
            Mathf.Min(
                costs[index].currntCost + amount,
                costs[index].Max
            );
    }

    /// <summary>
    /// 최대 코스트 설정
    /// 설정 시 현재 코스트도 최대치로 초기화
    /// </summary>
    public void SetMax(CostType type, int amount)
    {
        int index = (int)type;

        costs[index].Max = amount;
        costs[index].currntCost = amount;
    }

    /// <summary>
    /// 현재 코스트 반환
    /// </summary>
    public int GetCurrent(CostType type)
    {
        return costs[(int)type].currntCost;
    }

    /// <summary>
    /// 최대 코스트 반환
    /// </summary>
    public int GetMax(CostType type)
    {
        return costs[(int)type].Max;
    }


}
