using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Inventory : MonoBehaviour
{
    //몇 칸인지?
    //칸 제한을 걸기 위해서 필요한 두가지의 숫자
    //가로개수 세로개수
    //Columns(열) Rows(행)
    public int columns;
    public int rows;

    //아이템 슬롯을 columnd와 rows 개수만큼 준비해야해요.
    //2차원 행렬을 준비!
    //대상을 여러개 저장, 개수가 바뀌지 않고, 순환하는데에 빨라야 해요!
    //배열(Array)
    // [1,2]
    ItemSlot[,] slots;

    public void Initialized()
    {
        slots = new ItemSlot[columns, rows];
        for (int r = 0; r < columns; r++) 
        { 
            for (int c = 0; c < rows; c++)
            {
                slots[r, c] = new ItemSlot();
            }
        }
    }
    /// <summary>
    /// 정렬하다.
    /// </summary>
    public void Sort(System.Comparison<ItemContainer> Method)
    {

    }
    /// <summary>
    /// 빠른 자동 정렬
    /// </summary>
    public void AutoQuickInsert(Inventory other)
    {

    }

    public void AutoQuickInsert(Inventory[] other)
    {

    }
    /// <summary>
    /// 모든 아이템 가져가기
    /// </summary>
    public bool InsertAll(Inventory other)
    {
        return default;
    }
    public bool InsertAll(Inventory other, ItemContainer target)
    {
        return default;
    }
    public bool InsertAll(ItemContainer target)
    {
        return default;
    }
    /// <summary>
    /// 아이템 칸 장금
    /// </summary>
    public void LockSlot(int wantRow, int wantClumn)
    {

    }
    /// <summary>
    /// 아이템 칸 장금 해제
    /// </summary>
    public void UnLockSlot(int wantRow, int wantClumn)
    {

    }

    public IEnumerable<ItemSlot> GetAllSlot()
    {
       // ItemSlot[] result = new ItemSlot[slots.Length];
        // X = Width * R + C
        int height = slots.GetLength(0);
        int width = slots.GetLength(1);
        for (int r = 0; r < height; r++)
        {
            for(int c = 0; c < width; c++)
            {
                if (slots[r, c] is null) continue;
                yield return slots[r,c];
            }
        }

        
    }

    public IEnumerable<ItemSlot> GetAllSlotReveres()
    {
        int height = slots.GetLength(0);
        int width = slots.GetLength(1);
        for (int r = height -1; r >= height; r--)
        {
            for(int c = width -1; c >= width; c--)
            {
                if (slots[r, c] is null) continue;
                yield return slots[r,c];
            }
        }
    }

    public IEnumerable<ItemSlot> FindItem(ItemContainer target)
    {
        foreach (ItemSlot currentSlot in GetAllSlot())
        {
            if(currentSlot.GetItem() == target) yield return currentSlot;
        }
    }     
    public ItemSlot FindItem(ItemType wantType)
    {
        return default;
    }
    public ItemSlot FindItem(int wantRow, int wantClumn)
    {
        if (wantRow < 0 || wantClumn < 0) return null;
        if (wantRow >= slots.GetLength(0)) return null;
        if (wantClumn >= slots.GetLength(1)) return null;
        return slots[wantRow, wantClumn];
    }      

    public IEnumerable<ItemSlot> FindFirstEmptySlot()
    {
        foreach(ItemSlot currentSlot in GetAllSlot())
        {
            if(currentSlot.GetIsEmpty()) yield return currentSlot;
        }
    }
    public IEnumerable<ItemSlot> FindLastEmptySlot()
    {
        foreach (ItemSlot currentSlot in GetAllSlotReveres())
        {
            if (currentSlot.GetIsEmpty()) yield return currentSlot;
        }
    }

    public IEnumerable<ItemSlot> FindFirstItem(ItemContainer tatget)
    {
        foreach (ItemSlot currentSlot in GetAllSlot())
        {
            if (currentSlot.GetItem() == tatget) yield return currentSlot;
        }
    }
           
    public IEnumerable<ItemSlot> FindLastItem(ItemContainer target)
    {
        foreach (ItemSlot currentSlot in GetAllSlotReveres())
        {
            if (currentSlot.GetItem() == target) yield return currentSlot;
        }
    }

    /// <summary>
    /// 아이템 추가
    /// </summary>
    public int AddItem(ItemContainer wantItem, int amount = 1)
    { 
        amount = AddItemOnEmptySlots(wantItem, amount);

        if(amount <= 0) return 0;

        return AddItemOnEmptySlots(wantItem, amount);
    }

    public int AddItemOnExistSlots(ItemContainer wantItem, int amount)
    {
        foreach (ItemSlot currentSlot in FindFirstItem(wantItem))
        {
            if (amount <= 0) return 0;
            amount = currentSlot.AddItem(wantItem, amount);
            currentSlot.NoticeChanged();
        }
        return amount;
    }

    public int AddItemOnEmptySlots(ItemContainer wantItem, int amount)
    {
        foreach (ItemSlot currentSlot in FindFirstEmptySlot())
        {
            if (amount <= 0) return 0;
            amount = currentSlot.AddItem(wantItem, amount);
            currentSlot.NoticeChanged();
        }
        return amount;
    }

    public int AddItemToLocation(ItemContainer wantItem, int amount, int row, int column)
    {
        return default;
    }

    public ItemSlot[,] Clear()
    {
        ItemSlot[,] origin = slots;
        Initialized();
        return origin;
    }

    public int RemoveItem(ItemContainer wantItam, int amount)
    {
        foreach (ItemSlot currentSlot in FindFirstItem(wantItam)) 
        {
            if (amount <= 0) return 0;
            amount = currentSlot.AddItem(wantItam, amount);
            currentSlot.NoticeChanged();
        }
        return amount;
    }
    /// <summary>
    /// 아이템 버리기
    /// </summary>
    /// 
   
   // public int RemoveItem(ItemContainer wantItem)
   // {
   //     return default;
   // }
   //
   // public int RemoveItem(ItemContainer wantItem, int amount = 1)
   // {
   //     return default;
   // }   

    public int RemoveItemOnExistSlots(ItemContainer wantItem, int amount)
    {
        return default;
    }
    public int RemoveItemFromLocation(int row, int column)
    {
        return default;
    }

    public int RemoveItemFromLocation( int row, int column, int amount)
    {
        return default;
    }

    /// <summary>
    /// 아이템 움직임
    /// </summary>
    public void MoveItem(int startRow, int startColumn, Inventory targetInventory, int targetRow, int targetColumn, int amoumt = -1)
    {

    }
    /// <summary>
    /// 아이템을 사용한다.
    /// </summary>
    public bool UseItem(ItemContainer target)
    {
        return default;
    }

    public bool UseItem(int startRow, int startColumn)
    {
        return default;
    }

    public int CounItem(ItemContainer wantitem, out List<ItemSlot> returnSlots)
    {
        returnSlots = default;
        return default;
    }


    public void EX(int amount)
    {
        ItemContainer potion = DataManager.LoadDataFile<ItemContainer>("Potion");
        AddItem(potion, amount);
    }
}
