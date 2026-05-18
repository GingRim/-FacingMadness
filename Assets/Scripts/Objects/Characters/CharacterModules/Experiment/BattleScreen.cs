using UnityEngine;

public class BattleScreen : MonoBehaviour
{

    [SerializeField] EX_CardDrawButtonTest drawButton;

    [SerializeField] CharacterBase fieldCharacter;

    private void Start()
    {
        drawButton.SetCharacter(fieldCharacter);
    }
}
