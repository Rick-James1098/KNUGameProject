using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public GameObject inventoryWindow; 
    public Transform gridParent;       
    public Transform quickSlotParent;  
    public SlotUI bucketSlotUI;        

    public static InventoryUI Instance;
    public BucketViewUI bucketView;

    private List<SlotUI> allMainSlots = new List<SlotUI>();
    private List<SlotUI> allQuickSlots = new List<SlotUI>();

    // 인벤토리나 물고기 통 창이 하나라도 열려있는지 확인
    public bool IsAnyUIOpen => inventoryWindow.activeSelf || (bucketView != null && bucketView.viewPanel.activeSelf);

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 슬롯 수집 로직
        allMainSlots.Clear();
        foreach (Transform child in gridParent) 
        {
            SlotUI slot = child.GetComponent<SlotUI>();
            if (slot != null) allMainSlots.Add(slot); 
        }
                
        allQuickSlots.Clear();
        foreach (Transform child in quickSlotParent) 
        {
            SlotUI slot = child.GetComponent<SlotUI>();
            if (slot != null) allQuickSlots.Add(slot);
        }

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
        }
        
        inventoryWindow.SetActive(false);
        RefreshUI();
    }

    // [추가] 중요: 씬 전환 시 이벤트 구독 해제
    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            inventoryWindow.SetActive(!inventoryWindow.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            // 수정: invData.equippedBucket 확인
            if (InventoryManager.Instance.invData != null && InventoryManager.Instance.invData.equippedBucket != null)
            {
                bucketView.ToggleView();
            }
            else
            {
                Debug.Log("장착된 통이 없습니다.");
            }
        }
    }

    public void RefreshUI()
    {
        // 수정: invData 경로로 접근하도록 변경
        if (InventoryManager.Instance == null || InventoryManager.Instance.invData == null) return;

        var mainInv = InventoryManager.Instance.invData.mainInventory;

        // 1. 메인 가방 새로고침
        for (int i = 0; i < allMainSlots.Count; i++)
        {
            if (i < mainInv.Count)
                allMainSlots[i].UpdateSlot(mainInv[i]);
            else
                allMainSlots[i].UpdateSlot(null);
        }

        // 2. 하단 퀵슬롯 새로고침
        for (int i = 0; i < allQuickSlots.Count; i++)
        {
            if (i < mainInv.Count)
                allQuickSlots[i].UpdateSlot(mainInv[i]);
            else
                allQuickSlots[i].UpdateSlot(null);
        }

        // 3. 장착된 통 슬롯 업데이트
        if (bucketSlotUI != null)
        {
            bucketSlotUI.UpdateSlot(InventoryManager.Instance.invData.equippedBucket);
        }
    }
}