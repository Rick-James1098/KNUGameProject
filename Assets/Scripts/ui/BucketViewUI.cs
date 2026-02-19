using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BucketViewUI : MonoBehaviour
{
    public GameObject viewPanel;      
    public Transform gridParent;      
    public TextMeshProUGUI titleText; 

    private List<SlotUI> bucketSlots = new List<SlotUI>();

    void Awake()
    {
        bucketSlots.Clear();
        foreach (Transform child in gridParent)
        {
            SlotUI slot = child.GetComponent<SlotUI>();
            if (slot != null) bucketSlots.Add(slot);
        }
    }

    void Start()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshBucketView;

        viewPanel.SetActive(false);
    }

    // [추가] 씬이 바뀔 때 이벤트 연결을 해제해줘야 에러가 안 납니다.
    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshBucketView;
    }

    public void ToggleView()
    {
        // 수정: invData.equippedBucket으로 접근
        if (InventoryManager.Instance.invData.equippedBucket == null)
        {
            viewPanel.SetActive(false);
            return;
        }

        viewPanel.SetActive(!viewPanel.activeSelf);
        if (viewPanel.activeSelf) RefreshBucketView();
    }

    public void RefreshBucketView()
    {
        // 수정: invData 및 데이터 존재 여부 삼중 체크
        if (!viewPanel.activeSelf || InventoryManager.Instance.invData == null || InventoryManager.Instance.invData.equippedBucket == null) return;

        ItemSlot currentBucket = InventoryManager.Instance.invData.equippedBucket;
        titleText.text = $"{currentBucket.item.itemName} 내부";

        for (int i = 0; i < bucketSlots.Count; i++)
        {
            if (i < currentBucket.innerSlots.Count)
                bucketSlots[i].UpdateSlot(currentBucket.innerSlots[i]);
            else
                bucketSlots[i].UpdateSlot(null);
        }
    }
}