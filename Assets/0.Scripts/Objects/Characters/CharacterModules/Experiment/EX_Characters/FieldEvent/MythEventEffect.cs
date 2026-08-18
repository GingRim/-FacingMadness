using UnityEngine;

public abstract class MythEventEffect : ScriptableObject
{
    /// <summary>
    /// 신화 이벤트의 실제 효과를 실행한다.
    /// </summary>
    public abstract void Execute(MythTurnContext context);
}
