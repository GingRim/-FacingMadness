using UnityEngine;

public enum ItemType
{
    None, Equipment, Consumable, Material, Miscellaneous, Quest, Important,
    _Length
}

[CreateAssetMenu(fileName = "ItemContainer", menuName = "Item/ItemBase")]
public class ItemContainer : InfoContainer
{
    public ItemType type;
    public int maxStack;
    public float weight;
}
