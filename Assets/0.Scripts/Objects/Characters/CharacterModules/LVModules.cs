using UnityEngine;

public class LVModules : CharacterModule
{
    [SerializeField] private int level = 1;
    [SerializeField] private int exp = 0;
    [SerializeField] private int maxLevel = 10;

    public sealed override System.Type RegistrationType
        => typeof(LVModules);

    public int Level => level;
    public int Exp => exp;
    public int MaxLevel => maxLevel;
    public bool IsMaxLevel => level >= maxLevel;

    /// <summary>
    /// 레벨을 직접 설정한다.
    /// </summary>
    public void SetLevel(int value)
    {
        level = Mathf.Clamp(value, 1, maxLevel);
    }

    /// <summary>
    /// 경험치를 추가한다.
    /// </summary>
    public void AddExp(int amount)
    {
        if (IsMaxLevel)
            return;

        exp += amount;

        CheckLevelUp();
    }

    /// <summary>
    /// 레벨업 조건 확인.
    /// 지금은 임시로 레벨 * 100 경험치 필요.
    /// </summary>
    private void CheckLevelUp()
    {
        while (!IsMaxLevel && exp >= GetRequiredExp())
        {
            exp -= GetRequiredExp();
            level++;
        }
    }

    /// <summary>
    /// 현재 레벨에서 다음 레벨까지 필요한 경험치.
    /// </summary>
    public int GetRequiredExp()
    {
        return level * 100;
    }
}
