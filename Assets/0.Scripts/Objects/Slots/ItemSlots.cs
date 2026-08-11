using System;
using Unity.VisualScripting;
using UnityEngine;


public delegate void ItemSlotChangeEvent(ItemSlot changedSlot);
public class ItemSlot
{
    // 이 칸에 들어있는 아이템의 정보
    [SerializeField] ItemContainer item;
    // 이 칸 만의 정보
    [SerializeField] int currentStack;

    public event ItemSlotChangeEvent OnItemSlotChanged;

    public void NoticeChanged() => OnItemSlotChanged?.Invoke(this);

    public virtual bool Containable(ItemContainer wantItem)
    {
        if (!wantItem) return false;
        
        if (item && item != wantItem) return false;

        if (GetIsMax()) return false;

        return true;
       
    }
    public ItemContainer GetItem() => item;


    public int GatStack() => currentStack;

    public bool GetIsMax() => item ? currentStack >= item.maxStack : false;
    internal bool GetIsEmpty() => item is null || currentStack <= 0;

    public int AddItem(ItemContainer wantItem, int amount)
    {
        if (amount <= 0)
            return 0;

        if (!Containable(wantItem))
            return amount;

        item = wantItem;

        int available = Mathf.Max(item.maxStack - currentStack, 0);

        int addedAmount = Mathf.Min(amount, available);

        currentStack += addedAmount;

        return amount - addedAmount;
    }

    public int RemoveItem(ItemContainer wantItem)
    {
        if(!wantItem) return 0;

        if(GetIsEmpty()) return 0;

        if(item != wantItem) return 0;

        return Clear();
    }


    public int RemoveItem(ItemContainer wantItem, int amount)
    {
        if(amount <= 0) return 0;

        if(!wantItem) return 0;

        if (!wantItem) return amount;

        if (GetIsEmpty()) return amount;

        if (item != wantItem) return amount;

        if(amount >= currentStack) return amount - Clear();

        currentStack -= amount;

        return 0;
    }
    private int Clear()
    {
        item = null;
        int removed = currentStack;
        currentStack = 0;
        return removed;
    }
}
