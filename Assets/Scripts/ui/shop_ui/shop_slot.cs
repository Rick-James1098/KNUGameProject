using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopSlot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public Button selectButton; // 버튼 컴포넌트

    private ItemData _item;
    private ShopManager _manager;

    public void Setup(ItemData item, ShopManager manager)
    {
        _item = item;
        _manager = manager;

        iconImage.sprite = item.icon;
        if(nameText != null) nameText.text = item.itemName;
        if (priceText != null) priceText.text = $"{item.price:N0}";

        Debug.Log($"버튼 연결됨: {item.itemName}");

        // 클릭하면 "나를 선택해줘!"라고 매니저에게 요청
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => {
        Debug.Log("버튼 클릭됨!"); // 클릭했을 때 이 로그가 뜨는지 확인!
        _manager.OnSlotClicked(_item);
        });
    }
}