using UnityEngine;

public class UI_ScreenChanger : OpenableUIBase
{
    [SerializeField] Animator anim;

    public void ChangeStart()
    {
        anim?.SetTrigger("Out");
    }

    public void ChangeEnd()
    {
        anim?.SetTrigger("IN");
    }
}
