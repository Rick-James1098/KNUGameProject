using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    public ItemData stone;   // 인스펙터에서 할당
    public FishData carp;    // 인스펙터에서 할당
    public ContainerItemData bucket; // 인스펙터에서 할당

    void Update()
    {
        // 1번 누르면 돌멩이 추가 (일반 가방으로 가야 함)
        if (Input.GetKeyDown(KeyCode.Alpha1)) InventoryManager.Instance.AddItem(stone, 1);

        // 2번 누르면 붕어 추가 (통이 있으면 통으로, 없으면 가방으로 가야 함)
        if (Input.GetKeyDown(KeyCode.Alpha2)) InventoryManager.Instance.AddItem(carp, 1);

        // 3번 누르면 물고기 통 추가 (일반 가방에 생겨야 함)
        if (Input.GetKeyDown(KeyCode.Alpha3)) InventoryManager.Instance.AddItem(bucket, 1);
    }
}