using UnityEngine;

public class FishMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2.0f;  // 이동 속도
    public float moveRange = 2.0f;  // 좌우 이동 범위 (얼마나 넓게 움직일지)

    private Vector3 startPos;       // 시작 위치 저장
    private bool isStopped = false; // 멈춤 상태 체크

    void Start()
    {
        // 처음 배치된 위치를 기준점으로 잡습니다.
        startPos = transform.position;
    }

    void Update()
    {
        // 멈춰있거나 게임이 아니면 움직이지 않음
        if (isStopped) return;

        // Sin 함수를 이용해 부드럽게 좌우 왕복 이동
        // (Time.time * 속도) 값을 Sin에 넣어 -1 ~ 1 사이 값을 만들고, 범위(range)를 곱함
        float newX = startPos.x + Mathf.Sin(Time.time * moveSpeed) * moveRange;

        // 새로운 위치 적용 (Y, Z는 유지)
        transform.position = new Vector3(newX, startPos.y, startPos.z);
        
       
        float direction = Mathf.Cos(Time.time * moveSpeed);
        if (direction > 0) transform.localScale = new Vector3(-1, 1, 1); // 오른쪽 (이미지 방향에 따라 조절)
        else transform.localScale = new Vector3(1, 1, 1); // 왼쪽
    }

    // 외부(게이지 매니저)에서 호출할 함수
    public void StopMoving()
    {
        isStopped = true;
    }
}
