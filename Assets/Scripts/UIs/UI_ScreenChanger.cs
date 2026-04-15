using System;
using UnityEngine;

public class UI_ScreenChanger : OpenableUIBase
{
    [SerializeField] Animator anim;
    Action AnimEndFunction;
    public void ChangeStart(Action newFunction = null)
    {
        AnimEndFunction = newFunction;
        if (anim) anim.SetTrigger("Out");
        else OnAnimEnd();
    }

    public void ChangeEnd(Action newFunction = null)
    {
        AnimEndFunction = newFunction;
        if(anim) anim.SetTrigger("IN");
        else OnAnimEnd();
    }

    public void OnAnimEnd()
    {
        AnimEndFunction?.Invoke();
    }
}
