using System;
using UnityEngine;

public class PlauerController : ControllerBase
{
    protected override void OnPossess(CharacterBase newCgaracter)
    {
        base.OnPossess(newCgaracter);
        InputManager.OnMouseRightButton -= MoveToMousePosition;
        InputManager.OnMouseRightButton += MoveToMousePosition;
        InputManager.OnMove -= MoveToMousePosition;
        InputManager.OnMove += MoveToMousePosition;
    }

    private void MoveToMousePosition(Vector2 value)
    {
        CommandMoveToDirection(value);
    }

    protected override void OnUnpossess(CharacterBase oldCgaracter)
    {
        base.OnUnpossess(oldCgaracter);
        InputManager.OnMouseRightButton -= MoveToMousePosition;
    }

    public void MoveToMousePosition(bool value, Vector2 screenPosition, Vector3 WorldPosition)
    {
        CommandMoveToDestination(WorldPosition, 0.0f);
    }
}
