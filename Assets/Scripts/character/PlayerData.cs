using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "FishingGame/Player Data")]
public class PlayerData : ScriptableObject
{
    [Header("플레이어 상태")]
    public Vector2 lastDirection = Vector2.down; // 마지막으로 바라본 방향
    public bool isFishing = false;               // 찌를 던진 상태인가?
    public bool isStrikeGameActive = false;      // 챔질(ArcGame) UI가 떠야 하는가?

    [Header("낚시 진행 데이터")]
    public GameObject currentBobber;
    public FishData hookedFish;                  // 현재 바늘에 걸린 물고기 정보
    public bool isBattleSuccess = false;         // 배틀(릴링) 게임 성공 여부

    /// <summary>
    /// 한 번의 낚시 사이클이 끝났을 때 데이터를 초기화합니다.
    /// </summary>
    public void ResetFishingStatus()
    {
        isFishing = false;
        isStrikeGameActive = false;
        hookedFish = null;
        isBattleSuccess = false;
        Debug.Log("플레이어 낚시 장부가 초기화되었습니다.");
    }

    /// <summary>
    /// 게임을 처음 시작할 때 전체적인 초기화가 필요할 경우 사용합니다.
    /// </summary>
    public void OnEnable()
    {
        // 에디터에서 플레이를 누를 때마다 이전 기록이 남지 않도록 초기화
        ResetFishingStatus();
    }
}