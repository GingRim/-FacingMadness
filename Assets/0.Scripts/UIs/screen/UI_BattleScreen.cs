using UnityEngine;

public class UI_BattleScreen : UI_ScreenBase
{
    private UI_CardUseSelect cardUseSelect;
    private CardCrkClick cardClick;
    private UI_Hand handUI;

    private UI_ReactionSelect reactionSelect;
    public UI_ReactionSelect ReactionSelect => reactionSelect;

    private void Awake()
    {
        cardClick = GetComponentInChildren<CardCrkClick>(true);
        handUI = GetComponentInChildren<UI_Hand>(true);

        reactionSelect = GetComponentInChildren<UI_ReactionSelect>(true);

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

}
