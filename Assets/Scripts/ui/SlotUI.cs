using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Image frameImage; // 선택/장착 강조용 (선택 사항)

    [HideInInspector] public ItemSlot assignedSlot; // 이 UI가 가리키는 실제 데이터

    // 슬롯에 데이터를 채우는 함수
    public void UpdateSlot(ItemSlot newSlot)
    {
        assignedSlot = newSlot;

        if (newSlot != null && newSlot.item != null)
        {
            iconImage.enabled = true;

            // --- 아이콘 결정 로직 시작 ---
            if (newSlot.item is ContainerItemData containerData)
            {
                // 통 안에 물고기가 1개라도 들어있는지 확인
                if (newSlot.innerSlots != null && newSlot.innerSlots.Count > 0)
                {
                    // 내용물이 있으면 가득 찬 아이콘
                    iconImage.sprite = containerData.fullIcon;
                }
                else
                {
                    // 비어있으면 빈 아이콘
                    iconImage.sprite = containerData.emptyIcon;
                }
            }
            else
            {
                // 일반 아이템은 기존대로 기본 아이콘 사용
                iconImage.sprite = newSlot.item.icon;
            }
            // --- 아이콘 결정 로직 끝 ---

            if (newSlot.item.maxStackSize > 1 && newSlot.count > 1)
                countText.text = newSlot.count.ToString();
            else
                countText.text = "";
        }
        else
        {
            iconImage.enabled = false;
            countText.text = "";
        }
    }

    // 슬롯 클릭 이벤트 (Button 컴포넌트에 연결)
    public void OnClickSlot()
    {
        // 1. 내가 '장착 전용 슬롯'인가?
        if (this == InventoryUI.Instance.bucketSlotUI)
        {
            if (assignedSlot != null && assignedSlot.item != null)
            {
                // 이제 클릭하면 창을 여는 대신 장착을 해제합니다!
                InventoryManager.Instance.UnequipBucket();
                
                // 통 창이 열려있었다면 같이 닫아주는 게 자연스럽습니다.
                if(InventoryUI.Instance.bucketView.viewPanel.activeSelf)
                    InventoryUI.Instance.bucketView.ToggleView();
            }
            return;
        }

        // 2. 일반 가방에 있는 아이템 클릭 시
        if (assignedSlot == null || assignedSlot.item == null) return;

        if (assignedSlot.item is ContainerItemData)
        {
            InventoryManager.Instance.EquipBucket(assignedSlot);
        }
    }
}