using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("데이터 에셋 (SO)")]
    public InventoryData invData; 

    // UI 새로고침 알림용 이벤트
    public Action OnInventoryChanged;

    void Awake()
    {
        // 싱글톤 세팅
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 매니저는 유지
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 아이템 추가 통합 입구
    /// </summary>
    public void AddItem(ItemData newItem, int amount = 1)
    {
        if (invData == null) { Debug.LogError("InventoryData가 연결되지 않았습니다!"); return; }

        bool wasAdded = false;

        // 1. 물고기인 경우: 장착된 통(Bucket)에 먼저 넣기 시도
        if (newItem is FishData && invData.equippedBucket != null)
        {
            ContainerItemData bucketData = invData.equippedBucket.item as ContainerItemData;
            
            // 통의 현재 수납량 확인
            if (invData.equippedBucket.innerSlots.Count < bucketData.capacity)
            {
                wasAdded = AddItemToList(invData.equippedBucket.innerSlots, newItem, amount, bucketData.capacity);
                if (wasAdded) Debug.Log($"{newItem.itemName}을(를) 물고기 통에 넣었습니다.");
            }
        }

        // 2. 일반 아이템이거나, 통에 넣지 못했을 경우: 메인 가방으로
        if (!wasAdded)
        {
            wasAdded = AddItemToList(invData.mainInventory, newItem, amount, invData.maxMainSlots);
            if (wasAdded) Debug.Log($"{newItem.itemName}을(를) 가방에 넣었습니다.");
            else Debug.Log("가방이 꽉 찼습니다!");
        }

        // 3. 변화가 있다면 UI 갱신 신호 발생
        if (wasAdded)
        {
            OnInventoryChanged?.Invoke();
        }
    }

    private bool AddItemToList(List<ItemSlot> targetList, ItemData item, int amount, int maxCapacity)
    {
        bool isContainer = item is ContainerItemData;

        // 1. 겹치기(Stack) 처리 (통 아이템 제외)
        if (!isContainer && item.maxStackSize > 1)
        {
            foreach (var slot in targetList)
            {
                if (slot.item == item && slot.count < item.maxStackSize)
                {
                    int canAdd = Mathf.Min(amount, item.maxStackSize - slot.count);
                    slot.count += canAdd;
                    amount -= canAdd;

                    if (amount <= 0) return true;
                }
            }
        }

        // 2. 새 슬롯 추가
        if (amount > 0 && targetList.Count < maxCapacity)
        {
            if (isContainer)
            {
                for (int i = 0; i < amount; i++)
                {
                    if (targetList.Count < maxCapacity)
                        targetList.Add(new ItemSlot(item, 1));
                }
            }
            else
            {
                targetList.Add(new ItemSlot(item, amount));
            }
            return true;
        }

        return false;
    }

    // 물고기 통 장착
    public void EquipBucket(ItemSlot bucketSlot)
    {
        if (bucketSlot.item is ContainerItemData)
        {
            // 기존 통이 있다면 가방으로 복구
            if (invData.equippedBucket != null && invData.equippedBucket.item != null)
            {
                invData.mainInventory.Add(invData.equippedBucket);
            }

            invData.equippedBucket = bucketSlot;
            invData.mainInventory.Remove(bucketSlot);
            OnInventoryChanged?.Invoke();
        }
    }

    // 물고기 통 해제
    public void UnequipBucket()
    {
        if (invData.equippedBucket != null)
        {
            if (invData.mainInventory.Count < invData.maxMainSlots)
            {
                invData.mainInventory.Add(invData.equippedBucket);
                invData.equippedBucket = null;
                OnInventoryChanged?.Invoke();
            }
        }
    }

    // 아이템 제거
    public void RemoveItem(ItemData itemToRemove, int amount = 1)
    {
        for (int i = 0; i < invData.mainInventory.Count; i++)
        {
            if (invData.mainInventory[i].item == itemToRemove)
            {
                invData.mainInventory[i].count -= amount;
                if (invData.mainInventory[i].count <= 0) invData.mainInventory.RemoveAt(i);
                
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }
}