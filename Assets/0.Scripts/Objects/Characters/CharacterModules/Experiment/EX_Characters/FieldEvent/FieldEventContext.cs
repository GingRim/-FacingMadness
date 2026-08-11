using UnityEngine;

/// <summary>
/// 이벤트 실행 정보
/// </summary>
public class FieldEventContext
{
    public CharacterBase Player { get; }
    public FieldNode Node { get; }
    public FieldManager FieldManager { get; }

    public string ResultTextOverride { get; private set; }

    public bool HasResultTextOverride => !string.IsNullOrEmpty(ResultTextOverride);

    public CharacterBase Character => FieldManager != null ? FieldManager.CurrentPlayer : null;

    public void SetResultText(string resultText)
    {
        ResultTextOverride = resultText;
    }

    public void ClearResultText()
    {
        ResultTextOverride = string.Empty;
    }

    public FieldEventContext(CharacterBase player, FieldNode node, FieldManager fieldManager)
    {
        Player = player;
        Node = node;
        FieldManager = fieldManager;
    }

    public CardData SelectedCard { get; private set; }

    public void SetSelectedCard(CardData card)
    {
        SelectedCard = card;
    }

    public void ClearSelectedCard()
    {
        SelectedCard = null;
        ClearCardCheck();
    }


    public FieldCardCheckData CardCheck { get; private set; }

    public bool HasCardCheck { get; private set; }

    public void SetCardCheck(FieldCardCheckData checkData)
    {
        CardCheck = checkData;
        HasCardCheck = true;
    }

    public void ClearCardCheck()
    {
        CardCheck = default;
        HasCardCheck = false;
    }

    public Inventory Inventory
    {
        get
        {
            if (Character == null)
                return null;

            return Character.GetComponentInChildren<Inventory>(true);
        }
    }

}
