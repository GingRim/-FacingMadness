using UnityEngine;

public class CardEX : MonoBehaviour
{
    [SerializeField] CharacterBase character;
    [SerializeField] UI_Hand handUI;

    public void DrawOneCard()
    {
        DeckModule deck = character.GetModule<DeckModule>();

        if (deck == null)
            return;

        CardData card = deck.Draw();

        if (card == null)
            return;

        handUI.AddCard(card);
    }
}

