using UnityEngine;

public class UI_Button_OpenScreen : MonoBehaviour
{

    [SerializeField] UIType wantType;
    [SerializeField] ScreenChangeType cgangeType;
    public void Open()
    {
        UIManager.OpenScreenM2(wantType, cgangeType);
    }
}
