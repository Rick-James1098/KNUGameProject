using UnityEngine;

public class ConchBackFishMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.0f; // 이동 속도

    [Header("Spawn Position Settings")]
    public float leftSpawnX = -12f;  // 화면 왼쪽 밖 시작 지점 (X좌표)
    public float rightSpawnX = 12f;  // 화면 오른쪽 밖 시작 지점 (X좌표)
    
    public float minY = -4f;         // Y축 랜덤 범위 최소값
    public float maxY = 4f;          // Y축 랜덤 범위 최대값

    // 내부 변수
    private int direction = 1; // 1: 오른쪽으로 이동, -1: 왼쪽으로 이동

    void Start()
    {
        // 게임 시작 시 왼쪽 -> 오른쪽으로 출발하도록 설정
        SetupLeftToRight();
    }

    void Update()
    {
        // 1. 현재 방향으로 계속 이동
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);

        // 2. 오른쪽 끝(목표점)에 도달했는지 확인 (왼쪽 -> 오른쪽 이동 중일 때)
        if (direction == 1 && transform.position.x >= rightSpawnX)
        {
            // 도착했으면 다음엔 오른쪽 -> 왼쪽으로 가도록 설정
            SetupRightToLeft();
        }
        // 3. 왼쪽 끝(목표점)에 도달했는지 확인 (오른쪽 -> 왼쪽 이동 중일 때)
        else if (direction == -1 && transform.position.x <= leftSpawnX)
        {
            // 도착했으면 다음엔 왼쪽 -> 오른쪽으로 가도록 설정
            SetupLeftToRight();
        }
    }

    // 왼쪽에서 시작해서 오른쪽으로 가는 세팅
    void SetupLeftToRight()
    {
        direction = 1; // 이동 방향: 오른쪽 (+)
        
        // 위치 재설정 (X는 왼쪽 끝, Y는 랜덤)
        float randomY = Random.Range(minY, maxY);
        transform.position = new Vector3(leftSpawnX, randomY, transform.position.z);

        // 이미지 좌우 반전 처리 (오른쪽을 보게 함)
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x); // X 스케일을 음수로 (보통 이러면 오른쪽 봄)
        transform.localScale = scale;
    }

    // 오른쪽에서 시작해서 왼쪽으로 가는 세팅
    void SetupRightToLeft()
    {
        direction = -1; // 이동 방향: 왼쪽 (-)

        // 위치 재설정 (X는 오른쪽 끝, Y는 랜덤)
        float randomY = Random.Range(minY, maxY);
        transform.position = new Vector3(rightSpawnX, randomY, transform.position.z);

        // 이미지 좌우 반전 처리 (왼쪽을 보게 함)
        Vector3 scale = transform.localScale;
        scale.x = -Mathf.Abs(scale.x); // X 스케일을 양수로
        transform.localScale = scale;
    }
}
