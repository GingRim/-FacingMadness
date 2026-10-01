using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 필드 이벤트의 이미지, 설명과 선택지 페이지를 표시합니다.
/// 선택 결과는 UI_ChapterStory에 전달합니다.
/// </summary>
public class UI_FieldEvent : UIBase
{
    private const int MaximumChoiceButtonCount = 5;
    private const int MemoPageSize = 3;
    private const int MemoPreviousButtonId = 1000000;
    private const int MemoNextButtonId = 1000001;

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

    [Header("이벤트 결과 화면")]
    [SerializeField]
    private UI_ChapterStory resultStoryUI;

    [Header("이벤트 결과 전환")]
    [SerializeField, Min(0f)]
    private float resultDisplayDelay = 0.5f;

    private readonly Dictionary<int, FieldEventChoice> displayedChoices = new();

    private readonly List<int> availableChoiceIndices = new();

    private readonly List<FieldInformationData> memoInformation = new();

    private int memoPageIndex;

    private bool isShowingMemoContent;

    private Coroutine resultTransitionRoutine;

    /// <summary>
    /// 현재 이벤트를 진행하는 캐릭터다.
    /// </summary>
    public CharacterBase Character { get; private set; }

    /// <summary>
    /// 이벤트 실행기와 버튼을 연결하고 화면을 초기화한다.
    /// </summary>
    private void Awake()
    {
        ResolveEventRunner();

        if (resultStoryUI == null)
        {
            Debug.LogError(
                "UI_FieldEvent: Result Story UI에 " +
                "결과창의 UI_ChapterStory를 연결해야 합니다.");
        }

        BindRunner();

        BindButtons();

        ClearChoiceButtons();

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ResolveEventRunner();
        BindRunner();

        // 후보 선택창이 닫히는 동안 이 UI가 비활성화됐더라도
        // 이미 열린 이벤트의 현재 화면을 즉시 복구합니다.
        if (eventRunner != null && eventRunner.IsEventActive)
        {
            HandleEventOpened(
                eventRunner.CurrentEvent,
                eventRunner.CurrentContext);

            HandleEventDataChanged(eventRunner.CurrentData);
        }
    }

    private void ResolveEventRunner()
    {
        FieldManager runtimeField =
            GameManager.Instance != null
                ? GameManager.Instance.Field
                : null;

        if (runtimeField != null && runtimeField.EventRunner != null)
        {
            if (eventRunner != runtimeField.EventRunner)
            {
                UnbindRunner();
                eventRunner = runtimeField.EventRunner;
            }

            return;
        }

        if (eventRunner == null)
        {
            eventRunner = FindFirstObjectByType<FieldEventRunner>(
                FindObjectsInactive.Include);
        }
    }

    /// <summary>
    /// 이벤트 실행기와 버튼에 연결한 콜백을 해제한다.
    /// </summary>
    private void OnDestroy()
    {
        StopResultTransition();

        UnbindRunner();

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);
        }

    }

    /// <summary>
    /// 이벤트 시작, 표시 데이터 변경, 선택 결과,
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

        eventRunner.OnEventDataChanged -= HandleEventDataChanged;
        eventRunner.OnEventDataChanged += HandleEventDataChanged;

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

        eventRunner.OnEventDataChanged -= HandleEventDataChanged;

        eventRunner.OnChoiceSelected -= HandleChoiceSelected;

        eventRunner.OnChoiceFailed -= HandleChoiceFailed;

        eventRunner.OnEventClosed -= HandleEventClosed;
    }

    /// <summary>
    /// 이전 페이지 버튼을 등록합니다.
    /// </summary>
    private void BindButtons()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(HandleBackButtonClicked);

            backButton.onClick.AddListener(HandleBackButtonClicked);
        }

    }

    /// <summary>
    /// 이벤트 화면을 열고 실행 캐릭터를 연결합니다.
    /// </summary>
    /// <param name="eventData">표시할 이벤트 데이터</param>
    /// <param name="context">현재 이벤트 실행 정보</param>
    private void HandleEventOpened(FieldEventData eventData, FieldEventContext context)
    {
        if (eventData == null)
            return;

        Character = context != null ? context.Character : null;

        memoPageIndex = 0;
        isShowingMemoContent = false;

        if (choiceGroup != null)
        {
            choiceGroup.SetActive(true);
        }

        ClearChoiceButtons();

        RefreshBackButton();

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    /// <summary>
    /// 현재 FieldEventData의 설명, 이미지와 선택지를 표시합니다.
    /// 기존 페이지 호환 데이터에 비어 있는 값은 루트 이벤트 값을 사용합니다.
    /// </summary>
    /// <param name="data">새로 표시할 통합 이벤트 데이터</param>
    private void HandleEventDataChanged(FieldEventData data)
    {
        if (data == null)
            return;

        isShowingMemoContent = false;

        FieldEventData rootEvent =
            eventRunner != null
                ? eventRunner.CurrentEvent
                : null;

        string displayName =
            !string.IsNullOrWhiteSpace(data.EventName)
                ? data.EventName
                : rootEvent != null
                    ? rootEvent.EventName
                    : string.Empty;

        string displayDescription =
            !string.IsNullOrWhiteSpace(data.Description)
                ? data.Description
                : rootEvent != null
                    ? rootEvent.Description
                    : string.Empty;

        Sprite displayImage =
            data.EventImage != null
                ? data.EventImage
                : rootEvent != null
                    ? rootEvent.EventImage
                    : null;

        if (eventNameText != null)
        {
            eventNameText.gameObject.SetActive(true);
            eventNameText.SetText(displayName);
        }

        if (descriptionText != null)
        {
            descriptionText.gameObject.SetActive(true);
            descriptionText.SetText(displayDescription);
        }

        if (eventImage != null)
        {
            eventImage.sprite = displayImage;
            eventImage.enabled = displayImage != null;
        }

        if (choiceGroup != null)
        {
            choiceGroup.SetActive(true);
        }

        if (data.ShowOwnedMemos)
        {
            CreateMemoButtons();
        }
        else
        {
            memoPageIndex = 0;

            CreateChoiceButtons(
                data.Choices,
                data.DisplayType,
                data.MaximumVisibleChoices);
        }

        RefreshBackButton();
    }

    /// <summary>
    /// 현재 캐릭터가 보유한 메모를 기존 선택지 버튼에 자동으로 표시합니다.
    /// 메모가 5개를 넘으면 이전/다음 버튼을 포함해 페이지 단위로 표시합니다.
    /// </summary>
    private void CreateMemoButtons()
    {
        ClearChoiceButtons();
        memoInformation.Clear();

        FieldInformationInventory inventory =
            FieldInformationInventory.Get(Character);

        if (inventory != null)
        {
            foreach (FieldInformationData information in inventory.AcquiredInformation)
            {
                if (information != null && information.IsMemo)
                {
                    memoInformation.Add(information);
                }
            }
        }

        if (memoInformation.Count == 0)
        {
            if (descriptionText != null)
                descriptionText.SetText("보유한 메모가 없습니다.");

            if (choiceGroup != null)
                choiceGroup.SetActive(false);

            return;
        }

        if (choiceGroup != null)
            choiceGroup.SetActive(true);

        int buttonSlot = 0;

        if (memoInformation.Count <= MaximumChoiceButtonCount)
        {
            for (int i = 0; i < memoInformation.Count; i++)
            {
                SetMemoButton(
                    ref buttonSlot,
                    i,
                    GetMemoButtonText(memoInformation[i]));
            }

            return;
        }

        int pageCount =
            Mathf.CeilToInt(
                memoInformation.Count / (float)MemoPageSize);

        memoPageIndex =
            Mathf.Clamp(memoPageIndex, 0, pageCount - 1);

        if (memoPageIndex > 0)
        {
            SetMemoButton(
                ref buttonSlot,
                MemoPreviousButtonId,
                "이전 정보 목록");
        }

        int startIndex = memoPageIndex * MemoPageSize;
        int endIndex =
            Mathf.Min(startIndex + MemoPageSize, memoInformation.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            SetMemoButton(
                ref buttonSlot,
                i,
                GetMemoButtonText(memoInformation[i]));
        }

        if (memoPageIndex < pageCount - 1)
        {
            SetMemoButton(
                ref buttonSlot,
                MemoNextButtonId,
                "다음 정보 목록");
        }
    }

    private void SetMemoButton(ref int buttonSlot, int id, string text)
    {
        if (choiceButtons == null)
            return;

        while (buttonSlot < choiceButtons.Length &&
               choiceButtons[buttonSlot] == null)
        {
            buttonSlot++;
        }

        if (buttonSlot >= choiceButtons.Length)
            return;

        choiceButtons[buttonSlot].SetText(
            id,
            text,
            HandleMemoButtonSelected);

        buttonSlot++;
    }

    private static string GetMemoButtonText(FieldInformationData information)
    {
        if (information == null)
            return string.Empty;

        return string.IsNullOrWhiteSpace(information.Title)
            ? "이름 없는 메모"
            : information.Title;
    }

    private void HandleMemoButtonSelected(int id)
    {
        if (id == MemoPreviousButtonId)
        {
            memoPageIndex = Mathf.Max(0, memoPageIndex - 1);
            CreateMemoButtons();
            return;
        }

        if (id == MemoNextButtonId)
        {
            memoPageIndex++;
            CreateMemoButtons();
            return;
        }

        if (id < 0 || id >= memoInformation.Count)
            return;

        if (!eventRunner.ResolveInformationSelection(memoInformation[id]))
        {
            HandleChoiceFailed("선택한 정보를 확인할 수 없습니다.");
        }
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

            // 조건을 충족하지 못한 선택지는 존재 자체를 공개하지 않는다.
            if (!choice.CanSelect(eventRunner.CurrentContext))
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

        string resultDescription =
            choice != null
                ? choice.GetResultText(succeeded)
                : string.Empty;

        string checkDescription = CreateCheckResultText();

        FieldEventContext context = eventRunner != null ? eventRunner.CurrentContext : null;

        if (context != null)
        {
            resultDescription =
                context.BuildResultText(resultDescription);
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

        Sprite resultImage = choice != null ? choice.GetResultImage(succeeded) : null;

        BeginResultTransition(
            displayText,
            resultImage);
    }

    /// <summary>
    /// 결과가 확정된 뒤 지정된 시간만큼 기다리고 결과창을 엽니다.
    /// 대기 중에는 이 오브젝트를 활성 상태로 유지합니다.
    /// </summary>
    private void BeginResultTransition(string displayText, Sprite resultImage)
    {
        StopResultTransition();

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
        }

        if (resultDisplayDelay <= 0f)
        {
            ShowResultStory(
                displayText,
                resultImage);

            return;
        }

        resultTransitionRoutine = StartCoroutine(
            ShowResultAfterDelay(
                displayText,
                resultImage));
    }

    private IEnumerator ShowResultAfterDelay(string displayText, Sprite resultImage)
    {
        yield return new WaitForSecondsRealtime(
            resultDisplayDelay);

        resultTransitionRoutine = null;

        ShowResultStory(
            displayText,
            resultImage);
    }

    private void StopResultTransition()
    {
        if (resultTransitionRoutine == null)
            return;

        StopCoroutine(resultTransitionRoutine);
        resultTransitionRoutine = null;
    }

    /// <summary>
    /// 기존 UI_ChapterStory에 이벤트 결과를 표시하고
    /// 선택지 화면을 완전히 닫습니다.
    /// </summary>
    private void ShowResultStory(string displayText, Sprite resultImage)
    {
        if (resultStoryUI == null)
        {
            Debug.LogError(
                "UI_FieldEvent: Result Story UI가 연결되지 않아 " +
                "이벤트 결과를 표시할 수 없습니다.");

            return;
        }

        if (resultStoryUI.OpenEventResult(
            displayText,
            resultImage,
            HandleEventResultConfirmed))
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }

            return;
        }

        Debug.LogError(
            "UI_FieldEvent: 연결된 결과창을 활성화하지 못했습니다. " +
            "result의 부모 오브젝트 활성 상태를 확인하세요.");
    }


    /// <summary>
    /// 이벤트 결과 화면의 다음 버튼을 누르면
    /// 현재 필드 이벤트를 최종 완료합니다.
    /// </summary>
    private void HandleEventResultConfirmed()
    {
        if (eventRunner == null)
            return;

        eventRunner.CompleteCurrentEvent();
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

        bool canReturn =
            isShowingMemoContent ||
            (eventRunner != null && eventRunner.CanReturnToPreviousPage);

        backButton.gameObject.SetActive(canReturn);
    }

    /// <summary>
    /// 현재 하위 페이지에서 이전 페이지로 돌아간다.
    /// </summary>
    private void HandleBackButtonClicked()
    {
        if (eventRunner == null)
            return;

        if (isShowingMemoContent)
        {
            isShowingMemoContent = false;
            HandleEventDataChanged(eventRunner.CurrentData);
            return;
        }

        eventRunner.TryReturnToPreviousPage();
    }

    /// <summary>
    /// 이벤트 종료 시 카드 대기 상태와 화면 표시를 초기화한다.
    /// </summary>
    private void HandleEventClosed()
    {
        StopResultTransition();

        if (resultStoryUI != null &&
            resultStoryUI.IsShowingEventResult)
        {
            resultStoryUI.Close();
        }

        ClearChoiceButtons();

        Character = null;

        memoInformation.Clear();
        memoPageIndex = 0;
        isShowingMemoContent = false;

        if (backButton != null)
        {
            backButton.gameObject.SetActive(false);
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

        if (usedCard != null)
        {
            return
                $"{usedCard.cardName} 사용\n" +
                "판정 성공";
        }

        if (!eventRunner.HasLastJudgeResult)
            return string.Empty;

        JudgeResult result = eventRunner.LastJudgeResult;

        string resultName;

        if (result.fumble)
        {
            resultName = "펌블";
        }
        else
        {
            resultName = result.success ? "성공" : "실패";
        }

        return
            $"결과 {result.total} / 목표 {result.target}\n" +
            $"{resultName}";
    }

}
