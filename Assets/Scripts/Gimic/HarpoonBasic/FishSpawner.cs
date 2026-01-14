using UnityEngine;
using System.Collections;

public class FishSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject fish;
    public PolygonCollider2D movementArea;
    public PolygonCollider2D spawnArea;
    public bool isOvered = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {   
        SpawnFish();
    }

    void SpawnFish() 
    {
        Vector3 startPos = GetRandomPointInCollider(spawnArea);
        startPos.z = 0;

        GameObject currentFish = Instantiate(fish, startPos, Quaternion.identity);

        // 3. 물고기에게 "너는 movementArea에서 놀아라"라고 알려줌
        FishShadow fishScript = currentFish.GetComponent<FishShadow>();
        if (fishScript != null)
        {
            // spawnArea가 아니라 movementArea를 넘겨줍니다 (중요!)
            fishScript.Setup(movementArea);
        }

    }

    Vector3 GetRandomPointInCollider(PolygonCollider2D collider)
    {
        if (collider == null) 
        {
            Debug.LogWarning("Spawn Area가 연결되지 않았습니다! (0,0)에서 생성합니다.");
            return Vector3.zero;
        }

        Vector2 randomPoint = Vector2.zero;
        int attempts = 0;

        // 최대 50번 시도하여 콜라이더 내부의 점을 찾음
        while (attempts < 50)
        {
            // 콜라이더의 사각형 경계(Bounds) 내에서 랜덤 좌표 추출
            float x = Random.Range(collider.bounds.min.x, collider.bounds.max.x);
            float y = Random.Range(collider.bounds.min.y, collider.bounds.max.y);
            Vector2 p = new Vector2(x, y);

            // 추출한 점이 실제로 콜라이더 도형 안에 있는지 검사 (OverlapPoint)
            if (collider.OverlapPoint(p))
            {
                return p; // 유효한 점이면 반환
            }
            attempts++;
        }

        // 50번 시도해도 실패하면 그냥 콜라이더의 중심점 반환
        return collider.bounds.center;
    }
}
