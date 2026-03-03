using UnityEngine;

public static class FishingDataManager
{
    // 1. 현재 챔질에 성공한 물고기 데이터 (다음 씬에서 이 정보를 보고 HP나 난이도 결정)
    public static FishDataFormat SelectedFish;

    // 2. 원래 낚시하던 맵의 플레이어 위치 (전투 끝나고 돌아왔을 때 그 자리에 세워주기 위함)
    public static Vector3 PlayerPos;

    // 3. (선택사항) 돌아올 맵의 이름
    public static string OriginSceneName;

    // 데이터를 초기화하고 싶을 때 사용
    public static void ResetData()
    {
        SelectedFish = null;
        PlayerPos = Vector3.zero;
        OriginSceneName = "";
    }
}