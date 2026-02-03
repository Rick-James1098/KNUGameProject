using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("메인 인벤토리 설정")]
    public int maxMainSlots = 20;
    // 기존의 slots를 mainInventory로 명칭 통합 (혼동 방지)
    public List<ItemSlot> mainInventory = new List<ItemSlot>();

    [Header("장착된 특수 보관함")]
    public ItemSlot equippedBucket; // 현재 장착 중인 물고기 통

    // UI 새로고침 알림
    public Action OnInventoryChanged;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// [통합 입구] 어떤 아이템이든 이 함수를 통해 인벤토리에 들어옵니다.
    /// </summary>
    public void AddItem(ItemData newItem, int amount = 1)
    {
        bool wasAdded = false;

        // 1. 물고기인지 확인하여 통(Bucket)에 먼저 넣기 시도
        if (newItem is FishData fish && equippedBucket != null)
        {
            ContainerItemData bucketData = equippedBucket.item as ContainerItemData;
            
            // 통에 자리가 있는지 확인 후 추가
            if (equippedBucket.innerSlots.Count < bucketData.capacity)
            {
                wasAdded = AddItemToList(equippedBucket.innerSlots, newItem, amount, bucketData.capacity);
                if (wasAdded) Debug.Log($"{newItem.itemName}을(를) 물고기 통에 담았습니다.");
            }
        }

        // 2. 물고기가 아니거나, 통에 넣지 못했을 경우 메인 가방으로
        if (!wasAdded)
        {
            wasAdded = AddItemToList(mainInventory, newItem, amount, maxMainSlots);
            if (wasAdded) Debug.Log($"{newItem.itemName}을(를) 일반 가방에 담았습니다.");
            else Debug.Log("가방이 꽉 찼습니다!");
        }

        // 3. 아이템이 어디든 들어갔다면 UI 갱신 호출
        if (wasAdded)
        {
            OnInventoryChanged?.Invoke();
        }
    }

    /// <summary>
/// 특정 리스트에 아이템을 추가하는 핵심 로직입니다.
/// </summary>
/// <param name="targetList">아이템이 들어갈 리Add스트 (가방 혹은 물고기 통 내부)</param>
/// <param name="item">추가할 아이템 데이터</param>
/// <param name="amount">추가할 개수</param>
/// <param name="maxCapacity">해당 리스트의 최대 칸 수</param>
/// <returns>추가 성공 여부</returns>
private bool AddItemToList(List<ItemSlot> targetList, ItemData item, int amount, int maxCapacity)
{
    bool wasAdded = false;
    
    // 1. 물고기 통(Container) 여부 확인
    // 통 아이템은 절대로 겹치면 안 되므로 겹치기 로직을 건너뜁니다.
    bool isContainer = item is ContainerItemData;

    // 2. 겹치기(Stack) 처리
    // 통이 아니고, 아이템 자체가 겹치기가 가능한 설정일 때만 실행
    if (!isContainer && item.maxStackSize > 1)
    {
        foreach (var slot in targetList)
        {
            // 같은 아이템이고, 해당 슬롯에 아직 여유 공간이 있다면
            if (slot.item == item && slot.count < item.maxStackSize)
            {
                // 최대한 채울 수 있는 양 계산 (오버플로우 방지)
                int canAdd = Mathf.Min(amount, item.maxStackSize - slot.count);
                slot.count += canAdd;
                amount -= canAdd;

                if (amount <= 0)
                {
                    wasAdded = true;
                    break;
                }
            }
        }
    }

    // 3. 새 슬롯 추가
    // 아직 남은 수량이 있고(혹은 통이어서 위 로직을 안 탔고), 가방에 빈 자리가 있다면
    if (amount > 0 && targetList.Count < maxCapacity)
    {
        // 물고기 통은 무조건 한 칸에 1개씩만 생성되도록 보장
        if (isContainer)
        {
            // 통은 개별적으로 존재해야 하므로 하나씩 리스트에 추가
            for (int i = 0; i < amount; i++)
            {
                if (targetList.Count < maxCapacity)
                {
                    targetList.Add(new ItemSlot(item, 1));
                    wasAdded = true;
                }
            }
        }
        else
        {
            // 일반 아이템은 한 번에 추가
            targetList.Add(new ItemSlot(item, amount));
            wasAdded = true;
        }
    }

    // 4. 최종 결과 보고 및 UI 갱신 신호
    if (wasAdded)
    {
        // 데이터가 실제로 바뀌었을 때만 UI 새로고침 이벤트를 발생시킵니다.
        
        OnInventoryChanged?.Invoke();
    }

    return wasAdded;
}

    // 물고기 통 장착
    public void EquipBucket(ItemSlot bucketSlot)
    {
        if (bucketSlot.item is ContainerItemData)
        {
            // 1. 이미 장착된 통이 있다면 다시 일반 가방으로 돌려보냄
            if (equippedBucket != null && equippedBucket.item != null)
            {
                mainInventory.Add(equippedBucket);
            }

            // 2. 새로운 통을 장착 슬롯으로 설정
            equippedBucket = bucketSlot;

            // 3. 일반 가방 리스트에서 해당 아이템 제거
            mainInventory.Remove(bucketSlot);

            Debug.Log(bucketSlot.item.itemName + " 장착 및 이동 완료!");
            
            // 4. UI 새로고침
            OnInventoryChanged?.Invoke();
        }
    }

    // InventoryManager.cs에 추가
    public void UnequipBucket()
    {
        if (equippedBucket != null)
        {
            // 1. 가방에 빈 자리가 있는지 확인 (최대 칸 수 maxMainSlots가 있다고 가정)
            if (mainInventory.Count < 20) // 20은 본인의 가방 최대 칸 수로 조절하세요
            {
                mainInventory.Add(equippedBucket);
                string name = equippedBucket.item.itemName;
                equippedBucket = null; // 장착칸 비우기
                
                Debug.Log($"{name} 장착 해제 완료!");
                OnInventoryChanged?.Invoke(); // UI 갱신 신호
            }
            else
            {
                Debug.Log("가방이 꽉 차서 해제할 수 없습니다!");
            }
        }
    }

    // 아이템 제거 (상점/제작용)
    public void RemoveItem(ItemData itemToRemove, int amount = 1)
    {
        // 메인 인벤토리에서 먼저 검색 (필요 시 Bucket 내부 검색 로직도 추가 가능)
        for (int i = 0; i < mainInventory.Count; i++)
        {
            if (mainInventory[i].item == itemToRemove)
            {
                mainInventory[i].count -= amount;
                if (mainInventory[i].count <= 0) mainInventory.RemoveAt(i);
                
                OnInventoryChanged?.Invoke();
                return;
            }
        }
    }
}