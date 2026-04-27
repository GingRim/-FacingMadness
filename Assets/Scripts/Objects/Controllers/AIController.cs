using UnityEngine;

public abstract class AIController : ControllerBase
{
    [SerializeField] GameObject _focusTarget = null;
    public GameObject FocusTarget => _focusTarget;

    protected abstract void Think(float deltaTime);
    
    public GameObject SetFocusTarget(GameObject newTarget)
    {
        if (IsFocussable(newTarget))
        {
            _focusTarget = newTarget;
            OnFocusTargetChanged(FocusTarget, newTarget);
        }

            return _focusTarget;

    }
    
    protected virtual bool IsFocussable(GameObject target) => target != _focusTarget;

   protected virtual void OnFocusTargetChanged(GameObject oldTarget, GameObject newTarget)
    {

    }



}
