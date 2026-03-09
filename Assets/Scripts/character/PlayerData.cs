using NUnit.Framework.Internal.Filters;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "FishingGame/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("플레이어 상태")]
    public Vector2 lastDirection = Vector2.down; // 마지막으로 바라본 방향
    public bool isFishing = false;               // 찌를 던진 상태인가?
    public bool isStrikeGameActive = false;      // 챔질(ArcGame) UI가 떠야 하는가?
    public bool hasFishBucket = false; // 고기통 장착 여부
    private bool isLuckDoped = false;

    [Header("낚시 진행 데이터")]
    public GameObject currentBobber;
    public FishDataFormat hookedFish;                  // 현재 바늘에 걸린 물고기 정보
    public bool isBattleSuccess = false;         // 배틀(릴링) 게임 성공 여부
    [Header("경제 시스템")]
    public int gold;

    [Header("스탯")]
    public int luck = 100;
    public int technic = 0;
    public float rodSkill = 0f;
    public float harpoonSkill = 0f;
    public float conchSkill = 0f;
    public int dopingLuck = 0;

    /// <summary>
    /// 한 번의 낚시 사이클이 끝났을 때 데이터를 초기화합니다.
    /// </summary>
    public void ResetCycle()
    {
        isFishing = false;
        isStrikeGameActive = false;
        isBattleSuccess = false;
        Debug.Log("사이클 데이터 초기화 완료 (장비는 유지)");
    }

    /// <summary>
    /// [2] 게임 시작 시 초기화 (OnEnable에서 호출)
    /// 에디터 잔상을 지우기 위한 용도입니다.
    /// </summary>
    public void OnEnable()
    {
        ResetCycle();
        // 장착 상태도 처음엔 false로 시작 (InventoryManager가 채워줄 것임)
        hasFishBucket = false; 
    }

    public int GetPlayerLuck()
    {
        return luck;
    }
}