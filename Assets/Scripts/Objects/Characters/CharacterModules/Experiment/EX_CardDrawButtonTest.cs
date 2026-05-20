using UnityEngine;

public class EX_CardDrawButtonTest : MonoBehaviour

{
    private CharacterBase character;
    [SerializeField] private UI_Hand handUI;
    private void Start()
    {
        character = FindFirstObjectByType<CharacterBase>();
    }

    public void SetCharacter(CharacterBase newCharacter)
    {
        character = newCharacter;
    }

    public void DrawCardToHand()
    {
        if (character == null)
        {
            Debug.LogError("CharacterBase가 연결되지 않았습니다.");
            return;
        }

        if (handUI == null)
        {
            Debug.LogError("UI_Hand가 연결되지 않았습니다.");
            return;
        }

        DeckModule deck = character.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.LogError("DeckModule을 찾지 못했습니다.");
            return;
        }

        CardData card = deck.Draw();

        if (card == null)
        {
            Debug.LogWarning("드로우할 카드가 없습니다.");
            return;
        }

        handUI.AddCard(card);
    }
}

