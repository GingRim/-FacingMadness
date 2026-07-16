using System;
using TMPro;
using UnityEngine;


public class UI_Cost : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentName;
    [SerializeField] private CostType costName;
    
    private CostModule costModule;

    public CostType CostType => costName;

    private void Start()
    {
        SetCharacter(FindControlledCharacter());
    }

    private CharacterBase FindControlledCharacter()
    {
        CharacterBase[] characters =
            FindObjectsByType<CharacterBase>(FindObjectsSortMode.None);

        foreach (CharacterBase character in characters)
        {
            if (character.Controller != null)
                return character;
        }
        return null;
    }

    public void SetCharacter(CharacterBase character)
    {
        if (character == null)
            return;

        costModule = character.GetModule<CostModule>();

        Refresh();
    }

    public void Refresh()
    {
        if (costModule == null || currentName == null)
            return;

        int current = costModule.GetCurrent(CostType);
        int max = costModule.GetMax(CostType);

        currentName.SetText(current.ToString());
    }

    private void Update()
    {
        Refresh();
    }
}
