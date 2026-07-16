using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class KeywordEncyclopedia : MonoBehaviour
{
    [Header("기본 카테고리 버튼")]
    [SerializeField]
    private Button cardButton;

    [SerializeField]
    private Button effectButton;

    [SerializeField]
    private Button etcButton;

    [Header("생성 버튼 영역")]
    [SerializeField]
    private Transform core;

    [SerializeField]
    private Button buttonTemplate;

    [Header("설명 출력 UI")]
    [SerializeField]
    private UI_KeywordHoverInfo keywordInfo;

    [Header("카드 도감 데이터")]
    [SerializeField]
    private EncyclopediaEntry[] cardEntries;

    [Header("상태 이상 도감 데이터")]
    [SerializeField]
    private EncyclopediaEntry[] effectEntries;

    [Header("고유 명사 도감 데이터")]
    [SerializeField]
    private EncyclopediaEntry[] etcEntries;

    // 현재 선택된 카테고리
    // 0 = 카드, 1 = 상태 이상, 2 = 고유 명사
    private int currentCategory = -1;

    private void Awake()
    {
        FindKeywordInfo();
        PrepareButtonTemplate();
        BindCategoryButtons();
    }

    private void Start()
    {
        // 도감이 처음 열렸을 때
        // 카드 카테고리를 기본으로 생성
        OpenCardCategory();
    }

    private void OnDestroy()
    {
        UnbindCategoryButtons();
    }

    /// <summary>
    /// 설명 출력 UI를 탐색한다.
    /// </summary>
    private void FindKeywordInfo()
    {
        if (keywordInfo != null)
            return;

        // 같은 오브젝트에서 우선 탐색
        keywordInfo =
            GetComponent<UI_KeywordHoverInfo>();

        if (keywordInfo != null)
            return;

        // 비활성화된 오브젝트를 포함해 전체 탐색
        keywordInfo =
            Object.FindFirstObjectByType<UI_KeywordHoverInfo>(
                FindObjectsInactive.Include);

        if (keywordInfo == null)
        {
            Debug.LogWarning(
                "KeywordEncyclopedia: UI_KeywordHoverInfo를 찾지 못했습니다.");
        }
    }

    /// <summary>
    /// Core 안의 원본 버튼을 생성용으로 준비한다.
    /// </summary>
    private void PrepareButtonTemplate()
    {
        if (buttonTemplate == null)
        {
            Debug.LogWarning(
                "KeywordEncyclopedia: ButtonTemplate이 연결되지 않았습니다.");

            return;
        }

        // 원본 버튼은 화면에 표시하지 않음
        buttonTemplate.gameObject.SetActive(false);
    }

    /// <summary>
    /// 기본 카테고리 버튼 연결.
    /// </summary>
    private void BindCategoryButtons()
    {
        if (cardButton != null)
        {
            cardButton.onClick.RemoveListener(OpenCardCategory);
            cardButton.onClick.AddListener(OpenCardCategory);
        }

        if (effectButton != null)
        {
            effectButton.onClick.RemoveListener(OpenEffectCategory);
            effectButton.onClick.AddListener(OpenEffectCategory);
        }

        if (etcButton != null)
        {
            etcButton.onClick.RemoveListener(OpenEtcCategory);
            etcButton.onClick.AddListener(OpenEtcCategory);
        }
    }

    /// <summary>
    /// 기본 카테고리 버튼 연결 해제.
    /// </summary>
    private void UnbindCategoryButtons()
    {
        if (cardButton != null)
        {
            cardButton.onClick.RemoveListener(OpenCardCategory);
        }

        if (effectButton != null)
        {
            effectButton.onClick.RemoveListener(OpenEffectCategory);
        }

        if (etcButton != null)
        {
            etcButton.onClick.RemoveListener(OpenEtcCategory);
        }
    }

    private void OpenCardCategory()
    {
        CreateCategoryButtons(
            0,
            cardEntries);
    }

    private void OpenEffectCategory()
    {
        CreateCategoryButtons(
            1,
            effectEntries);
    }

    private void OpenEtcCategory()
    {
        CreateCategoryButtons(
            2,
            etcEntries);
    }

    /// <summary>
    /// 선택한 카테고리의 버튼을 생성한다.
    /// </summary>
    private void CreateCategoryButtons(
        int category,
        EncyclopediaEntry[] entries)
    {
        // 같은 카테고리를 다시 누르면
        // 기존 버튼을 유지하고 아무 작업도 하지 않음
        if (currentCategory == category)
            return;

        if (core == null)
        {
            Debug.LogWarning(
                "KeywordEncyclopedia: Core가 연결되지 않았습니다.");

            return;
        }

        if (buttonTemplate == null)
        {
            Debug.LogWarning(
                "KeywordEncyclopedia: ButtonTemplate이 연결되지 않았습니다.");

            return;
        }

        currentCategory = category;

        ClearGeneratedButtons();
        CreateButtons(entries);
    }

    /// <summary>
    /// 이전 카테고리에서 생성된 버튼을 제거한다.
    /// 생성 원본인 ButtonTemplate은 제거하지 않는다.
    /// </summary>
    private void ClearGeneratedButtons()
    {
        for (int i = core.childCount - 1; i >= 0; i--)
        {
            Transform child =
                core.GetChild(i);

            if (child.gameObject ==
                buttonTemplate.gameObject)
            {
                continue;
            }

            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// 설정된 도감 데이터 수만큼 버튼을 생성한다.
    /// </summary>
    private void CreateButtons(
        EncyclopediaEntry[] entries)
    {
        if (entries == null)
            return;

        foreach (EncyclopediaEntry entry in entries)
        {
            if (entry == null)
                continue;

            Button createdButton =
                Instantiate(buttonTemplate, core);

            createdButton.gameObject.SetActive(true);

            createdButton.name =
                $"EncyclopediaButton_{entry.ButtonName}";

            TextMeshProUGUI buttonText =
                createdButton.GetComponentInChildren<TextMeshProUGUI>(true);

            if (buttonText != null)
            {
                buttonText.SetText(
                    entry.ButtonName);
            }

            // 반복문 데이터가 정확히 유지되도록
            // 현재 항목을 별도 변수에 저장
            EncyclopediaEntry selectedEntry =
                entry;

            createdButton.onClick.AddListener(
                () => SelectEntry(selectedEntry));
        }
    }

    /// <summary>
    /// 생성된 도감 버튼을 선택했을 때
    /// 이름과 설명을 출력한다.
    /// </summary>
    private void SelectEntry(
        EncyclopediaEntry entry)
    {
        if (entry == null)
            return;

        if (keywordInfo == null)
        {
            FindKeywordInfo();
        }

        if (keywordInfo == null)
        {
            Debug.LogWarning(
                "KeywordEncyclopedia: UI_KeywordHoverInfo가 연결되지 않았습니다.");

            return;
        }

        keywordInfo.SetEncyclopediaInfo(
            entry.DisplayName,
            entry.Description);
    }
}
