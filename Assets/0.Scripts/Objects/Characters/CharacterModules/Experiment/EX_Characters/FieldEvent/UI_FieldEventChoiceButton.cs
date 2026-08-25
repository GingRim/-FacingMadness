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

        if (button != null)
        {
            button.interactable = choice != null;
        }

        gameObject.SetActive(choice != null);
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

        if (button != null)
        {
            button.interactable = false;
        }

        gameObject.SetActive(false);
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