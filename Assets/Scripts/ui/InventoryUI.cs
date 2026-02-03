using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryWindow; // 전체 창
    public Transform gridParent;       // 그리드 부모 (Main Inventory)
    public Transform quickSlotParent;  // 하단 5칸 부모
    public SlotUI bucketSlotUI;        // 장착된 통 슬롯

    public static InventoryUI Instance;
    public BucketViewUI bucketView;

    private List<SlotUI> allMainSlots = new List<SlotUI>();
    private List<SlotUI> allQuickSlots = new List<SlotUI>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        allMainSlots.Clear();
        foreach (Transform child in gridParent) 
        {
            SlotUI slot = child.GetComponent<SlotUI>();
            if (slot != null) allMainSlots.Add(slot); // 스크립트가 있는 경우만 추가
        }
                
        allQuickSlots.Clear();
        foreach (Transform child in quickSlotParent) 
        {
            SlotUI slot = child.GetComponent<SlotUI>();
            if (slot != null) allQuickSlots.Add(slot);
        }

        // 싱글톤 인스턴스가 있는지 확인 후 연결
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
        }
        
        inventoryWindow.SetActive(false);
        RefreshUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventoryWindow.SetActive(!inventoryWindow.activeSelf);
        }

        // F키: 물고기 통 내부 창 토글
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 장착된 통이 있을 때만 작동
            if (InventoryManager.Instance.equippedBucket != null)
            {
                bucketView.ToggleView();
            }
            else
            {
                Debug.Log("장착된 통이 없어 열 수 없습니다.");
            }
        }
    }

    public void RefreshUI()
    {
        // InventoryManager가 아직 없거나 리스트가 생성 전이면 리턴
        if (InventoryManager.Instance == null || InventoryManager.Instance.mainInventory == null) return;

        // 1. 메인 가방 새로고침
        for (int i = 0; i < allMainSlots.Count; i++)
        {
            if (i < InventoryManager.Instance.mainInventory.Count)
                allMainSlots[i].UpdateSlot(InventoryManager.Instance.mainInventory[i]);
            else
                allMainSlots[i].UpdateSlot(null);
        }

        // 2. 하단 퀵슬롯 새로고침
        for (int i = 0; i < allQuickSlots.Count; i++)
        {
            if (i < InventoryManager.Instance.mainInventory.Count)
                allQuickSlots[i].UpdateSlot(InventoryManager.Instance.mainInventory[i]);
            else
                allQuickSlots[i].UpdateSlot(null);
        }

        // 3. 장착된 통 슬롯 업데이트
        if (bucketSlotUI != null)
        {
            bucketSlotUI.UpdateSlot(InventoryManager.Instance.equippedBucket);
        }
    }
}