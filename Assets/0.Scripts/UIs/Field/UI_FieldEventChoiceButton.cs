using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 필드 이벤트 선택지 버튼 하나를 관리한다.
/// 원본 선택지 번호와 클릭 처리를 연결한다.
/// </summary>
[RequireComponent(typeof(Button))]
public class UI_FieldEventChoiceButton : MonoBehaviour
{
    [Header("선택지 문장")]
    [SerializeField]
    private TextMeshProUGUI choiceText;

    [Header("판정 아이콘 영역")]
    [Tooltip("두 번째 예시의 빨간 Image 영역 오브젝트를 연결합니다.")]
    [SerializeField]
    private GameObject requirementIconRoot;

    [Tooltip("빨간 영역 안에서 실제 아이콘을 표시할 자식 Image를 연결합니다.")]
    [SerializeField]
    private Image requirementIcon;

    private Button button;

    private int choiceIndex = -1;

    private Action<int> onSelected;

    /// <summary>
    /// 버튼 컴포넌트와 클릭 이벤트를 초기화한다.
    /// </summary>
    private void Awake()
    {
        BindButton();
    }

    /// <summary>
    /// 버튼 클릭 이벤트를 해제한다.
    /// </summary>
    private void OnDestroy()
    {
        if (button == null)
            return;

        button.onClick.RemoveListener(HandleClick);
    }

    /// <summary>
    /// 버튼 컴포넌트를 확보하고 클릭 이벤트를 연결한다.
    /// 비활성화된 버튼을 다시 사용할 때도 호출할 수 있다.
    /// </summary>
    private void BindButton()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button == null)
            return;

        button.onClick.RemoveListener(HandleClick);

        button.onClick.AddListener(HandleClick);
    }

    /// <summary>
    /// 선택지 데이터를 버튼에 연결하고 화면에 표시한다.
    /// </summary>
    /// <param name="index">원본 선택지 배열 번호</param>
    /// <param name="choice">표시할 선택지 데이터</param>
    /// <param name="selectedCallback">
    /// 버튼 클릭 시 실행할 콜백
    /// </param>
    public void SetChoice(int index, FieldEventChoice choice, Action<int> selectedCallback)
    {
        BindButton();

        choiceIndex = index;

        onSelected = selectedCallback;

        if (choiceText != null)
        {
            choiceText.SetText(choice != null ? choice.ChoiceText : string.Empty);
        }

        RefreshPresentation(choice);

        if (button != null)
        {
            button.interactable = choice != null;
        }

        gameObject.SetActive(choice != null);
    }

    /// <summary>
    /// ScriptableObject 선택지 없이 런타임 문장을 버튼에 연결합니다.
    /// 메모 목록과 페이지 이동 버튼에서 사용합니다.
    /// </summary>
    public void SetText(int index, string text, Action<int> selectedCallback)
    {
        BindButton();

        choiceIndex = index;
        onSelected = selectedCallback;

        if (choiceText != null)
            choiceText.SetText(text ?? string.Empty);

        HideRequirementIcon();

        if (button != null)
            button.interactable = !string.IsNullOrWhiteSpace(text);

        gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
    }

    /// <summary>
    /// 버튼에 연결된 선택지와 클릭 정보를 초기화한다.
    /// </summary>
    public void Clear()
    {
        choiceIndex = -1;

        onSelected = null;

        if (choiceText != null)
        {
            choiceText.SetText(string.Empty);
        }

        HideRequirementIcon();

        if (button != null)
        {
            button.interactable = false;
        }

        gameObject.SetActive(false);
    }

    private void RefreshPresentation(FieldEventChoice choice)
    {
        bool showIconArea = choice != null && choice.RequiresStatCheck;
        Sprite icon = showIconArea ? choice.ChoiceIcon : null;

        if (requirementIconRoot != null)
            requirementIconRoot.SetActive(showIconArea);

        if (requirementIcon != null)
        {
            requirementIcon.sprite = icon;
            requirementIcon.enabled = icon != null;

            // 별도 Root를 연결하지 않은 경우에도 기존 프리팹에서 안전하게 작동합니다.
            if (requirementIconRoot == null)
                requirementIcon.gameObject.SetActive(showIconArea);
        }
    }

    private void HideRequirementIcon()
    {
        if (requirementIconRoot != null)
            requirementIconRoot.SetActive(false);

        if (requirementIcon == null)
            return;

        requirementIcon.sprite = null;
        requirementIcon.enabled = false;

        if (requirementIconRoot == null)
            requirementIcon.gameObject.SetActive(false);
    }

    /// <summary>
    /// 버튼에 연결된 원본 선택지 번호를 UI에 전달한다.
    /// </summary>
    private void HandleClick()
    {
        if (choiceIndex < 0)
            return;

        onSelected?.Invoke(choiceIndex);
    }
}
