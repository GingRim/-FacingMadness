using UnityEngine;

public class ChaseAiController : AIController
{
    protected override void OnPossess(CharacterBase newCgaracter)
    {
        GameManager.OnUpdateEventController -= Think;
        GameManager.OnUpdateEventController += Think;
    }

    protected override void OnUnpossess(CharacterBase oldCgaracter)
    {
        GameManager.OnUpdateEventController -= Think;
    }
    protected override void Think(float deltaTime)
    {
        if(!FocusTarget) return;
        CommandMoveToDestination(FocusTarget.transform.position, 1.0f); //이동하라 명령

    }
}
