using System;
using UnityEngine;

[Serializable]
public class EncyclopediaEntry
{
    [SerializeField]
    private string buttonName;

    [SerializeField]
    private string displayName;

    [SerializeField, TextArea(3, 10)]
    private string description;

    public string ButtonName => buttonName;
    public string DisplayName => displayName;
    public string Description => description;
}