using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject fishPrefab; // 생성할 물고기 프리팹 (Project 창에서 드래그)
    
    // 생성 범위 (최소 좌표 ~ 최대 좌표)
    public Vector2 minPosition = new Vector2(-26.6f, -2f);
    public Vector2 maxPosition = new Vector2(-32f, -1f);

    public GameObject SpawnFish()
    {
        // 1. X좌표와 Y좌표를 범위 내에서 랜덤으로 뽑기
        float randomX = Random.Range(minPosition.x, maxPosition.x);
        float randomY = Random.Range(minPosition.y, maxPosition.y);

        // 2. 뽑은 좌표로 벡터 생성
        Vector3 spawnPos = new Vector3(randomX, randomY, 0f);

        // 3. 물고기 생성 (Instantiate)
        GameObject fish = Instantiate(fishPrefab, spawnPos, Quaternion.identity);
        return fish;
    }

    // 에디터에서 범위를 눈으로 보여주는 기능 (게임 실행 안 해도 보임)
    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan; // 옥색 박스
        
        // 범위의 중심과 크기 계산
        Vector3 center = (minPosition + maxPosition) / 2;
        Vector3 size = maxPosition - minPosition;

        // 와이어 프레임 박스 그리기
        Gizmos.DrawWireCube(center, size);
    }*/
}
