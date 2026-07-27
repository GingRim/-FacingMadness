using UnityEngine;

public class UI_BattleScreen : UI_ScreenBase
{
    private UI_CardUseSelect cardUseSelect;
    private CardCrkClick cardClick;
    private UI_Hand handUI;

    private UI_ReactionSelect reactionSelect;
    public UI_ReactionSelect ReactionSelect => reactionSelect;

    private UI_BattleLog battleLog;
    public UI_BattleLog BattleLog => battleLog;

    private void Awake()
    {
        cardClick = GetComponentInChildren<CardCrkClick>(true);
        handUI = GetComponentInChildren<UI_Hand>(true);

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

        cardUseSelect = popupObj.GetComponent<UI_CardUseSelect>();

        if (cardUseSelect != null)
        {
            cardUseSelect.SetHandUI(handUI);
            cardUseSelect.Close();
        }

        if (cardClick != null)
        {
            cardClick.SetUseSelectUI(cardUseSelect);
        }
    }

    private void OnEnable()
    {
        InputManager.OnPause -= CanelPause;
        InputManager.OnPause += CanelPause;
    }

    private void OnDisable()
    {
        InputManager.OnPause -= CanelPause;
    }


    void CanelPause(bool value)
    {
        if (!value)
            return;

        UI_KeywordHoverInfo encyclopedia =
            UIManager.GetUIM2(UIType.ExperimentHoverInfp)
            as UI_KeywordHoverInfo;

        // 도감이 열려 있으면 도감만 닫고
        // 일시정지 창은 열지 않음
        if (encyclopedia != null && encyclopedia.IsOpen)
        {
            encyclopedia.Close();
            return;
        }

        OpenableUIBase pauseUI =
            UIManager.GetUIM2(UIType.Pause)
            as OpenableUIBase;

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
