using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_InvetoryWindow : OpenableUIBase
{
    [SerializeField] Inventory targetInvetory;
    [SerializeField] LayoutGroup layout;
    [SerializeField] string itemSlotPrefabName;

    public override void Registration(UIManager manager)
    {
        base.Registration(manager);
        targetInvetory?.Initialized();
        ConnectInvetory(targetInvetory);
    }


    public override void Unregistration(UIManager manager)
    {
        base.Unregistration(manager);
        DisconnectInvetory();
    }

    private void ConnectInvetory(Inventory newInvetory)
    {
        if (!newInvetory) return;
        targetInvetory = newInvetory;

        if (!layout) return;

        if(layout is GridLayoutGroup asGridLayout)
        {
            asGridLayout.constraintCount = targetInvetory.columns;
        }

        
        foreach (ItemSlot currentSlot in newInvetory.GetAllSlot()) 
        { 
            if(currentSlot is null) continue; //슬롯이 없는데? 넘어가
            //만들어서 Instance에 저장
            GameObject instance = ObjectManager.CreateObject(itemSlotPrefabName, layout.transform);
            if (!instance) continue;
            if(instance.TryGetComponent(out UI_ItemSlotinfo createdSlot))
            {
                createdSlot.Connectslot(currentSlot);
            }
        }
    }
    private void DisconnectInvetory()
    {
        if (!layout) return;
        //레이아웃에 들어있는 모든 자식들을 다 지워줄 것!
        while (layout.transform.childCount > 0)
        {
            Transform targetChild = layout.transform.GetChild(0);
            targetChild.SetParent(null);
            ObjectManager.DestroyObject(layout.transform.GetChild(0).gameObject);
        }
    }
}
