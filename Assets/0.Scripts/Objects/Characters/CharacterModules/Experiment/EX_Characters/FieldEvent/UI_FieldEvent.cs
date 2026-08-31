using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 필드 이벤트의 이미지, 설명, 선택지 페이지,
/// 카드 선택 및 결과 문장을 표시한다.
/// </summary>
public class UI_FieldEvent : UIBase
{
    private const int MaximumChoiceButtonCount = 5;

    [Header("이벤트 실행기")]
    [SerializeField]
    private FieldEventRunner eventRunner;

    [Header("화면")]
    [SerializeField]
    private GameObject panel;

    [Header("이벤트 이미지")]
    [SerializeField]
    private Image eventImage;

    [Header("텍스트")]
    [SerializeField]
    private TextMeshProUGUI eventNameText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    [Header("선택지 그룹")]
    [SerializeField]
    private GameObject choiceGroup;

    [Header("기존 선택지 버튼")]
    [SerializeField]
    private UI_FieldEventChoiceButton[] choiceButtons = new UI_FieldEventChoiceButton[5];

    [Header("페이지 이동")]
    [SerializeField]
    private Button backButton;

    [Header("결과 확인")]
    [SerializeField]
    private Button continueButton;

    private readonly Dictionary<int, FieldEventChoice> displayedChoices = new();

    private readonly List<int> availableChoiceIndices = new();

    /// <summary>
    /// 현재 이벤트를 진행하는 캐릭터다.
    /// </summary>
    public CharacterBase Character { get; private set; }

    /// <summary>
    /// 이벤트 실행기와 버튼을 연결하고 화면을 초기화한다.
    /// </summary>
    private void Awake()
    {
        BindRunner();

        BindButtons();

        ClearChoiceButtons();

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// 이벤트 실행기와 버튼에 연결한 콜백을 해제한다.
    /// </summary>
    private void OnDestroy()
    {
        UnbindRunner();

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinue);
        }
    }

    /// <summary>
    /// 이벤트 시작, 페이지 변경, 선택 결과,
    /// 실패 및 종료 이벤트를 등록한다.
    /// </summary>
    private void BindRunner()
    {
        if (eventRunner == null)
        {
            Debug.LogWarning("UI_FieldEvent: FieldEventRunner가 없습니다.");

            return;
        }

        eventRunner.OnEventOpened -= HandleEventOpened;
        eventRunner.OnEventOpened += HandleEventOpened;

        eventRunner.OnPageChanged -= HandlePageChanged;
        eventRunner.OnPageChanged += HandlePageChanged;

        eventRunner.OnChoiceSelected -= HandleChoiceSelected;
        eventRunner.OnChoiceSelected += HandleChoiceSelected;

        eventRunner.OnChoiceFailed -= HandleChoiceFailed;
        eventRunner.OnChoiceFailed += HandleChoiceFailed;

        eventRunner.OnEventClosed -= HandleEventClosed;
        eventRunner.OnEventClosed += HandleEventClosed;
    }

    /// <summary>
    /// 이벤트 실행기에 등록한 모든 콜백을 해제한다.
    /// </summary>
    private void UnbindRunner()
    {
        if (eventRunner == null)
            return;

        eventRunner.OnEventOpened -= HandleEventOpened;

        eventRunner.OnPageChanged -= HandlePageChanged;

        eventRunner.OnChoiceSelected -= HandleChoiceSelected;

        eventRunner.OnChoiceFailed -= HandleChoiceFailed;

        eventRunner.OnEventClosed -= HandleEventClosed;
    }

    /// <summary>
    /// 이전 페이지 버튼과 결과 확인 버튼을 등록한다.
    /// </summary>
    private void BindButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);

            backButton.onClick.AddListener(HandleBackButtonClicked);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinue);

            continueButton.onClick.AddListener(HandleContinue);
        }
    }

    /// <summary>
    /// 이벤트의 기본 정보와 분위기 이미지를 표시한다.
    /// 시작 페이지가 없는 기존 이벤트는 기존 선택지 배열을 사용한다.
    /// </summary>
    /// <param name="eventData">표시할 이벤트 데이터</param>
    /// <param name="context">현재 이벤트 실행 정보</param>
    private void HandleEventOpened(FieldEventData eventData, FieldEventContext context)
    {
        if (eventData == null)
            return;


        Character = context != null ? context.Character : null;

        if (eventNameText != null)
        {
            eventNameText.SetText(eventData.EventName);
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(eventData.Description);
        }

        if (eventImage != null)
        {
            eventImage.sprite = eventData.EventImage;

            // 이미지를 숨기더라도 자식 텍스트는 유지한다.
            eventImage.enabled = eventData.EventImage != null;
        }

        if (choiceGroup != null)
        {
            choiceGroup.SetActive(true);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        ClearChoiceButtons();


        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    /// <summary>
    /// 현재 페이지의 설명과 선택지를 새로 표시한다.
    /// 고정 페이지는 순서대로 표시하고
    /// 무작위 페이지는 후보를 섞어 표시한다.
    /// </summary>
    /// <param name="page">새로 표시할 이벤트 페이지</param>
    private void HandlePageChanged(FieldEventPageData page)
    {
        if (page == null)
            return;

        if (descriptionText != null)
        {
            string pageDescription = !string.IsNullOrWhiteSpace(page.Description) ? page.Description : GetCurrentEventDescription();

            descriptionText.SetText(pageDescription);
        }

        if (choiceGroup != null)
        {
            choiceGroup.SetActive(true);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        CreateChoiceButtons(page.Choices, page.DisplayType, page.MaximumVisibleChoices);

        RefreshBackButton();
    }

    /// <summary>
    /// 현재 이벤트의 기본 설명 문장을 반환한다.
    /// </summary>
    /// <returns>현재 이벤트 설명</returns>
    private string GetCurrentEventDescription()
    {
        if (eventRunner == null || eventRunner.CurrentEvent == null)
        {
            return string.Empty;
        }

        return eventRunner.CurrentEvent.Description;
    }

    /// <summary>
    /// 사용 가능한 선택지 중 최대 5개를 기존 버튼에 연결한다.
    /// 선택지 번호는 원본 배열 번호를 그대로 유지한다.
    /// </summary>
    /// <param name="choices">페이지에 등록된 전체 선택지</param>
    /// <param name="displayType">고정 또는 무작위 표시 방식</param>
    /// <param name="maximumCount">최대 표시 개수</param>
    private void CreateChoiceButtons(FieldEventChoice[] choices, FieldEventPageDisplayType displayType, int maximumCount)
    {
        ClearChoiceButtons();

        if (choices == null || choiceButtons == null || eventRunner == null)
        {
            return;
        }

        CollectAvailableChoiceIndices(choices);

        if (availableChoiceIndices.Count == 0)
        {
            Debug.LogWarning("UI_FieldEvent: 표시할 수 있는 선택지가 없습니다.");

            return;
        }

        if (displayType == FieldEventPageDisplayType.Random)
        {
            ShuffleChoiceIndices();
        }

        int usableButtonCount = GetUsableButtonCount();

        int displayCount = Mathf.Min(maximumCount, MaximumChoiceButtonCount, usableButtonCount, availableChoiceIndices.Count);

        int displayedCount = 0;

        foreach (UI_FieldEventChoiceButton button
                 in choiceButtons)
        {
            if (displayedCount >= displayCount)
                break;

            if (button == null)
                continue;

            int originalChoiceIndex = availableChoiceIndices[displayedCount];

            FieldEventChoice choice = choices[originalChoiceIndex];

            displayedChoices[originalChoiceIndex] = choice;

            button.SetChoice(originalChoiceIndex, choice, HandleChoiceButtonSelected);

            displayedCount++;
        }
    }

    /// <summary>
    /// 현재 필드에서 사용할 수 있는 선택지의 원본 배열 번호를 수집한다.
    /// 이미 사용한 1회용 선택지는 제외한다.
    /// </summary>
    /// <param name="choices">현재 페이지의 전체 선택지</param>
    private void CollectAvailableChoiceIndices(FieldEventChoice[] choices)
    {
        availableChoiceIndices.Clear();

        if (choices == null || eventRunner == null)
        {
            return;
        }

        for (int i = 0; i < choices.Length; i++)
        {
            FieldEventChoice choice = choices[i];

            if (!eventRunner.IsChoiceAvailable(choice))
                continue;

            availableChoiceIndices.Add(i);
        }
    }

    /// <summary>
    /// 무작위 페이지의 선택지 표시 순서를 섞는다.
    /// </summary>
    private void ShuffleChoiceIndices()
    {
        for (int i = availableChoiceIndices.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);

            int temporary = availableChoiceIndices[i];

            availableChoiceIndices[i] = availableChoiceIndices[randomIndex];

            availableChoiceIndices[randomIndex] = temporary;
        }
    }

    /// <summary>
    /// 실제로 연결되어 있는 선택지 버튼 개수를 확인한다.
    /// </summary>
    /// <returns>사용 가능한 버튼 개수</returns>
    private int GetUsableButtonCount()
    {
        if (choiceButtons == null)
            return 0;

        int count = 0;

        foreach (UI_FieldEventChoiceButton button
                 in choiceButtons)
        {
            if (button == null)
                continue;

            count++;
        }

        return count;
    }

    /// <summary>
    /// 기존 선택지 버튼을 비활성화하고 표시 기록을 초기화한다.
    /// </summary>
    private void ClearChoiceButtons()
    {
        displayedChoices.Clear();

        availableChoiceIndices.Clear();

        if (choiceButtons == null)
            return;

        foreach (UI_FieldEventChoiceButton button
                 in choiceButtons)
        {
            if (button == null)
                continue;

            button.Clear();
        }
    }

    /// <summary>
    /// 선택지 버튼을 클릭하면 카드 요구 여부를 확인한 후
    /// 페이지 이동 또는 실제 효과 실행을 요청한다.
    /// </summary>
    /// <param name="choiceIndex">원본 선택지 배열의 번호</param>
    private void HandleChoiceButtonSelected(int choiceIndex)
    {
        if (eventRunner == null)
            return;

        if (!displayedChoices.TryGetValue(choiceIndex, out FieldEventChoice choice))
        {
            Debug.LogWarning($"선택지 정보를 찾을 수 없습니다: " + $"{choiceIndex}");

            return;
        }

        eventRunner.SelectChoice(choiceIndex);
    }

    /// <summary>
    /// 선택지의 판정 방식과 성공 여부에 맞는 결과를 표시합니다.
    /// </summary>
    private void HandleChoiceSelected(FieldEventData eventData, FieldEventChoice choice)
    {
        ClearChoiceButtons();


        bool succeeded = eventRunner != null && eventRunner.LastChoiceSucceeded;

        string resultDescription = choice != null ? choice.GetResultText(eventRunner.LastChoiceSucceeded) : string.Empty;

        string checkDescription = CreateCheckResultText();

        FieldEventContext context = eventRunner != null ? eventRunner.CurrentContext : null;

        // 효과가 별도의 결과 문장을 설정했다면 우선 사용
        if (context != null && context.HasResultTextOverride)
        {
            resultDescription = context.ResultTextOverride;
        }

        string displayText;

        if (string.IsNullOrEmpty(checkDescription))
        {
            displayText = resultDescription;
        }
        else if (string.IsNullOrEmpty(resultDescription))
        {
            displayText = checkDescription;
        }
        else
        {
            displayText = $"{checkDescription}\n\n{resultDescription}";
        }

        if (descriptionText != null)
        {
            descriptionText.SetText(displayText);
            descriptionText.gameObject.SetActive(true);
        }

        Sprite resultImage = choice != null ? choice.GetResultImage(succeeded) : null;

        if (eventImage != null && resultImage != null)
        {
            eventImage.sprite = resultImage;
            eventImage.enabled = true;
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 선택지 조건을 만족하지 못했을 때
    /// 이벤트 설명 공간에 실패 원인을 표시합니다.
    /// </summary>
    private void HandleChoiceFailed(string message)
    {
        if (descriptionText == null)
            return;

        descriptionText.SetText(message);
        descriptionText.gameObject.SetActive(true);
    }

    /// <summary>
    /// 이전 페이지로 돌아가기 버튼의 표시 여부를 갱신한다.
    /// </summary>
    private void RefreshBackButton()
    {
        if (backButton == null)
            return;

        bool canReturn = eventRunner != null && eventRunner.CanReturnToPreviousPage;

        backButton.gameObject.SetActive(canReturn);
    }

    /// <summary>
    /// 현재 하위 페이지에서 이전 페이지로 돌아간다.
    /// </summary>
    private void HandleBackButtonClicked()
    {
        if (eventRunner == null)
            return;

        eventRunner.TryReturnToPreviousPage();
    }

    /// <summary>
    /// 결과 확인 버튼을 눌러 현재 이벤트를 종료한다.
    /// </summary>
    private void HandleContinue()
    {
        if (eventRunner == null)
            return;

        eventRunner.CompleteCurrentEvent();
    }

    /// <summary>
    /// 이벤트 종료 시 카드 대기 상태와 화면 표시를 초기화한다.
    /// </summary>
    private void HandleEventClosed()
    {

        ClearChoiceButtons();

        Character = null;

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// 직접 판정 또는 대응 카드 사용 정보를
    /// 이벤트 결과 화면에 표시할 문자열로 만듭니다.
    /// </summary>
    private string CreateCheckResultText()
    {
        if (eventRunner == null)
            return string.Empty;

        CardData usedCard = eventRunner.LastUsedJudgeCard;

        string cardUseText = usedCard != null ? $"{usedCard.cardName} 사용\n" : string.Empty;

        if (!eventRunner.HasLastJudgeResult)
        {
            if (usedCard == null)
                return string.Empty;

            return $"{cardUseText}" + "판정 자동 성공";
        }

        JudgeResult result = eventRunner.LastJudgeResult;

        string resultName;

        if (!result.valid)
        {
            resultName = "판정 불가";
        }
        else if (result.fumble)
        {
            resultName = "펌블";
        }
        else
        {
            resultName = result.success ? "성공" : "실패";
        }

        return
            $"{cardUseText}" +
            $"D10 {result.dice} " +
            $"+ 능력 보정 {result.statModifier} " +
            $"+ 상태 보정 {result.statusModifier}\n" +
            $"= {result.total} / " +
            $"목표 {result.target}\n" +
            $"{resultName}";
    }

}