using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventoryData", menuName = "Inventory/Inventory Data")]
public class InventoryData : ScriptableObject
{
    [Header("메인 가방")]
    public int maxMainSlots = 20;
    public List<ItemSlot> mainInventory = new List<ItemSlot>();

    [Header("장착된 특수 보관함")]
    public ItemSlot equippedBucket; // 현재 장착 중인 물고기 통

    // 데이터를 깨끗이 비우는 함수 (테스트용)
    public void Clear()
    {
        mainInventory.Clear();
        equippedBucket = null;
    }
}