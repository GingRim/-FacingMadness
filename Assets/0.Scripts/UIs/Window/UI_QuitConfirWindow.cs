using UnityEngine;

public class UI_QuitConfirWindow : OpenableUIBase
{
  public void Confirm()
    {
        GameManager.QuitGame();
    }

    public void Cancel()
    {
        UIManager.CloseUIM2(UIType.Quit);
    }
}
