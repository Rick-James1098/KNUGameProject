using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("데이터 에셋 (SO)")]
    public InventoryData invData; 

    // UI 새로고침 알림용 이벤트
    public Action OnInventoryChanged;
    public PlayerData playerData;

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

    void Start()
    {
        // 게임 시작 시 인벤토리 상황을 장부에 보고
        SyncBucketState();
    }

    public void SyncBucketState()
    {
        playerData.hasFishBucket = (invData.equippedBucket != null && invData.equippedBucket.item != null);
    }

    /// <summary>
    /// 아이템 추가 통합 입구
    /// </summary>
    public void AddItem(ItemData newItem, int amount = 1)
    {
        if (invData == null) { Debug.LogError("InventoryData가 연결되지 않았습니다!"); return; }

        bool wasAdded = false;

        // 1. 물고기인 경우 처리
        if (newItem is FishDataFormat)
        {
            // 장착된 통이 있다면 넣기 시도
            if (invData.equippedBucket != null)
            {
                ContainerItemData bucketData = invData.equippedBucket.item as ContainerItemData;
                
                if (invData.equippedBucket.innerSlots.Count < bucketData.capacity)
                {
                    wasAdded = AddItemToList(invData.equippedBucket.innerSlots, newItem, amount, bucketData.capacity);
                    if (wasAdded) Debug.Log($"{newItem.itemName}을(를) 물고기 통에 넣었습니다.");
                }
                else
                {
                    Debug.Log("물고기 통이 꽉 찼습니다!");
                }
            }
            else
            {
                // [핵심] 통이 없으면 여기서 리턴시켜서 가방으로 못 넘어가게 함
                Debug.LogWarning("장착된 고기통이 없어 물고기를 놓아주었습니다.");
                return; 
            }
            
            // 물고기인데 통에 넣는 걸 실패했다면(통이 꽉 찬 경우 등) 여기서 종료
            if (!wasAdded) return;
        }

        // 2. 일반 아이템인 경우 (또는 위에서 처리되지 않은 경우만 실행)
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
        if (bucketSlot == null || bucketSlot.item == null) return;

        if (bucketSlot.item is ContainerItemData)
        {

            // [수정] 만약 지금 클릭한 게 이미 장착된 통이라면? -> 해제(Unequip)로 연결
            if (invData.equippedBucket == bucketSlot)
            {
                Debug.Log("이미 장착된 통입니다. 해제를 실행합니다.");
                UnequipBucket();
                return;
            }

            // 1. 기존에 이미 장착된 통이 있으면 가방으로 복구
            if (invData.equippedBucket != null && invData.equippedBucket.item != null)
            {
                invData.mainInventory.Add(invData.equippedBucket);
            }

            // 2. 새 통을 장착 (참조 복사)
            invData.equippedBucket = new ItemSlot(bucketSlot.item, bucketSlot.count);
            invData.equippedBucket.innerSlots = bucketSlot.innerSlots; // 내용물도 복사

            // 3. 가방에서 해당 슬롯 제거 (참조 기반 제거가 불안하면 이렇게 하세요)
            invData.mainInventory.Remove(bucketSlot);

            // 4. 데이터 갱신 알림
            if (playerData != null) playerData.hasFishBucket = true;
            
            Debug.Log($"{bucketSlot.item.itemName} 장착 완료");
            OnInventoryChanged?.Invoke();
        }

        SyncBucketState();
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

                // [추가] 장부에 고기통 해제됨을 기록
                if (playerData != null) playerData.hasFishBucket = false;

                OnInventoryChanged?.Invoke();
            }
        }

        SyncBucketState();
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