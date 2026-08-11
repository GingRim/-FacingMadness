using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_FieldEvent : UIBase
{
    [Header("이벤트 실행기")]
    [SerializeField]
    private FieldEventRunner eventRunner;

    [Header("화면")]
    [SerializeField] private GameObject panel;

    [Header("텍스트")]
    [SerializeField]
    private TextMeshProUGUI eventNameText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    [SerializeField]
    private TextMeshProUGUI resultText;

    [Header("선택지")]
    [SerializeField] private Transform choiceCore;

    [SerializeField]
    private UI_FieldEventChoiceButton choiceTemplate;

    [Header("결과 확인")]
    [SerializeField] private Button continueButton;

    private readonly List<UI_FieldEventChoiceButton> choicePool = new();

    private FieldEventChoice pendingCardChoice;

    public CharacterBase Character { get; private set; }

    public bool IsWaitingCardSelection => pendingCardChoice != null;

    private readonly Dictionary<int, FieldEventChoice> displayedChoices = new();

    public event Action<FieldEventChoice> OnCardSelectionRequested;

    private int pendingCardChoiceIndex = -1;

    private void Awake()
    {
        InitializeChoicePool();
        BindRunner();
        BindContinueButton();

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        UnbindRunner();

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinue);
        }
    }

    private void BindRunner()
    {
        if (eventRunner == null)
        {
            Debug.LogWarning("UI_FieldEvent: FieldEventRunner가 없습니다.");

            return;
        }

        eventRunner.OnEventOpened -= HandleEventOpened;

        eventRunner.OnEventOpened += HandleEventOpened;

        eventRunner.OnChoiceSelected -= HandleChoiceSelected;

        eventRunner.OnChoiceSelected += HandleChoiceSelected;

        eventRunner.OnChoiceFailed -= HandleChoiceFailed;

        eventRunner.OnChoiceFailed += HandleChoiceFailed;

        eventRunner.OnEventClosed -= HandleEventClosed;

        eventRunner.OnEventClosed += HandleEventClosed;
    }

    private void UnbindRunner()
    {
        if (eventRunner == null)
            return;

        eventRunner.OnEventOpened -= HandleEventOpened;

        eventRunner.OnChoiceSelected -= HandleChoiceSelected;

        eventRunner.OnChoiceFailed -= HandleChoiceFailed;

        eventRunner.OnEventClosed -= HandleEventClosed;
    }

    private void BindContinueButton()
    {
        if (continueButton == null)
            return;

        continueButton.onClick.RemoveListener(HandleContinue);

        continueButton.onClick.AddListener(HandleContinue);
    }

    private void InitializeChoicePool()
    {
        if (choiceTemplate == null || choiceCore == null)
        {
            return;
        }

        choiceTemplate.gameObject.SetActive(false);
    }

    private void HandleEventOpened(FieldEventData eventData, FieldEventContext context)
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

        if (resultText != null)
        {
            resultText.SetText(string.Empty);
            resultText.gameObject.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        CreateChoiceButtons(eventData.Choices);

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void CreateChoiceButtons(FieldEventChoice[] choices)
    {
        ClearChoiceButtons();

        if (choices == null)
            return;

        EnsurePoolSize(choices.Length);

        for (int i = 0; i < choices.Length; i++)
        {
            FieldEventChoice choice = choices[i];

            if (choice == null)
                continue;

            displayedChoices[i] = choice;

            choicePool[i].SetChoice(i, choice, HandleChoiceButtonSelected);
        }
    }

    private void EnsurePoolSize(int count)
    {
        if (choiceTemplate == null || choiceCore == null)
        {
            return;
        }

        while (choicePool.Count < count)
        {
            UI_FieldEventChoiceButton newButton = Instantiate(choiceTemplate, choiceCore);

            newButton.name = $"EventChoice_{choicePool.Count}";

            newButton.gameObject.SetActive(false);

            choicePool.Add(newButton);
        }
    }

    private void ClearChoiceButtons()
    {
        displayedChoices.Clear();

        foreach (UI_FieldEventChoiceButton button in choicePool)
        {
            if (button == null)
                continue;

            button.Clear();
        }
    }

    private void HandleChoiceButtonSelected(int choiceIndex)
    {
        if (eventRunner == null)
            return;

        if (!displayedChoices.TryGetValue(choiceIndex, out FieldEventChoice choice))
        {
            Debug.LogWarning($"선택지 정보를 찾을 수 없습니다: {choiceIndex}");

            return;
        }

        eventRunner.ClearSelectedCard();

        if (choice.RequiresCard)
        {
            pendingCardChoice = choice;
            pendingCardChoiceIndex = choiceIndex;

            OnCardSelectionRequested?.Invoke(choice);
            return;
        }

        eventRunner.SelectChoice(choiceIndex);
    }

    private void HandleChoiceSelected(FieldEventData eventData, FieldEventChoice choice)
    {
        ClearChoiceButtons();

        string displayResult = choice != null ? choice.ResultText : string.Empty;

        FieldEventContext context = eventRunner != null ? eventRunner.CurrentContext : null;

        if (context != null && context.HasResultTextOverride)
        {
            displayResult = context.ResultTextOverride;
        }

        if (resultText != null)
        {
            resultText.SetText(displayResult);
            resultText.gameObject.SetActive(true);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }
    }

    private void HandleChoiceFailed(string message)
    {
        if (resultText == null)
            return;

        resultText.SetText(message);
        resultText.gameObject.SetActive(true);
    }

    private void HandleContinue()
    {
        if (eventRunner == null)
            return;

        eventRunner.CompleteCurrentEvent();
    }

    private void HandleEventClosed()
    {
        CancelCardSelection();
        ClearChoiceButtons();

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public bool SubmitSelectedCard(CardData card)
    {
        if (pendingCardChoice == null || pendingCardChoiceIndex < 0)
        {
            return false;
        }

        if (!pendingCardChoice.CanUseCard(card))
        {
            Debug.Log("이 선택지에는 해당 카드를 사용할 수 없습니다.");

            return false;
        }

        int selectedChoiceIndex = pendingCardChoiceIndex;

        pendingCardChoice = null;
        pendingCardChoiceIndex = -1;

        eventRunner.SetSelectedCard(card);
        eventRunner.SelectChoice(selectedChoiceIndex);

        return true;
    }


    public void CancelCardSelection()
    {
        pendingCardChoice = null;
        pendingCardChoiceIndex = -1;

        if (eventRunner != null)
        {
            eventRunner.ClearSelectedCard();
        }
    }

}
