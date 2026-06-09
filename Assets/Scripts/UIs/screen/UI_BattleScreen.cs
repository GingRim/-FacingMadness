using UnityEngine;

public class UI_BattleScreen : UI_ScreenBase
{
    private UI_CardUseSelect cardUseSelect;
    private CardCrkClick cardClick;
    private UI_Hand handUI;


    private void Awake()
    {
        cardClick = GetComponentInChildren<CardCrkClick>(true);
        handUI = GetComponentInChildren<UI_Hand>(true);

        GameObject popupObj =
            ObjectManager.CreateObject("Resolver", transform);

        cardUseSelect =
            popupObj.GetComponent<UI_CardUseSelect>();

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
        if (UIManager.GetUIM2(UIType.Pause).isActiveAndEnabled)
        {
            UIManager.CloseUIM2(UIType.Pause);
        }
        else
        {
            UIManager.ToggleUIM2(UIType.Pause);
        }

    }

}
