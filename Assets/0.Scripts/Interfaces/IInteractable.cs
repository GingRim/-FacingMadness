using UnityEngine;


    public enum InteractType
    {
        None,
        Talk, Trade, Take, 
        _Length
    }

public interface IInteractable
{
    public bool IsInteractable(GameObject from);
    public string GetInteractText(GameObject from);
    public InteractType GetInteractType();
    public void Interact(GameObject from);
    public void StopInteract(GameObject from);
}
