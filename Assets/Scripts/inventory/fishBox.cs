using UnityEngine;

[CreateAssetMenu(fileName = "NewBucket", menuName = "Inventory/ContainerItem")]
public class ContainerItemData : ItemData
{
    public int capacity = 10; // 이 통에 담을 수 있는 물고기 수
    // 물고기만 담을 수 있는지 등을 체크하기 위한 필드를 추가할 수도 있습니다.
}