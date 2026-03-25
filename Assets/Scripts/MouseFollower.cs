using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    void Start()
    {
        InputManager.OnMouseRightUp += DestroyOnmouse;
        InputManager.OnMouseLeftDown += CeateToMouse;
    }

    void CeateToMouse(Vector2 screenPosution, Vector3 worldposition)
    {
        Instantiate(DataManager.LoadDataFile<GameObject>("Square 4"), worldposition, Quaternion.identity);
    }

    void MoveToMouse(Vector2 screenPosition, Vector3 worldposition)
    {
        transform.position = worldposition;
    }

    void DestroyOnmouse(Vector2 screenPosition, Vector3 worldposition)
    {
        Debug.Log(GameManager.Instance.Input.GetGameObjectUnderCursor());
    }
}
