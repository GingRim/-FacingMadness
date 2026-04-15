using System;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class UI_CharacterCreationScreen : UI_ScreenBase
{
    private void OnEnable()
    {
        InputManager.OnCancel -= CanelPause;
        InputManager.OnCancel += CanelPause;
    }

    private void OnDisable()
    {
        InputManager.OnCancel -= CanelPause;
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

    

