using UnityEngine;

public class UI_Button_OpenScreen : MonoBehaviour
{

    [SerializeField] UIType wantType;
    public void Open()
    {
        UIManager.OpenScreenM2(wantType);
    }
}
