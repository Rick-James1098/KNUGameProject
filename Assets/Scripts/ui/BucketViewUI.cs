using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BucketViewUI : MonoBehaviour
{
    public GameObject viewPanel;      // BucketViewPanel 본체
    public Transform gridParent;      // 슬롯들이 들어있는 BucketGrid
    public TextMeshProUGUI titleText; // 통 이름을 표시할 텍스트

    private List<SlotUI> bucketSlots = new List<SlotUI>();

    void Awake()
    {
        // 1. 그리드 자식들에 있는 슬롯들을 미리 수집
        foreach (Transform child in gridParent)
        {
            SlotUI slot = child.GetComponent<SlotUI>();
            if (slot != null) bucketSlots.Add(slot);
        }
    }

    void Start()
    {
        // 2. 인벤토리 데이터가 변할 때마다 이 창도 같이 갱신되도록 연결
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshBucketView;

        viewPanel.SetActive(false);
    }

    // 창 열기/닫기 (장착된 통 슬롯을 클릭할 때 실행됨)
    public void ToggleView()
    {
        // 장착된 통이 없으면 아예 열리지 않음
        if (InventoryManager.Instance.equippedBucket == null)
        {
            viewPanel.SetActive(false);
            return;
        }

        viewPanel.SetActive(!viewPanel.activeSelf);
        if (viewPanel.activeSelf) RefreshBucketView();
    }

    public void RefreshBucketView()
    {
        // 창이 꺼져있거나 통이 없으면 무시
        if (!viewPanel.activeSelf || InventoryManager.Instance.equippedBucket == null) return;

        ItemSlot currentBucket = InventoryManager.Instance.equippedBucket;
        titleText.text = $"{currentBucket.item.itemName} 내부";

        // 통 안의 innerSlots 데이터를 UI 슬롯에 하나씩 뿌려줌
        for (int i = 0; i < bucketSlots.Count; i++)
        {
            if (i < currentBucket.innerSlots.Count)
                bucketSlots[i].UpdateSlot(currentBucket.innerSlots[i]);
            else
                bucketSlots[i].UpdateSlot(null);
        }
    }
}