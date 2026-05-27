using System;
using UnityEngine;

public class ItemSlot
{
    // 이 칸에 들어있는 아이템의 정보
    [SerializeField] ItemContainer item;
    // 이 칸 만의 정보
    [SerializeField] int currentStack;

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

        return amount;//남은 값을 돌려준다.
    }
}
