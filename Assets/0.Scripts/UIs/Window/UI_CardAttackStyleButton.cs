using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 공격 방식 선택 버튼의 문장과 우측 판정 아이콘을 갱신합니다.
/// </summary>
public class UI_CardAttackStyleButton : MonoBehaviour
{
    [Header("판정 아이콘 영역")]
    [Tooltip("두 번째 예시의 빨간 Image 영역 오브젝트를 연결합니다.")]
    [SerializeField] private GameObject requirementIconRoot;

    [Tooltip("색상 영역 안에서 실제 아이콘을 표시할 자식 Image를 연결합니다.")]
    [SerializeField] private Image requirementIcon;

    [Header("공격 방식 이름")]
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("설명")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    public void SetStyle(CardAttackStyleData style)
    {
        if (style == null)
        {
            Clear();
            return;
        }

        bool showIconArea = style.RequiresStatCheck;

        if (requirementIconRoot != null)
            requirementIconRoot.SetActive(showIconArea);

        if (requirementIcon != null)
        {
            requirementIcon.sprite = showIconArea ? style.Icon : null;
            requirementIcon.enabled = showIconArea && style.Icon != null;

            if (requirementIconRoot == null)
                requirementIcon.gameObject.SetActive(showIconArea);
        }

        if (titleText != null)
            titleText.SetText(style.DisplayName);

        if (descriptionText != null)
            descriptionText.SetText(style.Description ?? string.Empty);
    }

    public void Clear()
    {
        if (requirementIconRoot != null)
            requirementIconRoot.SetActive(false);

        if (requirementIcon != null)
        {
            requirementIcon.sprite = null;
            requirementIcon.enabled = false;

            if (requirementIconRoot == null)
                requirementIcon.gameObject.SetActive(false);
        }

        if (titleText != null)
            titleText.SetText(string.Empty);

        if (descriptionText != null)
            descriptionText.SetText(string.Empty);
    }
}
