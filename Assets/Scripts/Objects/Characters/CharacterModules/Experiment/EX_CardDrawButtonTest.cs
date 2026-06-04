using UnityEngine;

public class EX_CardDrawButtonTest : MonoBehaviour
{
    private CharacterBase character;

    [SerializeField] private UI_Hand handUI;

    private CharacterBase FindControlledCharacter()
    {
        CharacterBase[] characters =
            FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);

        foreach (CharacterBase current in characters)
        {
            if (current.Controller != null)
                return current;
        }

        return null;
    }

    public void DrawCardToHand()
    {
        character = FindControlledCharacter();

        if (character == null)
        {
            Debug.LogError("컨트롤러가 연결된 캐릭터가 없습니다.");
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

        handUI.ClearHand();

        foreach (CardData handCard in deck.Hand)
        {
            handUI.AddCard(handCard);
        }
    }

}

