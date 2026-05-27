using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlotinfo : UIBase
{
    [SerializeField] Image iconImage;
    [SerializeField] TextMeshProUGUI amountText;

    [SerializeField] Sprite noneIcon;
    ItemSlot connectedSlot;

    public void Connectslot(ItemSlot targetSlot)
    {
        if(targetSlot is null) return;
        connectedSlot = targetSlot;
        VisualUpdate(connectedSlot);
    }

    protected virtual void VisualUpdate(ItemSlot targetSlot)
    {
        if(connectedSlot is null) return;
        ItemContainer targetItem = targetSlot.GetItem();
        if (iconImage)
        {
            if (targetItem)
            {
                iconImage.sprite = targetItem.icon ?? noneIcon;
                iconImage.enabled = true; //아이템이 있어야 이미지가 켜짐
            }
            else
            {
                iconImage.enabled = false;
            }
        }
        if (amountText)
        {
            int targetStack = targetSlot.GatStack();
            if (!targetItem || targetItem.maxStack <= 1 || targetStack <= 1)
            {
                amountText.SetText("");
            }
            else
            {
                amountText.SetText($"{targetStack}");
            }
        }
    }
}
