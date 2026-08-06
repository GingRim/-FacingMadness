using System;
using UnityEngine;

/// <summary>
/// 이벤트 선택지
/// </summary>
[Serializable]
public class FieldEventChoice
{
    [SerializeField] private string choiceText;

    [TextArea(2, 6)]
    [SerializeField] private string resultText;

    [Header("선택 조건")]
    [SerializeField]
    private FieldEventCondition[] conditions;

    [Header("선택 결과")]
    [SerializeField]
    private FieldEventEffect[] effects;

    public string ChoiceText => choiceText;
    public string ResultText => resultText;

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

    public void Execute(FieldEventContext context)
    {
        if (effects == null)
            return;

        foreach (FieldEventEffect effect in effects)
        {
            if (effect == null)
                continue;

            effect.Execute(context);
        }
    }
}
