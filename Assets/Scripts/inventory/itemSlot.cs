using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine;

[System.Serializable]
public class ItemSlot
{
    public ItemData item;
    public int count;

    // [추가] 만약 이 아이템이 '통'이라면 내부에 담긴 아이템들
    [HideInInspector]
    public List<ItemSlot> innerSlots; 

    public ItemSlot(ItemData item, int count)
    {
        this.item = item;
        this.count = count;
        
        // 만약 아이템이 컨테이너라면 내부 리스트 초기화
        if (item is ContainerItemData container)
        {
            innerSlots = new List<ItemSlot>();
        }
    }
}