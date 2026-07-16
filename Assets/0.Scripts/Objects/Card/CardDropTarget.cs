using UnityEngine;

public class CardDropTarget : MonoBehaviour
{
    private CharacterBase cachedCharacter;

    public CharacterBase Character
    {
        get
        {
            if (cachedCharacter != null)
                return cachedCharacter;

            cachedCharacter = GetComponentInParent<CharacterBase>();

            if (cachedCharacter == null)
            {
                cachedCharacter = GetComponentInChildren<CharacterBase>();
            }

            return cachedCharacter;
        }
    }

    public bool TryGetCharacter(out CharacterBase character)
    {
        character = Character;
        return character != null;
    }
}
