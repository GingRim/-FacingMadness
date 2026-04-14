using System.ComponentModel;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class UI_CharacterCreationScreen : UI_ScreenBase
{
     public void Pause()
     {
        UIManager.OpenUIM2(UIType.Pause);
        GameManager.Pause();
     }
    
    public void UnPause()
    {
        UIManager.CloseUIM2(UIType.Pause);
        GameManager.Unpause();
    }
}
