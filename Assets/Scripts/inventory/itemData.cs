using UnityEngine;

public enum ItemType {Rod, Conch, Harpoon, Fish}

// 이 클래스는 부모 역할만 하므로 CreateAssetMenu를 굳이 안 넣어도 됩니다.
[CreateAssetMenu(fileName = "newItem", menuName = "Item")]
public class ItemData : ScriptableObject
{
    [Header("공통 정보")]
    public string itemName;      // 이름
    public ItemType itemType;
    public Sprite icon;          // 인벤토리 아이콘
    [TextArea]
    public string description;   // 아이템 설명
    public int maxStackSize = 99; // 최대 겹치기 개수
    public int price;            // 판매 가격
}