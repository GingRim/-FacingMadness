using UnityEngine;
using UnityEngine.UI;

public class UI_FieldCharacterMarker : MonoBehaviour
{
    [SerializeField]
    private Image characterImage;

    private CharacterBase character;

    public CharacterBase Character => character;

    public void SetCharacter(CharacterBase newCharacter)
    {
        character = newCharacter;

        if (characterImage != null)
        {
            characterImage.sprite = character != null ? character.Icon : null;

            characterImage.enabled = characterImage.sprite != null;
        }

        gameObject.SetActive(character != null);
    }

    public void MoveToNode(FieldNode node)
    {
        if (node == null)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.SetParent(node.MarkerRoot, false);

        gameObject.SetActive(true);
    }

    public void Clear()
    {
        character = null;

        if (characterImage != null)
        {
            characterImage.sprite = null;
            characterImage.enabled = false;
        }

        gameObject.SetActive(false);
    }
}