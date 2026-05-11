using UnityEngine;

public class UI_BattleScreen : UI_ScreenBase
{
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
