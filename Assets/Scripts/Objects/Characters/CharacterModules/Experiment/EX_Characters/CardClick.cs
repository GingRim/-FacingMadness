using UnityEngine;

public class CardCrkClick : MonoBehaviour
{
    
    [SerializeField] private CharacterBase user;
    [SerializeField] private CharacterBase target;

    private void OnEnable()
    {
        InputManager.OnMouseLeftButton -= OnClick;
        InputManager.OnMouseLeftButton += OnClick;
    }

    private void OnDisable()
    {
        InputManager.OnMouseLeftButton -= OnClick;
    }

    private void OnClick(bool value, Vector2 screenPosition, Vector3 worldPosition)
    {
        if (!value)
            return;

        GameObject clickedObject =
            GameManager.Instance.Input.GetGameObjectUnderCursor();

        UI_Card card =
            clickedObject?.GetComponentInParent<UI_Card>();

        if (card == null)
            return;

        Debug.Log($"카드 클릭: {card.CardData.cardName}");

        CardResolver resolver = new CardResolver();

        
    }
}

