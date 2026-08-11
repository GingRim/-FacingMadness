using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Inventory : MonoBehaviour
{

    private void Awake()
    {
        Initialized();
    }

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
        columns = Mathf.Max(1, columns);
        rows = Mathf.Max(1, rows);

        slots = new ItemSlot[columns, rows];

        for (int column = 0; column < columns; column++)
        {
            for (int row = 0; row < rows; row++)
            {
                slots[column, row] = new ItemSlot();
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
        if (slots == null)
            yield break;

        int height = slots.GetLength(0);
        int width = slots.GetLength(1);

        for (int row = height - 1; row >= 0; row--)
        {
            for (int column = width - 1;
                 column >= 0;
                 column--)
            {
                if (slots[row, column] == null)
                    continue;

                yield return slots[row, column];
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
        if (wantItem == null || amount <= 0)
            return amount;

        amount = AddItemOnExistSlots(wantItem, amount);

        if (amount <= 0)
            return 0;

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
        if (wantItam == null || amount <= 0)
            return amount;

        foreach (ItemSlot currentSlot
                 in FindFirstItem(wantItam))
        {
            if (amount <= 0)
                return 0;

            amount = currentSlot.RemoveItem(wantItam, amount);

            currentSlot.NoticeChanged();
        }

        // 제거하지 못하고 남은 수량
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
        return RemoveItem(wantItem, amount); 
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
        returnSlots = new List<ItemSlot>();

        if (wantitem == null || slots == null)
            return 0;

        int totalAmount = 0;

        foreach (ItemSlot slot in FindItem(wantitem))
        {
            if (slot == null)
                continue;

            returnSlots.Add(slot);
            totalAmount += slot.GatStack();
        }

        return totalAmount;
    }


    public int CountItem(ItemContainer item)
    {
        return CounItem(item, out _);
    }

    public bool HasItem(
        ItemContainer item,
        int amount = 1)
    {
        if (item == null || amount <= 0)
            return false;

        return CountItem(item) >= amount;
    }

    public void EX(int amount)
    {
        ItemContainer potion = DataManager.LoadDataFile<ItemContainer>("Potion");
        AddItem(potion, amount);
    }
}
