using UnityEngine;

public class AnimationModule : CharacterModule
{
    [SerializeField] Animator ainm;
    [SerializeField] bool isRotationByMovement;

    public sealed override System.Type RegistrationType => typeof(AnimationModule);


    public override void OnRegistration(CharacterBase newOwner)
    {
        base.OnRegistration(newOwner);
        newOwner.OnLookAt -= AnimationByLookRotation;
        newOwner.OnLookAt += AnimationByLookRotation;
        newOwner.OnMovement -= AnimationByMovement;
        newOwner.OnMovement += AnimationByMovement;
    }


    public override void OnUnregistration(CharacterBase oldOwner)
    {
        base.OnUnregistration(oldOwner);
        oldOwner.OnLookAt -= AnimationByLookRotation;
        oldOwner.OnMovement -= AnimationByMovement;
    }

    public void AnimationByLookRotation(Vector3 lookRotation)
    {
        if (!ainm) { return; }
        ainm.SetFloat("MoveX", lookRotation.x);
        ainm.SetFloat("MoveY", lookRotation.y);

    }

    public void AnimationByMovement(Vector3 moveDelta)
    {
        if (!ainm) { return; }
        if(isRotationByMovement && moveDelta.sqrMagnitude > 0)
        {
            AnimationByLookRotation(moveDelta);
        }
        ainm.SetFloat("MoveSpeed", moveDelta.magnitude / Time.fixedDeltaTime);

    }
}
