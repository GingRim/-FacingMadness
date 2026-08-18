using System;
using System.Collections.Generic;
using UnityEngine;

public class MythTurnContext
{
    public FieldManager FieldManager { get; }

    public int TurnNumber { get; }

    public CharacterBase CurrentPlayer => FieldManager != null ? FieldManager.CurrentPlayer : null;

    public FieldNode CurrentNode => FieldManager != null ? FieldManager.CurrentNode : null;

    public IReadOnlyList<CharacterBase> Participants => FieldManager != null ? FieldManager.Participants : Array.Empty<CharacterBase>();

    public MythTurnContext(FieldManager fieldManager, int turnNumber)
    {
        FieldManager = fieldManager;
        TurnNumber = turnNumber;
    }
}
