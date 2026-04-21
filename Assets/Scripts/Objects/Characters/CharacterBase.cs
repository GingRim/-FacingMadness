using UnityEngine;
using UnityEngine.TextCore.Text;

public class CharacterBase : MonoBehaviour
{
    //가장 중요한 기능!
    //말을 했을 때 말을 잘 들어먹는 것
    ControllerBase _controller;
    public ControllerBase Controller => _controller;

    public virtual string DisplayName => "Character";

    public ControllerBase Possessed(ControllerBase from) //Possessed = 빙의되다.
    {
        //영혼이 있다면 다른 영혼이 있을 수 있는가?
        if (Controller) UnPossessed();
        _controller = from;
        OnPossessed(Controller);
        return Controller;
    }

   

    public virtual void OnPossessed(ControllerBase newController){ }

    public void UnPossessed()
    {
        if(Controller)OnUnPossessed(Controller);
        _controller = null;
    }

    public virtual void OnUnPossessed(ControllerBase oldController) { }
    public bool UnPossessed(ControllerBase oldController)
    {
        if (Controller != oldController) return false;
        UnPossessed();
        return true;
    }


}