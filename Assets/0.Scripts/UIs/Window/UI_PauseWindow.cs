using System;
using UnityEditor;
using UnityEngine;

[Serializable]


public class UI_PauseWindow : OpenableUIBase
{
    [SerializeField] UIClaim[] requiredUI;
    public override void Registration(UIManager manager)
    {
        base.Registration(manager);

        if (requiredUI is null) return;
        foreach (UIClaim currentClaim in requiredUI)
        {
            currentClaim.Execute();
        }
    }
}

