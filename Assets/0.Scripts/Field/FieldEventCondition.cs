using UnityEngine;

/// <summary>
/// 이벤트 조건
/// </summary>
public abstract class FieldEventCondition : ScriptableObject
{
    
    public abstract bool IsSatisfied(FieldEventContext context);

    public virtual string GetFailMessage()
    {
        return "조건을 만족하지 못했습니다.";
    }
}
