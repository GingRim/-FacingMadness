using UnityEngine;

public class UI_Button_OpenUI : MonoBehaviour
{
    [SerializeField] UIType wantType;
    [SerializeField] bool wantToggle;


    public void Open()
    {
        if (wantToggle) UIManager.ToggleUIM2(wantType);
        else UIManager.OpenUIM2(wantType);
    }

    public void Close()
    {
        if (wantToggle) UIManager.ToggleUIM2(wantType);
        else UIManager.CloseUIM2(wantType);
    }
}
