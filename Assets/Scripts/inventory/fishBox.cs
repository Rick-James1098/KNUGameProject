using UnityEngine;

[CreateAssetMenu(fileName = "NewBucket", menuName = "Inventory/ContainerItem")]
public class ContainerItemData : ItemData
{
    public int capacity = 10; // 이 통에 담을 수 있는 물고기 수
    public Sprite emptyIcon; // 빈 상태 아이콘 (기존 icon 변수를 써도 되지만 명확하게 분리)
    public Sprite fullIcon;  // 물고기가 하나라도 들어있을 때 아이콘
}