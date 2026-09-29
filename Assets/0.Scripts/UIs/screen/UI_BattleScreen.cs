using UnityEngine;

public class UI_BattleScreen : UI_ScreenBase
{
    private UI_CardUseSelect cardUseSelect;
    private UI_Hand handUI;
    public UI_Hand HandUI => handUI;
    private BattleCardUseController battleCardUseController;

    private UI_ReactionSelect reactionSelect;
    public UI_ReactionSelect ReactionSelect => reactionSelect;

    private UI_BattleLog battleLog;
    public UI_BattleLog BattleLog => battleLog;

    private void Awake()
    {
        handUI = GetComponentInChildren<UI_Hand>(true);

        if (handUI == null)
        {
            GameObject handObject = ObjectManager.CreateObject("Hand", transform);

            if (handObject != null)
                handUI = handObject.GetComponent<UI_Hand>();
        }

        battleCardUseController = GetComponent<BattleCardUseController>();

        if (battleCardUseController == null)
        {
            battleCardUseController = gameObject.AddComponent<BattleCardUseController>();
        }

        reactionSelect = GetComponentInChildren<UI_ReactionSelect>(true);

        battleLog = GetComponentInChildren<UI_BattleLog>(true);

        if (battleLog == null)
        {
            Debug.LogWarning("UI_BattleScreen: UI_BattleLog를 찾지 못했습니다.");
        }

        if (reactionSelect != null)
        {
            reactionSelect.Close();
        }

        GameObject popupObj = ObjectManager.CreateObject("Resolver", transform);

        if (popupObj != null)
        {
            cardUseSelect = popupObj.GetComponent<UI_CardUseSelect>();
        }
        else
        {
            Debug.LogWarning(
                "UI_BattleScreen: " +
                "Resolver UI를 생성하지 못했습니다.");
        }

        if (cardUseSelect != null)
        {
            cardUseSelect.SetHandUI(handUI);
            cardUseSelect.Close();
        }

        if (battleCardUseController != null)
        {
            battleCardUseController.Configure(cardUseSelect, handUI);
        }
    }

    private void OnEnable()
    {
        InputManager.OnPause -= CanelPause;
        InputManager.OnPause += CanelPause;

        BattleManager.OnBattleStarted -= RefreshPlayerUI;
        BattleManager.OnBattleStarted += RefreshPlayerUI;

        RefreshPlayerUI();
    }

    private void OnDisable()
    {
        InputManager.OnPause -= CanelPause;
        BattleManager.OnBattleStarted -= RefreshPlayerUI;
    }

    private void RefreshPlayerUI()
    {
        CharacterBase player = FindPlayerCharacter();

        if (player == null)
        {
            Debug.LogWarning("UI_BattleScreen: 플레이어를 찾지 못했습니다.");
            return;
        }

        DeckModule deck = player.GetModule<DeckModule>();

        if (handUI != null && deck != null)
            handUI.RefreshFromDeck(deck);

        UI_Cost[] costDisplays = GetComponentsInChildren<UI_Cost>(true);

        foreach (UI_Cost display in costDisplays)
        {
            if (display != null && display.CostType == CostType.Action)
                display.SetCharacter(player);
        }
    }

    private CharacterBase FindPlayerCharacter()
    {
        PlauerController[] controllers =
            FindObjectsByType<PlauerController>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

        foreach (PlauerController controller in controllers)
        {
            if (controller != null && controller.Character != null)
                return controller.Character;
        }

        return null;
    }


    void CanelPause(bool value)
    {
        if (!value)
            return;

        UI_KeywordHoverInfo encyclopedia = UIManager.GetUIM2(UIType.ExperimentHoverInfp) as UI_KeywordHoverInfo;

        // 도감이 열려 있으면 도감만 닫고
        // 일시정지 창은 열지 않음
        if (encyclopedia != null && encyclopedia.IsOpen)
        {
            encyclopedia.Close();
            return;
        }

        OpenableUIBase pauseUI = UIManager.GetUIM2(UIType.Pause) as OpenableUIBase;

        if (pauseUI == null)
            return;

        pauseUI.Toggle();
    }

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);

        BattleManager.OnBattleLog -= AddBattleLog;
        BattleManager.OnBattleLog += AddBattleLog;

        BattleManager.OnBattleLogClear -= ClearBattleLog;
        BattleManager.OnBattleLogClear += ClearBattleLog;
    }

    public override void Unregistration(UIManager manager)
    {
        BattleManager.OnBattleLog -= AddBattleLog;
        BattleManager.OnBattleLogClear -= ClearBattleLog;

        base.Unregistration(manager);
    }

    private void AddBattleLog(string message)
    {
        if (battleLog == null)
            return;

        battleLog.AddLog(message);
    }

    private void ClearBattleLog()
    {
        if (battleLog == null)
            return;

        battleLog.Clear();
    }

}
