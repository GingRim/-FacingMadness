using System;
using UnityEngine;

/// <summary>
/// 필드 이벤트 페이지에 표시되는 개별 선택지.
/// 선택지의 분류, 실행 방식, 요구 능력치, 조건과 결과를 관리한다.
/// </summary>
[Serializable]
public class FieldEventChoice
{
    [Header("선택지 식별")]
    [SerializeField]
    private string choiceId;

    [Header("선택지 문구")]
    [SerializeField]
    private string choiceText;

    [Header("선택지 분류")]
    [SerializeField]
    private FieldChoiceType choiceType = FieldChoiceType.Stat;

    [Header("선택지 동작")]
    [SerializeField]
    private FieldChoiceActionType actionType;

    [Header("선택지 사용 횟수")]
    [SerializeField]
    private FieldChoiceUsageType usageType;

    [Header("이동할 페이지")]
    [SerializeField]
    private FieldEventPageData nextPage;

    [Header("실행 방식")]
    [SerializeField]
    private FieldChoiceExecutionType executionType =
        FieldChoiceExecutionType.Direct;

    [Header("판정에 사용할 능력치")]
    [SerializeField]
    private StatType requiredStat = StatType.None;

    [Header("추가 조건")]
    [SerializeField]
    private FieldEventCondition[] conditions;

    [Header("성공 결과")]
    [SerializeField]
    private FieldChoiceResultData successResult = new();

    [Header("실패 결과")]
    [SerializeField]
    private FieldChoiceResultData failureResult = new();

    // 기존 코드에서 CardRequirement를 참조하는 부분을
    // 다음 단계에서 정리하기 전까지 유지하는 호환용 데이터.
    [SerializeField, HideInInspector]
    private FieldCardRequirement legacyCardRequirement = new();

    /// <summary>
    /// 선택지의 고유 식별값.
    /// </summary>
    public string ChoiceId => choiceId;

    /// <summary>
    /// 플레이어에게 표시할 선택지 문구.
    /// </summary>
    public string ChoiceText => choiceText;

    /// <summary>
    /// 핵심, 능력치, 맥거핀 중 해당 선택지의 분류.
    /// </summary>
    public FieldChoiceType ChoiceType => choiceType;

    /// <summary>
    /// 페이지 이동 또는 결과 실행 여부.
    /// </summary>
    public FieldChoiceActionType ActionType => actionType;

    /// <summary>
    /// 선택지의 반복 가능 여부.
    /// </summary>
    public FieldChoiceUsageType UsageType => usageType;

    /// <summary>
    /// 페이지 이동 선택지가 열어야 하는 다음 페이지.
    /// </summary>
    public FieldEventPageData NextPage => nextPage;

    /// <summary>
    /// 일반 실행인지 능력치 판정인지 구분한다.
    /// </summary>
    public FieldChoiceExecutionType ExecutionType => executionType;

    /// <summary>
    /// 능력치 판정에 사용되는 능력치 종류.
    /// </summary>
    public StatType RequiredStat => requiredStat;

    /// <summary>
    /// 성공했을 때 적용할 결과.
    /// </summary>
    public FieldChoiceResultData SuccessResult => successResult;

    /// <summary>
    /// 실패했을 때 적용할 결과.
    /// </summary>
    public FieldChoiceResultData FailureResult => failureResult;

    /// <summary>
    /// 다음 페이지로 이동하는 선택지인지 확인한다.
    /// </summary>
    public bool IsNavigation => actionType == FieldChoiceActionType.Navigate;

    /// <summary>
    /// 필드에서 한 번만 사용할 수 있는 선택지인지 확인한다.
    /// </summary>
    public bool IsOneTime => usageType == FieldChoiceUsageType.OncePerField;

    /// <summary>
    /// 선택지 실행 시 능력치 판정이 필요한지 확인한다.
    /// </summary>
    public bool RequiresStatCheck => executionType == FieldChoiceExecutionType.StatCheck;

    /// <summary>
    /// 기존 UI가 결과 설명을 참조할 수 있도록
    /// 성공 결과의 설명을 임시로 제공한다.
    /// </summary>
    public string ResultText => successResult != null ? successResult.Description : string.Empty;

    /// <summary>
    /// 기존 카드 요구 코드와의 임시 호환용 속성.
    /// 새로운 구조에서는 카드를 필수로 요구하지 않는다.
    /// </summary>
    public FieldCardRequirement CardRequirement => legacyCardRequirement;

    /// <summary>
    /// 새로운 이벤트 구조에서는 카드 사용이 필수가 아니므로
    /// 기존 카드 요구 여부를 항상 false로 처리한다.
    /// </summary>
    public bool RequiresCard => false;

    /// <summary>
    /// 선택지에 연결된 추가 조건을 모두 만족하는지 확인한다.
    /// 아이템이 없더라도 선택지는 표시할 수 있으며,
    /// 실제 선택하는 시점에 이 함수를 사용하여 조건을 검사한다.
    /// </summary>
    /// <param name="context">현재 이벤트의 실행 정보.</param>
    /// <returns>모든 조건을 만족하면 true.</returns>
    public bool CanSelect(FieldEventContext context)
    {
        if (context == null)
            return false;

        if (conditions == null)
            return true;

        foreach (FieldEventCondition condition in conditions)
        {
            if (condition == null)
                continue;

            if (!condition.IsSatisfied(context))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 선택지 조건을 만족하지 못했을 때 표시할 문구를 반환한다.
    /// </summary>
    /// <param name="context">현재 이벤트의 실행 정보.</param>
    /// <returns>실패한 조건의 안내 문구.</returns>
    public string GetFailMessage(FieldEventContext context)
    {
        if (conditions == null)
            return string.Empty;

        foreach (FieldEventCondition condition in conditions)
        {
            if (condition == null)
                continue;

            if (!condition.IsSatisfied(context))
            {
                return condition.GetFailMessage();
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 기존 실행 코드와의 호환을 위해 성공 결과를 실행한다.
    /// 이후 이벤트 실행기를 수정하면 성공과 실패가 분리된다.
    /// </summary>
    /// <param name="context">현재 이벤트의 실행 정보.</param>
    public void Execute(FieldEventContext context)
    {
        ExecuteSuccess(context);
    }

    /// <summary>
    /// 선택지의 성공 설명과 성공 효과를 적용한다.
    /// </summary>
    /// <param name="context">현재 이벤트의 실행 정보.</param>
    public void ExecuteSuccess(FieldEventContext context)
    {
        ExecuteResult(context, successResult);
    }

    /// <summary>
    /// 선택지의 실패 설명과 실패 효과를 적용한다.
    /// </summary>
    /// <param name="context">현재 이벤트의 실행 정보.</param>
    public void ExecuteFailure(FieldEventContext context)
    {
        ExecuteResult(context, failureResult);
    }

    /// <summary>
    /// 선택한 결과의 설명을 저장하고 연결된 효과를 실행한다.
    /// </summary>
    /// <param name="context">현재 이벤트의 실행 정보.</param>
    /// <param name="result">실행할 성공 또는 실패 결과.</param>
    private void ExecuteResult(FieldEventContext context, FieldChoiceResultData result)
    {
        if (context == null || result == null)
            return;

        context.SetResultText(result.Description);

        result.Execute(context);
    }

    /// <summary>
    /// 해당 카드가 요구 능력치와 대응하여
    /// 판정을 확정 성공시킬 수 있는지 확인한다.
    /// 무색 카드는 이벤트 판정에 사용할 수 없다.
    /// </summary>
    /// <param name="card">사용하려는 카드.</param>
    /// <returns>대응하는 색상의 카드이면 true.</returns>
    public bool CanUseCard(CardData card)
    {
        if (card == null || !RequiresStatCheck)
            return false;

        CardColorType requiredColor = GetRequiredCardColor();

        if (requiredColor == CardColorType.None)
            return false;

        return card.color == requiredColor;
    }

    /// <summary>
    /// 요구 능력치에 대응하는 카드 색상을 반환한다.
    /// </summary>
    /// <returns>판정을 확정 성공시킬 수 있는 카드 색상.</returns>
    public CardColorType GetRequiredCardColor()
    {
        switch (requiredStat)
        {
            case StatType.Strength:
                return CardColorType.Red;

            case StatType.Agility:
                return CardColorType.Yellow;

            case StatType.Health:
                return CardColorType.Green;

            case StatType.Intelligence:
                return CardColorType.Blue;

            case StatType.Will:
                return CardColorType.Purple;

            default:
                return CardColorType.None;
        }
    }
}