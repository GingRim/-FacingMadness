using UnityEngine;
using UnityEngine.TextCore.Text;

public class TEX : MonoBehaviour
{
    [SerializeField] private CharacterBase character;
    private void Start()
    {

  
    
        DeckModule deck = character.GetModule<DeckModule>();

        if (deck == null)
        {
            Debug.Log("DeckModule 없음");
            return;
        }

        CardData card = deck.Draw();

        if (card == null)
            Debug.Log("드로우 실패");
        else
            Debug.Log($"드로우 성공: {card.cardName}");
    
    }
}
