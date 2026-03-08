using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header("데이터 연결")]
    public PlayerData playerData;
    public InventoryManager inventoryManager;
    public List<ItemData> shopItems;

    [Header("UI 연결")]
    public GameObject shopPanel;
    public Transform slotListParent; // 슬롯들이 생길 곳 (Content)
    public GameObject slotPrefab;
    
    [Header("상세 설명창 연결")]
    public ShopDetailsPanel detailsPanel; // 위에서 만든 스크립트 연결
    public GameObject buyButtonObject;    // 구매 버튼 (비활성화 관리용)

    private void Start()
    {
        shopPanel.SetActive(false);
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        GenerateSlots();
        
        // [수정] 상세창을 끄지 않고, '내용만 비움'
        detailsPanel.ClearInfo(); 
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    void GenerateSlots()
    {
        // 기존 슬롯 삭제
        foreach (Transform child in slotListParent) Destroy(child.gameObject);

        // 새 슬롯 생성
        foreach (ItemData item in shopItems)
        {
            GameObject go = Instantiate(slotPrefab, slotListParent);
            go.GetComponent<ShopSlot>().Setup(item, this);
        }
    }

    // [Slot]에서 호출: "나 클릭됐어!"
    public void OnSlotClicked(ItemData item)
    {
        // 상세 패널에게 정보를 넘겨주고 갱신하라고 시킴
        detailsPanel.SetItemInfo(item);
    }

    // [구매 버튼] UI 버튼에 연결할 함수
    public void OnBuyBtnClick()
    {
        ItemData itemToBuy = detailsPanel.CurrentItem;
        int qtyToBuy = detailsPanel.CurrentQuantity;

        if (itemToBuy == null) return;

        int totalCost = itemToBuy.price * qtyToBuy;

        // 1. 돈 체크
        if (playerData.gold < totalCost)
        {
            Debug.Log("돈이 부족합니다!");
            return;
        }

        // 2. 인벤토리 체크 (선택사항)
        // if (inventoryManager.IsFull()) return;

        // 3. 거래 실행
        playerData.gold -= totalCost;

        // 수량만큼 인벤토리에 추가 (InventoryManager에 수량 추가 기능이 있다면 그걸 쓰고, 없다면 반복문)
        for (int i = 0; i < qtyToBuy; i++)
        {
            inventoryManager.AddItem(itemToBuy);
        }

        Debug.Log($"{itemToBuy.itemName} {qtyToBuy}개 구매 완료! (비용: {totalCost})");
        
        // UI 갱신 (돈 빠져나간 거 표시 등)
        // detailsPanel.UpdateQuantityUI(); // 필요하면 호출
    }
}