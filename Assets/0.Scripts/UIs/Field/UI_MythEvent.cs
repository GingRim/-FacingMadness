using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MythEvent : UIBase
{
    [Header("신화턴 제어")]
    [SerializeField]
    private MythTurnController mythTurnController;

    [Header("화면")]
    [SerializeField]
    private GameObject panel;

    [Header("텍스트")]
    [SerializeField]
    private TextMeshProUGUI eventNameText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    [Header("확인")]
    [SerializeField]
    private Button continueButton;

    private void Awake()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinue);

            continueButton.onClick.AddListener(HandleContinue);
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        RegisterController();
    }

    private void OnDisable()
    {
        UnregisterController();
    }

    private void OnDestroy()
    {
        UnregisterController();

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinue);
        }
    }

    private void RegisterController()
    {
        if (mythTurnController == null)
            return;

        mythTurnController.OnMythEventStarted -= HandleMythEventStarted;

        mythTurnController.OnMythEventStarted += HandleMythEventStarted;

        mythTurnController.OnMythEventCompleted -= HandleMythEventCompleted;

        mythTurnController.OnMythEventCompleted += HandleMythEventCompleted;
    }

    private void UnregisterController()
    {
        if (mythTurnController == null)
            return;

        mythTurnController.OnMythEventStarted -= HandleMythEventStarted;

        mythTurnController.OnMythEventCompleted -= HandleMythEventCompleted;
    }

    private void HandleMythEventStarted(MythEventData eventData, MythTurnContext context)
    {
        if (eventData == null)
            return;

        if (eventNameText != null)
        {
            eventNameText.SetText(eventData.EventName);
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(eventData.Description);
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void HandleMythEventCompleted(MythEventData eventData, MythTurnContext context)
    {
        Close();
    }

    private void HandleContinue()
    {
        if (mythTurnController == null)
            return;

        // 중복 클릭 방지
        if (continueButton != null)
        {
            continueButton.interactable = false;
        }

        mythTurnController.CompleteCurrentMythEvent();
    }

    private void Close()
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.interactable = true;
        }
    }
}
