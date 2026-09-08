using System;
using UnityEngine;


/// <summary>
/// 필드 이벤트 선택지의 실행 결과를 관리한다.
/// 결과 설명과 함께 실행할 효과 목록을 가진다.
/// </summary>
[Serializable]
public class FieldChoiceResultData
{
    [Header("결과 설명")]
    [TextArea(3, 10)]
    [SerializeField]
    private string description;

    [Header("결과 효과")]
    [SerializeField]
    private FieldEventEffect[] effects;

    [Header("결과 이미지")]
    [SerializeField]
    private Sprite resultImage;

    [Header("적색 라인 결과")]
    [Tooltip("일반 노드 이벤트는 None을 사용합니다. 적색 라인 이벤트에서만 Open 또는 Locked를 선택합니다.")]
    [SerializeField]
    private FieldRedLineResult redLineResult;

    public Sprite ResultImage => resultImage;
    public FieldRedLineResult RedLineResult => redLineResult;

    /// <summary>
    /// 결과가 발생했을 때 플레이어에게 표시할 설명.
    /// </summary>
    public string Description => description;

    /// <summary>
    /// 결과에 연결된 모든 효과를 순서대로 실행한다.
    /// </summary>
    /// <param name="context">현재 이벤트의 실행 정보.</param>
    public void Execute(FieldEventContext context)
    {
        if (context == null)
            return;

        if (effects != null)
        {
            foreach (FieldEventEffect effect in effects)
            {
                if (effect == null)
                    continue;

                effect.Execute(context);
            }
        }

        if (redLineResult != FieldRedLineResult.None &&
            context.FieldManager != null)
        {
            context.FieldManager.SetPendingRedLineResult(redLineResult);
        }
    }
}
