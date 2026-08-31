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

    public Sprite ResultImage => resultImage;

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
        if (context == null || effects == null)
            return;

        foreach (FieldEventEffect effect in effects)
        {
            if (effect == null)
                continue;

            effect.Execute(context);
        }
    }
}