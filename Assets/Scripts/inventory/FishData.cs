using UnityEngine;

// 상속 대상을 ScriptableObject에서 ItemData로 변경합니다.
[CreateAssetMenu(fileName = "newFish", menuName = "Fishing/FishData")]
public class FishData : ItemData
{
    // itemName, icon, description 등은 이미 부모에게 있으므로 
    // 물고기만의 고유 정보만 추가합니다.

    [Header("물고기 전용 설정")]
    [Range(0, 100)]
    public int rarityWeight;     // 입질 시 확률 가중치

    [Header("캐칭 설정")]
    public float difficulty;
    
    // 만약 인벤토리 아이콘(icon) 외에 
    // 물고기가 펄떡이는 월드 스프라이트가 따로 필요하다면 아래를 유지하세요.
    // public Sprite worldSprite; 
}