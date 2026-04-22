using UnityEngine;

public class ControllerBase : MonoBehaviour, IFunctionable
{
    CharacterBase _character;
    public CharacterBase Character => _character;

    //ChracterBase
    public void Possess(CharacterBase target)
    {
        if(!target) return; //대상이 없습니다.
        ControllerBase result = target.Possessed(this);//this = 자신
        //"내"가 당첨되었어! => 제대로 빙의가 된 거구나
        if (result == this)
        {
            _character = target;
            OnPossess(target);
        }
    }
    protected virtual void OnPossess(CharacterBase newCgaracter) { }

    public void UnPossess()
    {
        if (Character)
        {
            if (Character.UnPossessed(this)) // 이미 주인이 바뀌었다면? 집을 팔고자 한다. 집주인이 바뀐 상황
            {
                OnUnpossess(Character);
            }
        }
            _character = null;
    }

    protected virtual void OnUnpossess(CharacterBase oldCgaracter) { }

    public void RegistrationFunctions()
    {
        Possess(GetComponent<CharacterBase>());
    }

    public void UnRegistrationFunctions()
    {
        UnPossess();
    }

    public void CommandMoveToDirection(Vector3 direction)
    {
        if(Character is IRunnable target)
        {
            target.MoveToDirection(direction);
        }

    }
    public void CommandMoveToDestination(Vector3 direction, float tolerance) 
    {
         if(Character is IRunnable target)
         { 
             target.MoveToDestination(direction, tolerance);
         }
    
    }
    public void CommandStop()
    {
        if(Character is IRunnable target)
        {
            target.StopMovement();
        }
    }
}
