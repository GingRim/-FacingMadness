using UnityEngine;

/// <summary>
/// 이벤트 결과
/// </summary>
public abstract class FieldEventEffect : ScriptableObject
{
    public abstract void Execute(FieldEventContext context);
}
