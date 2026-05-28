using System;
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

    public virtual bool Containable(ItemContainer newitem)
    {
        if (item) return true;
        else      return false;
    }
    public ItemContainer GetItem() => item;


    public int GatStack() => currentStack;


    public bool GetIsMax() => item ? currentStack >= item.maxStack : false;

    public int AddItem(ItemContainer wantItem, int amount)
    {
        if (wantItem is null) return 0;
        if (amount <= 0) return 0;
        if (item is not null && item != wantItem) return amount;

        item = wantItem;
        //넣을 수 있는 만큼만 넣어야 한다.
        int stackable = Mathf.Max(item.maxStack - currentStack, 0);
        currentStack += stackable;

        return stackable;//남은 값을 돌려준다.
    }

    internal bool GetIsEmpty() => item is null || currentStack <= 0;
}
