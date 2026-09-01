using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// 덱에 들어간 실제 카드 한 장의 런타임 상태를 보관합니다.
/// CardData는 원본 정보이며, 내구도와 변경된 키워드는 이 클래스가 관리합니다.
/// </summary>
[Serializable]
public class CardInstance
{
    [SerializeField]
    private string instanceId;

    [SerializeField]
    private CardData data;

    [SerializeField]
    private List<CardKeywordType> keywords = new();

    [SerializeField]
    private int currentDurability;

    [SerializeField]
    private int maximumDurability;

    [SerializeField]
    private bool isKeywordActive;

    public string InstanceId => instanceId;
    public CardData Data => data;

    public string CardName => data != null ? data.cardName : string.Empty;

    public string Description => data != null ? data.description : string.Empty;

    public CardColorType Color => data != null ? data.color : CardColorType.None;

    public IReadOnlyList<CardKeywordType> Keywords => keywords;

    public int CurrentDurability => currentDurability;

    public int MaximumDurability => maximumDurability;

    public bool HasDurability => maximumDurability > 0;

    public bool IsDepleted => HasDurability && currentDurability <= 0;

    public bool IsKeywordActive => isKeywordActive;

    /// <summary>
    /// 현재 점화할 수 있는 비점화 카드인지 확인합니다.
    /// </summary>
    public bool CanIgnite => HasKeyword(CardKeywordType.Unignited) && !HasKeyword(CardKeywordType.Ignition) && !IsDepleted;

    public event Action<CardInstance> OnKeywordChanged;

    public event Action<CardInstance, int, int> OnDurabilityChanged;

    public event Action<CardInstance, bool> OnKeywordActiveChanged;

    /// <summary>
    /// 저장 데이터 복원 등을 위한 기본 생성자입니다.
    /// </summary>
    public CardInstance()
    {
        EnsureInstanceId();
    }

    /// <summary>
    /// CardData를 기반으로 새로운 카드 한 장을 생성합니다.
    /// initialDurability가 음수면 CardData의 기본 내구도를 사용합니다.
    /// </summary>
    public CardInstance(CardData newData, int initialDurability = -1)
    {
        EnsureInstanceId();
        Initialize(newData, initialDurability);
    }

    /// <summary>
    /// 원본 카드 데이터를 복사하여 런타임 상태를 초기화합니다.
    /// </summary>
    public void Initialize(CardData newData, int initialDurability = -1)
    {
        data = newData;

        keywords.Clear();

        currentDurability = 0;
        maximumDurability = 0;
        isKeywordActive = false;

        if (data == null)
            return;

        foreach (CardKeywordType keyword in data.Keywords)
        {
            if (keyword == CardKeywordType.None || keyword == CardKeywordType._Length)
            {
                continue;
            }

            if (!keywords.Contains(keyword))
            {
                keywords.Add(keyword);
            }
        }

        if (data.UsesDurability)
        {
            int durability = initialDurability >= 0 ? initialDurability : data.BaseDurability;

            SetMaximumDurability(durability);
        }

        // 이미 켜져 있는 광원 또는 점화 상태로 시작하는 카드는
        // 손패 턴 경과 내구도 감소가 즉시 적용되도록 활성화합니다.
        if (HasTurnDecayKeyword())
        {
            SetKeywordActive(true);
        }
    }

    /// <summary>
    /// 지정한 키워드를 현재 카드가 가지고 있는지 확인합니다.
    /// </summary>
    public bool HasKeyword(CardKeywordType keyword)
    {
        if (keyword == CardKeywordType.None)
            return false;

        return keywords.Contains(keyword);
    }

    /// <summary>
    /// 기존 카드에 키워드를 추가합니다.
    /// 내구도 키워드가 처음 추가되면 기본적으로 1+1D10 내구도를 생성합니다.
    /// </summary>
    public bool AddKeyword(CardKeywordType keyword, bool rollEngravedDurability = true)
    {
        if (keyword == CardKeywordType.None || keyword == CardKeywordType._Length)
        {
            return false;
        }

        if (keywords.Contains(keyword))
            return false;

        keywords.Add(keyword);

        if (CardKeywordRules.UsesDurability(keyword) && !HasDurability)
        {
            int durability;

            if (rollEngravedDurability)
            {
                durability = 1 + Dice.RollD10();
            }
            else if (data != null)
            {
                durability = data.BaseDurability;
            }
            else
            {
                durability = 1;
            }

            SetMaximumDurability(durability);
        }

        if (CardKeywordRules.LosesDurabilityEachTurn(keyword))
        {
            SetKeywordActive(true);
        }

        OnKeywordChanged?.Invoke(this);

        return true;
    }

    /// <summary>
    /// 지정한 키워드를 카드에서 제거합니다.
    /// 내구도를 사용하는 키워드가 남지 않으면 내구도도 제거합니다.
    /// </summary>
    public bool RemoveKeyword(CardKeywordType keyword)
    {
        if (!keywords.Remove(keyword))
            return false;

        if (!ContainsDurabilityKeyword())
        {
            currentDurability = 0;
            maximumDurability = 0;

            OnDurabilityChanged?.Invoke(this, currentDurability, maximumDurability);
        }

        if (!HasTurnDecayKeyword())
        {
            SetKeywordActive(false);
        }

        OnKeywordChanged?.Invoke(this);

        return true;
    }

    /// <summary>
    /// 기존 키워드를 다른 키워드로 교체합니다.
    /// 비점화에서 점화로 바뀔 때는 내구도를 유지하고 활성화합니다.
    /// </summary>
    public bool ReplaceKeyword(CardKeywordType oldKeyword, CardKeywordType newKeyword)
    {
        if (!HasKeyword(oldKeyword))
            return false;

        if (newKeyword == CardKeywordType.None || newKeyword == CardKeywordType._Length)
        {
            return false;
        }

        keywords.Remove(oldKeyword);

        if (!keywords.Contains(newKeyword))
        {
            keywords.Add(newKeyword);
        }

        if (CardKeywordRules.UsesDurability(newKeyword) && !HasDurability)
        {
            int durability = data != null && data.BaseDurability > 0 ? data.BaseDurability : 1 + Dice.RollD10();

            SetMaximumDurability(durability);
        }

        if (CardKeywordRules.LosesDurabilityEachTurn(newKeyword))
        {
            SetKeywordActive(true);
        }
        else if (!HasTurnDecayKeyword())
        {
            SetKeywordActive(false);
        }

        OnKeywordChanged?.Invoke(this);

        return true;
    }

    /// <summary>
    /// 키워드 기능의 활성 상태를 변경합니다.
    /// 광원과 점화의 턴 경과 내구도 감소에 사용합니다.
    /// </summary>
    public void SetKeywordActive(bool active)
    {
        if (isKeywordActive == active)
            return;

        isKeywordActive = active;

        OnKeywordActiveChanged?.Invoke(this, isKeywordActive);
    }

    /// <summary>
    /// 카드의 최대 내구도와 현재 내구도를 함께 설정합니다.
    /// </summary>
    public void SetMaximumDurability(int value)
    {
        maximumDurability = Mathf.Max(0, value);

        currentDurability = maximumDurability;

        OnDurabilityChanged?.Invoke(this, currentDurability, maximumDurability);
    }

    /// <summary>
    /// 저장 데이터 복원 등을 위해 현재 내구도를 직접 설정합니다.
    /// </summary>
    public void SetCurrentDurability(int value)
    {
        currentDurability = Mathf.Clamp(value, 0, maximumDurability);

        OnDurabilityChanged?.Invoke(this, currentDurability, maximumDurability);
    }

    /// <summary>
    /// 키워드 사용으로 지정한 양만큼 내구도를 감소시킵니다.
    /// 내구도가 없는 카드는 정상 사용된 것으로 처리합니다.
    /// </summary>
    public bool ConsumeDurability(int amount = 1)
    {
        if (amount <= 0)
            return true;

        if (!HasDurability)
            return true;

        if (currentDurability <= 0)
            return false;

        currentDurability = Mathf.Max(0, currentDurability - amount);

        OnDurabilityChanged?.Invoke(this, currentDurability, maximumDurability);

        return true;
    }

    /// <summary>
    /// 지정한 키워드를 사용하고 카드 내구도를 감소시킵니다.
    /// </summary>
    public bool UseKeyword(CardKeywordType keyword, int durabilityCost = 1)
    {
        if (!HasKeyword(keyword))
            return false;

        if (!CardKeywordRules.UsesDurability(keyword))
            return true;

        return ConsumeDurability(durabilityCost);
    }

    /// <summary>
    /// 카드가 손패에 있고 광원 또는 점화가 활성화된 경우
    /// 턴 경과 내구도를 1 감소시킵니다.
    /// </summary>
    public bool ConsumeTurnDurability(bool isInHand)
    {
        if (!isInHand || !isKeywordActive || !HasTurnDecayKeyword())
        {
            return false;
        }

        return ConsumeDurability(1);
    }

    /// <summary>
    /// 현재 카드의 키워드를 UI 표시 문자열로 만듭니다.
    /// </summary>
    public string GetKeywordDisplayText()
    {
        if (keywords == null || keywords.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();

        foreach (CardKeywordType keyword in keywords)
        {
            string displayName = CardKeywordRules.GetDisplayName(keyword);

            if (string.IsNullOrEmpty(displayName))
                continue;

            if (builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append('[');
            builder.Append(displayName);
            builder.Append(']');
        }

        return builder.ToString();
    }

    /// <summary>
    /// 내구도를 사용하는 키워드가 하나라도 있는지 확인합니다.
    /// </summary>
    private bool ContainsDurabilityKeyword()
    {
        foreach (CardKeywordType keyword in keywords)
        {
            if (CardKeywordRules.UsesDurability(keyword))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 손패 턴 경과 시 내구도가 감소하는 키워드가 있는지 확인합니다.
    /// </summary>
    private bool HasTurnDecayKeyword()
    {
        foreach (CardKeywordType keyword in keywords)
        {
            if (CardKeywordRules.LosesDurabilityEachTurn(keyword))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 카드 한 장을 구분할 고유 식별자가 없으면 생성합니다.
    /// </summary>
    private void EnsureInstanceId()
    {
        if (!string.IsNullOrEmpty(instanceId))
            return;

        instanceId = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// 점화 판정 결과를 적용합니다.
    /// 성공하면 비점화를 점화로 변경하고 활성화합니다.
    /// 실패하면 아무것도 변경하지 않습니다.
    /// </summary>
    public bool ResolveIgnition(bool checkSucceeded)
    {
        if (!CanIgnite)
            return false;

        if (!checkSucceeded)
            return false;

        bool changed = ReplaceKeyword(CardKeywordType.Unignited, CardKeywordType.Ignition);

        if (!changed)
            return false;

        SetKeywordActive(true);

        Debug.Log($"{CardName}: 점화 성공 / " + "비점화 → 점화");

        return true;
    }

}
