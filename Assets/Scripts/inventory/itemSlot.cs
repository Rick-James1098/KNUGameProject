using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemSlot
{
    public ItemData item;
    public int count;

    // 통 아이템일 경우 내부 아이템들 저장
    public List<ItemSlot> innerSlots = new List<ItemSlot>(); 

    public ItemSlot(ItemData item, int count)
    {
        this.item = item;
        this.count = count;
        
        // 통 아이템이라면 내부 리스트를 사용할 준비를 합니다.
        if (item is ContainerItemData)
        {
            innerSlots = new List<ItemSlot>();
        }
    }
}