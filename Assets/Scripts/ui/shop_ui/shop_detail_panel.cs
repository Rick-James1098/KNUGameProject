using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopDetailsPanel : MonoBehaviour
{
    [Header("정보 표시 UI")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI pricePerUnitText; // 개당 가격

    [Header("수량 조절 UI")]
    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI totalPriceText; // 총 가격 (개당 * 수량)
    public Button increaseBtn;
    public Button decreaseBtn;

    // 현재 상태 데이터
    private ItemData _currentItem;
    private int _currentQuantity = 1;

    public ItemData CurrentItem => _currentItem; // 매니저가 가져다 쓸 속성
    public int CurrentQuantity => _currentQuantity;

    private void Start()
    {
        // 화살표 버튼 연결
        increaseBtn.onClick.AddListener(() => AdjustQuantity(1));
        decreaseBtn.onClick.AddListener(() => AdjustQuantity(-1));
    }

    // 1. 아이템 정보를 받아서 화면을 갱신하는 함수
    public void SetItemInfo(ItemData item)
    {
        _currentItem = item;
        _currentQuantity = 1; // 다른 아이템 누르면 수량 1로 리셋

        // UI 갱
        nameText.text = item.itemName;
        typeText.text = item.itemType.ToString(); // 타입 표시
        descText.text = item.description;
        pricePerUnitText.text = $"{item.price:N0} G";

        UpdateQuantityUI();
        
        // 패널이 꺼져있었다면 켜기
        gameObject.SetActive(true);
    }

    // 2. 수량 조절 (+1, -1)
    void AdjustQuantity(int amount)
    {
        if (_currentItem == null) return;

        _currentQuantity += amount;

        // 최소 1개, 최대 99개 (원하면 999로 수정)
        if (_currentQuantity < 1) _currentQuantity = 1;
        if (_currentQuantity > 99) _currentQuantity = 99;

        UpdateQuantityUI();
    }

    // 3. 수량과 총 가격 텍스트 갱신
    void UpdateQuantityUI()
    {
        quantityText.text = _currentQuantity.ToString();
        
        if (_currentItem != null)
        {
            int total = _currentItem.price * _currentQuantity;
            totalPriceText.text = $"총 {total:N0} G";
        }
    }

    public void ClearInfo()
    {
        _currentItem = null;
        _currentQuantity = 1;
        
        // 2. 모든 텍스트 비우기
        nameText.text = "";
        typeText.text = "";
        descText.text = "";
        pricePerUnitText.text = "";
        
        // 3. 수량/가격 텍스트 비우기
        quantityText.text = "";
        totalPriceText.text = "";

        // 4. 버튼 비활성화 (클릭 못하게)
        increaseBtn.interactable = false;
        decreaseBtn.interactable = false;
        
        // (중요) 상점 매니저에 있는 구매 버튼도 비활성화해야 하는데, 
        // 그건 ShopManager에서 ClearInfo 호출 직후에 처리하고 있으니 여기선 패스
    }
}