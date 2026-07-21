using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ReactionSelect : UIBase
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Buttons")]
    [SerializeField] private Button guardButton;
    [SerializeField] private Button evadeButton;
    [SerializeField] private Button counterattackButton;
    [SerializeField] private Button cancelButton;

    public event Action<ActionType> OnSelected;

    public bool IsOpen => gameObject.activeSelf;

    private void Awake()
    {
        gameObject.SetActive(false);
        BindButtons();
    }

    private void OnEnable()
    {
        InputManager.OnPausePriority -= BlockPause;
        InputManager.OnPausePriority += BlockPause;
    }

    private void OnDisable()
    {
        InputManager.OnPausePriority -= BlockPause;
    }

    private bool BlockPause()
    {
        // 대응 UI가 켜져 있으면
        // Pause 입력을 소비함
        return IsOpen;
    }

    public void Open(int damageAmount, bool canCounter)
    {
        CloseOtherUI();

        if (descriptionText != null)
        {
            descriptionText.SetText(
                $"{damageAmount} 피해를 받았습니다.\n" + "어느 방식으로 대응하시겠습니까?");
        }

        if (guardButton != null)
            guardButton.gameObject.SetActive(true);

        if (evadeButton != null)
            evadeButton.gameObject.SetActive(true);

        if (counterattackButton != null)
            counterattackButton.gameObject.SetActive(canCounter);

        if (cancelButton != null)
            cancelButton.gameObject.SetActive(true);

        gameObject.SetActive(true);
    }

    private void BindButtons()
    {
        if (guardButton != null)
        {
            guardButton.onClick.RemoveListener(SelectGuard);
            guardButton.onClick.AddListener(SelectGuard);
        }

        if (evadeButton != null)
        {
            evadeButton.onClick.RemoveListener(SelectEvade);
            evadeButton.onClick.AddListener(SelectEvade);
        }

        if (counterattackButton != null)
        {
            counterattackButton.onClick.RemoveListener(SelectCounterattack);
            counterattackButton.onClick.AddListener(SelectCounterattack);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(SelectCancel);
            cancelButton.onClick.AddListener(SelectCancel);
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

     private void SelectGuard()
     {
        Select(ActionType.Guard);
     }

    private void SelectEvade()
    {
        Select(ActionType.Evade);
    }

    private void SelectCounterattack()
    {
        Select(ActionType.Counterattack);
    }

    private void SelectCancel()
    {
        Select(ActionType.None);
    }

    private void Select(ActionType actionType)
    {
        Close();

        Debug.Log($"대응 선택 UI: {actionType}");

        OnSelected?.Invoke(actionType);
    }

    private void CloseOtherUI()
    {
        UIBase pauseUI = UIManager.GetUIM2(UIType.Pause);

        if (pauseUI != null &&
            pauseUI.isActiveAndEnabled)
        {
            UIManager.CloseUIM2(UIType.Pause);
        }

        UI_KeywordHoverInfo encyclopedia = UnityEngine.Object.FindFirstObjectByType<UI_KeywordHoverInfo>(
                FindObjectsInactive.Include);

        if (encyclopedia != null && encyclopedia.IsOpen)
        {
            encyclopedia.Close();
        }
    }
}
